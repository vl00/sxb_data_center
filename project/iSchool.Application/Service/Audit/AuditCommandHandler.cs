using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using iSchool.Domain.Enum;
using System.Text;
using iSchool.Domain.Modles;

namespace iSchool.Application.Service.Audit
{
    public class AuditCommandHandler : IRequestHandler<AuditCommand>
    {
        IServiceProvider sp;
        UnitOfWork unitOfWork;
        IRepository<SchoolAudit> repo_sgAudit;
        IRepository<SchoolAuditorInfo> repo_sgAuditInfo;
        IMediator mediator;
        AuditOption auditOption;

        public AuditCommandHandler(IMediator mediator, IUnitOfWork unitOfWork, IServiceProvider sp,
            IRepository<SchoolAudit> repo_sgAudit,
            IRepository<SchoolAuditorInfo> repo_sgAuditInfo,
            AuditOption auditOption)
        {
            this.sp = sp;
            this.mediator = mediator;
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.auditOption = auditOption;
            this.repo_sgAudit = repo_sgAudit;
            this.repo_sgAuditInfo = repo_sgAuditInfo;
        }

        public async Task<Unit> Handle(AuditCommand req, CancellationToken cancellationToken)
        {
            SchoolAudit audit = null;
            string sql = null;
            bool? isnewAudit = null;

            lock (auditOption.Sync)
            {
                audit = repo_sgAudit.GetAll(_ => _.IsValid && _.Id == req.Id).FirstOrDefault();
                if (audit == null) throw new FnResultException(AuditOption.Errcode_NotExists, $"不存在的审核 id={req.Id}");
            }

            do
            {
                if (audit.Status.In((byte)SchoolAuditStatus.Success, (byte)SchoolAuditStatus.Failed))
                {
                    throw new FnResultException(AuditOption.Errcode_Audited, $"已审核过id={req.Id}");
                }
                if (audit.Status == (byte)SchoolAuditStatus.Auditing)
                {
                    if ((DateTime.Now - audit.ModifyDateTime).TotalMinutes > auditOption.AtSyncMin)
                    {
                        audit.Status = (byte)SchoolAuditStatus.InAudit;
                    }
                    else
                    {
                        throw new FnResultException(AuditOption.Errcode_Syncing, $"审核数据提交同步中 id={audit.Id}");
                    }
                }
                if (audit.Status == (byte)SchoolAuditStatus.InAudit)
                {
                    // 当前审核人获取这个审核单后在XX分钟内,如因意外情况离开可重进
                    // 审核单被a获取后超过了XX分钟, 视为未审核, b可获取
                    if ((DateTime.Now - audit.ModifyDateTime).TotalMinutes <= auditOption.AtAuditMin)
                    {
                        if (audit.Modifier == req.AuditorId)
                            break;
                        else
                            throw new FnResultException(AuditOption.Errcode_Handing, $"已正在处理中");
                    }
                }
            }
            while (false);

            if (!req.CanDoAll)
            {
                sql = @"
if not exists(select 1 from dbo.SchoolAuditorInfo where Sid=@Sid) begin
    select 1;
end else if exists(select 1 from dbo.SchoolAuditorInfo where Sid=@Sid and AuditorUid=@AuditorId) begin
    select 0;
end else begin
    select null;
end
";
                isnewAudit = unitOfWork.DbConnection.ExecuteScalar<bool?>(sql, new { audit.Sid, req.AuditorId });

                if (isnewAudit == null) throw new FnResultException(AuditOption.Errcode_IsForAnother, $"该学校由其他审核员负责");
            }

            // 状态由 UnAudit|InAudit (含中间状态过期重进) 更新到 Auditing
            lock (auditOption.Sync)
            {
                var prevModifyDateTime = audit.ModifyDateTime;
                audit.Status = (byte)SchoolAuditStatus.Auditing;
                audit.IsValid = true;
                audit.AuditMessage = req.Fail ? req.Msg : "";
                audit.Creator = audit.Modifier = req.AuditorId;
                audit.ModifyDateTime = DateTime.Now;

                sql = $@"
update dbo.SchoolAudit set [Status]=@Status,IsValid=@IsValid,Modifier=@Modifier,ModifyDateTime=@ModifyDateTime,Creator=@Creator
where Id=@Id and [Status] in ({(byte)SchoolAuditStatus.UnAudit},{(byte)SchoolAuditStatus.InAudit},{(byte)SchoolAuditStatus.Auditing}) 
    and ModifyDateTime=@prevModifyDateTime
";
                var i = unitOfWork.DbConnection.Execute(sql, new DynamicParameters(audit)
                    .Set("@prevModifyDateTime", prevModifyDateTime));

                if (i <= 0)
                    throw new FnResultException(AuditOption.Errcode_Syncing, $"审核数据提交同步中");
            }

            _ = mediator.Send(new CheckInAuditExpireCommand());

            ///
            /// 审核成功/失败
            ///

            var years4tb = Enumerable.Empty<int>();            
            if (!req.Fail)
            {
                years4tb = await unitOfWork.DbConnection.QueryAsync<int>($@"
select s.[value] from YearExtField y join SchoolExtension e on e.id=y.eid and e.IsValid=1
cross apply String_Split(y.years,',') s
where e.sid='{audit.Sid}' and s.[value]<>''
group by s.[value]
");
            }

            Exception ex_datasync = null;
            try
            {                 
                unitOfWork.BeginTransaction();

                #region School
                sql = @"
update dbo.School set status=@status,ModifyDateTime=getdate(),Modifier=@AuditorId,IsValid=1 where id=@Sid
";
                unitOfWork.DbConnection.Execute(sql, new
                {
                    status = (byte)(req.Fail ? SchoolStatus.Failed : SchoolStatus.Success),
                    req.AuditorId,
                    audit.Sid,
                }, unitOfWork.DbTransaction);
                #endregion

                // 审核成功, 需要更新online相关表
                if (!req.Fail)
                {
                    #region OnlineSchool
                    sql = @"
if not exists(select 1 from dbo.OnlineSchool where id=@Sid) begin
    insert dbo.OnlineSchool(id,name,name_e,website,intro,show,status,logo,CreateTime,Creator,ModifyDateTime,Modifier,IsValid,EduSysType)
    select id,name,name_e,website,intro,1,status,logo,CreateTime,Creator,getdate(),@AuditorId,1,EduSysType from dbo.School 
    where id=@Sid ;
end else begin
    update o set o.name=s.name,o.name_e=s.name_e,o.website=s.website,o.intro=s.intro,o.show=1,o.status=s.status,o.logo=s.logo,
        o.Creator=s.Creator,o.CreateTime=s.CreateTime,o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1,o.EduSysType=s.EduSysType
    from dbo.OnlineSchool o, dbo.School s
    where o.id=s.id and o.id=@Sid ;
end
";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);
                    #endregion
                                        
                    #region OnlineSchoolExtension
                    sql = @"
                            update dbo.OnlineSchoolExtension set ModifyDateTime=getdate(),Modifier=@AuditorId,IsValid=0 where sid=@Sid ;
                            
                            insert dbo.OnlineSchoolExtension(id,sid,grade,type,discount,diglossia,chinese,name,nickname,SchFtype,source,weixin,CreateTime,Creator,ModifyDateTime,Modifier,IsValid,ClaimedAmapEid,extintro)
                            select e.id,e.sid,e.grade,e.type,e.discount,e.diglossia,e.chinese,e.name,e.nickname,e.SchFtype,e.source,e.weixin,e.CreateTime,e.Creator,getdate(),@AuditorId,1,e.ClaimedAmapEid,extintro from dbo.SchoolExtension e
                            where not exists(select 1 from dbo.OnlineSchoolExtension o where o.id=e.id) and e.sid=@Sid and e.IsValid=1 ;
                            
                            update o set o.sid=e.sid,o.grade=e.grade,o.type=e.type,o.discount=e.discount,o.diglossia=e.diglossia,o.chinese=e.chinese,o.name=e.name,o.nickname=e.nickname,o.SchFtype=e.SchFtype,o.source=e.source,
                            o.Creator=e.Creator,o.CreateTime=e.CreateTime,o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1,o.AllowEdit=0,o.ClaimedAmapEid=e.ClaimedAmapEid,o.extintro=e.extintro
                            from dbo.OnlineSchoolExtension o, dbo.SchoolExtension e
                            where o.sid=@Sid and o.id=e.id and e.IsValid=1 ; 
                            ";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);
                    #endregion

                    /** sync sql for OnlineXXXX
update o set o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=0 
from dbo.Online{{ext_tableName}} o, dbo.OnlineSchoolExtension e0
where o.eid=e0.id and e0.sid=@Sid ;

-- insert for not exists in Online table
insert dbo.Online{{ext_tableName}}( {{ online-ext_tableName-fileds }} )
select e.{{ ext_tableName-fileds }},@AuditorId,getdate(),1 --Modifier,ModifyDateTime,IsValid
from dbo.SchoolExtAlgTeQuality e
inner join dbo.SchoolExtension e0 on e0.id=e.eid and e0.IsValid=1 and e.IsValid=1
	where not exists(select 1 from dbo.Online{{ext_tableName}} o where o.id=e.id) and e0.sid=@Sid ;   
                    
-- update for exists in Online table
update o set {{ o.XXX=e.XXX }},o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1
	from dbo.Online{{ext_tableName}} o, dbo.{{ext_tableName}} e
	where o.id=e.id and exists(select 1 from dbo.SchoolExtension e0 where e0.id=e.eid and e0.sid=@Sid and e0.IsValid=1 and e.IsValid=1) ; 	
                    //*/

                    // step2 - step7

                    #region OnlineSchoolExtContent
                    sql = @"
                            update o set o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=0 
                            from dbo.OnlineSchoolExtContent o, dbo.OnlineSchoolExtension e0
                            where o.eid=e0.id and e0.sid=@Sid ;
                            
                            insert dbo.OnlineSchoolExtContent(id,eid,province,city,area,address,latitude,longitude,LatLong,tel,lodging,studentcount,teachercount,tsPercent,authentication,abroad,canteen,meal,characteristic,project,SeniorTea,characteristicTea,foreignTeaCount,foreignTea,creationdate,acreage,openhours,calendar,range,counterpart,afterclass,CreateTime,Creator,ModifyDateTime,Modifier,IsValid,Completion,sdextern,HasSchoolBus)
                            select e.id,e.eid,e.province,e.city,e.area,e.address,e.latitude,e.longitude,e.LatLong,e.tel,e.lodging,e.studentcount,e.teachercount,e.tsPercent,e.authentication,e.abroad,e.canteen,e.meal,e.characteristic,e.project,e.SeniorTea,e.characteristicTea,e.foreignTeaCount,e.foreignTea,e.creationdate,e.acreage,e.openhours,e.calendar,e.range,e.counterpart,e.afterclass,e.CreateTime,e.Creator,getdate(),@AuditorId,1,e.Completion,e.sdextern,e.HasSchoolBus
                            from dbo.SchoolExtContent e inner join dbo.SchoolExtension e0 on e0.id=e.eid and e0.IsValid=1 and e.IsValid=1
                            where not exists(select 1 from dbo.OnlineSchoolExtContent o where o.id=e.id) and e0.sid=@Sid ;
                            
                            update o set o.eid=e.eid,o.province=e.province,o.city=e.city,o.area=e.area,o.address=e.address,o.latitude=e.latitude,o.longitude=e.longitude,o.LatLong=e.LatLong,o.tel=e.tel,o.lodging=e.lodging,o.studentcount=e.studentcount,o.teachercount=e.teachercount,o.tsPercent=e.tsPercent,o.authentication=e.authentication,o.abroad=e.abroad,o.canteen=e.canteen,o.meal=e.meal,o.characteristic=e.characteristic,o.project=e.project,o.SeniorTea=e.SeniorTea,o.characteristicTea=e.characteristicTea,o.foreignTeaCount=e.foreignTeaCount,o.foreignTea=e.foreignTea,o.creationdate=e.creationdate,o.acreage=e.acreage,o.openhours=e.openhours,o.calendar=e.calendar,o.range=e.range,o.counterpart=e.counterpart,o.afterclass=e.afterclass,o.CreateTime=e.CreateTime,o.Creator=e.Creator,o.Completion=e.Completion,o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1,o.sdextern=e.sdextern,o.Allocation=e.Allocation,o.HasSchoolBus=e.HasSchoolBus
                            from dbo.OnlineSchoolExtContent o, dbo.SchoolExtContent e
                            where o.id=e.id and exists(select 1 from dbo.SchoolExtension e0 where e0.id=e.eid and e0.sid=@Sid and e0.IsValid=1 and e.IsValid=1) ; 
                            ";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);
                    #endregion

                    #region OnlineSchoolVideo
                    sql = @"
update o set o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=0 
from dbo.OnlineSchoolVideo o, dbo.OnlineSchoolExtension e0
where o.eid=e0.id and e0.sid=@Sid ;

insert dbo.OnlineSchoolVideo(id,eid,videoUrl,videoDesc,cover,isOutSide,type,sort,CreateTime,Creator,ModifyDateTime,Modifier,IsValid,Completion)
select e.id,e.eid,e.videoUrl,e.videoDesc,e.cover,e.isOutSide,e.type,e.sort,e.CreateTime,e.Creator,getdate(),@AuditorId,1,e.Completion from dbo.SchoolVideo e
inner join dbo.SchoolExtension e0 on e0.id=e.eid and e0.IsValid=1 and e.IsValid=1
	where not exists(select 1 from dbo.OnlineSchoolVideo o where o.id=e.id) and e0.sid=@Sid ;
	
update o set o.eid=e.eid,o.videoUrl=e.videoUrl,o.videoDesc=e.videoDesc,o.cover=e.cover,o.isOutSide=e.isOutSide,o.type=e.type,o.sort=e.sort,o.CreateTime=e.CreateTime,o.Creator=e.Creator,o.Completion=e.Completion,
		o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1
	from dbo.OnlineSchoolVideo o, dbo.SchoolVideo e
	where o.id=e.id and exists(select 1 from dbo.SchoolExtension e0 where e0.id=e.eid and e0.sid=@Sid and e0.IsValid=1 and e.IsValid=1) ; 
";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);
                    #endregion

                    #region OnlineSchoolExtRecruit
                    sql = @"
update o set o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=0 
from dbo.OnlineSchoolExtRecruit o, dbo.OnlineSchoolExtension e0
where o.eid=e0.id and e0.sid=@Sid ;

insert dbo.OnlineSchoolExtRecruit(id,eid,age,maxage,count,target,proportion,date,point,data,contact,subjects,pastexam,scholarship,CreateTime,Creator,ModifyDateTime,Modifier,IsValid,Completion)
select e.id,e.eid,e.age,e.maxage,e.count,e.target,e.proportion,e.date,e.point,e.data,e.contact,e.subjects,e.pastexam,e.scholarship,e.CreateTime,e.Creator,getdate(),@AuditorId,1,e.Completion 
from dbo.SchoolExtRecruit e
inner join dbo.SchoolExtension e0 on e0.id=e.eid and e0.IsValid=1 and e.IsValid=1
	where not exists(select 1 from dbo.OnlineSchoolExtRecruit o where o.id=e.id) and e0.sid=@Sid ;
	
update o set o.eid=e.eid,o.age=e.age,o.maxage=e.maxage,o.count=e.count,o.target=e.target,o.proportion=e.proportion,o.date=e.date,o.point=e.point,o.data=e.data,o.contact=e.contact,o.subjects=e.subjects,o.pastexam=e.pastexam,o.scholarship=e.scholarship,o.CreateTime=e.CreateTime,o.Creator=e.Creator,o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1,o.Completion=e.Completion
	from dbo.OnlineSchoolExtRecruit o, dbo.SchoolExtRecruit e
	where o.id=e.id and exists(select 1 from dbo.SchoolExtension e0 where e0.id=e.eid and e0.sid=@Sid and e0.IsValid=1 and e.IsValid=1) ; 
";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);
                    #endregion

                    #region OnlineSchoolExtCourse
                    sql = @"
update o set o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=0 
from dbo.OnlineSchoolExtCourse o, dbo.OnlineSchoolExtension e0
where o.eid=e0.id and e0.sid=@Sid ;

insert dbo.OnlineSchoolExtCourse(id,eid,courses,characteristic,authentication,CreateTime,Creator,ModifyDateTime,Modifier,IsValid,Completion)
select e.id,e.eid,e.courses,e.characteristic,e.authentication,e.CreateTime,e.Creator,getdate(),@AuditorId,1,e.Completion 
from dbo.SchoolExtCourse e
inner join dbo.SchoolExtension e0 on e0.id=e.eid and e0.IsValid=1 and e.IsValid=1
	where not exists(select 1 from dbo.OnlineSchoolExtCourse o where o.id=e.id) and e0.sid=@Sid ;
	
update o set o.eid=e.eid,o.courses=e.courses,o.characteristic=e.characteristic,o.authentication=e.authentication,o.CreateTime=e.CreateTime,o.Creator=e.Creator,o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1,o.Completion=e.Completion
	from dbo.OnlineSchoolExtCourse o, dbo.SchoolExtCourse e
	where o.id=e.id and exists(select 1 from dbo.SchoolExtension e0 where e0.id=e.eid and e0.sid=@Sid and e0.IsValid=1 and e.IsValid=1) ; 
";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);
                    #endregion

                    #region OnlineSchoolExtCharge
                    sql = @"
update o set o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=0 
from dbo.OnlineSchoolExtCharge o, dbo.OnlineSchoolExtension e0
where o.eid=e0.id and e0.sid=@Sid ;

insert dbo.OnlineSchoolExtCharge(id,eid,applicationfee,tuition,otherfee,CreateTime,Creator,ModifyDateTime,Modifier,IsValid,Completion)
select e.id,e.eid,e.applicationfee,e.tuition,e.otherfee,e.CreateTime,e.Creator,getdate(),@AuditorId,1,e.Completion 
from dbo.SchoolExtCharge e
inner join dbo.SchoolExtension e0 on e0.id=e.eid and e0.IsValid=1 and e.IsValid=1
	where not exists(select 1 from dbo.OnlineSchoolExtCharge o where o.id=e.id) and e0.sid=@Sid ;

update o set o.eid=e.eid,o.applicationfee=e.applicationfee,o.tuition=e.tuition,o.otherfee=e.otherfee,o.CreateTime=e.CreateTime,o.Creator=e.Creator,o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1,o.Completion=e.Completion
	from dbo.OnlineSchoolExtCharge o, dbo.SchoolExtCharge e
	where o.id=e.id and exists(select 1 from dbo.SchoolExtension e0 where e0.id=e.eid and e0.sid=@Sid and e0.IsValid=1 and e.IsValid=1) ; 
";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);
                    #endregion

                    #region OnlineSchoolExtQuality
                    sql = @"
update o set o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=0 
from dbo.OnlineSchoolExtQuality o, dbo.OnlineSchoolExtension e0
where o.eid=e0.id and e0.sid=@Sid ;

insert dbo.OnlineSchoolExtQuality(id,eid,principal,videos,teacher,schoolhonor,studenthonor,CreateTime,Creator,ModifyDateTime,Modifier,IsValid,Completion)
select e.id,e.eid,e.principal,e.videos,e.teacher,e.schoolhonor,e.studenthonor,e.CreateTime,e.Creator,getdate(),@AuditorId,1,e.Completion 
from dbo.SchoolExtQuality e
inner join dbo.SchoolExtension e0 on e0.id=e.eid and e0.IsValid=1 and e.IsValid=1
	where not exists(select 1 from dbo.OnlineSchoolExtQuality o where o.id=e.id) and e0.sid=@Sid ;

update o set o.eid=e.eid,o.principal=e.principal,o.videos=e.videos,o.teacher=e.teacher,o.schoolhonor=e.schoolhonor,o.studenthonor=e.studenthonor,o.CreateTime=e.CreateTime,o.Creator=e.Creator,o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1,o.Completion=e.Completion
	from dbo.OnlineSchoolExtQuality o, dbo.SchoolExtQuality e
	where o.id=e.id and exists(select 1 from dbo.SchoolExtension e0 where e0.id=e.eid and e0.sid=@Sid and e0.IsValid=1 and e.IsValid=1) ; 
";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);
                    #endregion

                    #region OnlineSchoolExtlife
                    sql = @"
update o set o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=0 
from dbo.OnlineSchoolExtlife o, dbo.OnlineSchoolExtension e0
where o.eid=e0.id and e0.sid=@Sid ;

insert dbo.OnlineSchoolExtlife(id,eid,hardware,community,timetables,schedule,diagram,CreateTime,Creator,ModifyDateTime,Modifier,IsValid,Completion)
select e.id,e.eid,e.hardware,e.community,e.timetables,e.schedule,e.diagram,e.CreateTime,e.Creator,getdate(),@AuditorId,1,e.Completion
from dbo.SchoolExtlife e
inner join dbo.SchoolExtension e0 on e0.id=e.eid and e0.IsValid=1 and e.IsValid=1
	where not exists(select 1 from dbo.OnlineSchoolExtlife o where o.id=e.id) and e0.sid=@Sid ;
	
update o set o.eid=e.eid,o.hardware=e.hardware,o.community=e.community,o.timetables=e.timetables,o.schedule=e.schedule,o.diagram=e.diagram,o.CreateTime=e.CreateTime,o.Creator=e.Creator,o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1,o.Completion=e.Completion
	from dbo.OnlineSchoolExtlife o, dbo.SchoolExtlife e
	where o.id=e.id and exists(select 1 from dbo.SchoolExtension e0 where e0.id=e.eid and e0.sid=@Sid and e0.IsValid=1 and e.IsValid=1) ; 
";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);
                    #endregion

                    #region OnlineSchoolImage
                    sql = @"
update o set o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=0 
from dbo.OnlineSchoolImage o, dbo.OnlineSchoolExtension e0
where o.eid=e0.id and e0.sid=@Sid ;

insert dbo.OnlineSchoolImage(id,eid,url,surl,imageDesc,type,sort,CreateTime,Creator,ModifyDateTime,Modifier,IsValid)
select e.id,e.eid,e.url,e.surl,e.imageDesc,e.type,e.sort,e.CreateTime,e.Creator,getdate(),@AuditorId,1 from dbo.SchoolImage e
inner join dbo.SchoolExtension e0 on e0.id=e.eid and e0.IsValid=1 and e.IsValid=1
	where not exists(select 1 from dbo.OnlineSchoolImage o where o.id=e.id) and e0.sid=@Sid ;
	
update o set o.eid=e.eid,o.url=e.url,o.surl=e.surl,o.imageDesc=e.imageDesc,o.type=e.type,o.sort=e.sort,o.CreateTime=e.CreateTime,o.Creator=e.Creator,
		o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1
	from dbo.OnlineSchoolImage o, dbo.SchoolImage e
	where o.id=e.id and exists(select 1 from dbo.SchoolExtension e0 where e0.id=e.eid and e0.sid=@Sid and e0.IsValid=1 and e.IsValid=1) ; 
";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);

                    #endregion

                    #region OnlineSchoolAchievement
                    sql = @"
update o set o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=0 
from dbo.OnlineSchoolAchievement o, dbo.OnlineSchoolExtension e0
where o.extid=e0.id and e0.sid=@Sid ;

insert dbo.OnlineSchoolAchievement(Id,extId,year,schoolId,count,CreateTime,Creator,ModifyDateTime,Modifier,IsValid,Completion)
select e.Id,e.extId,e.year,e.schoolId,e.count,e.CreateTime,e.Creator,getdate(),@AuditorId,1,e.Completion from dbo.SchoolAchievement e
inner join dbo.SchoolExtension e0 on e0.id=e.extid and e0.IsValid=1 and e.IsValid=1
	where not exists(select 1 from dbo.OnlineSchoolAchievement o where o.id=e.id) and e0.sid=@Sid ;
	
update o set o.extId=e.extId,o.year=e.year,o.schoolId=e.schoolId,o.count=e.count,o.CreateTime=e.CreateTime,o.Creator=e.Creator,o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1,o.Completion=e.Completion
	from dbo.OnlineSchoolAchievement o, dbo.SchoolAchievement e
	where o.id=e.id and exists(select 1 from dbo.SchoolExtension e0 where e0.id=e.extid and e0.sid=@Sid and e0.IsValid=1 and e.IsValid=1) ; 
";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);
                    #endregion

                    #region OnlineHighSchoolAchievement
                    sql = @"
update o set o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=0 
from dbo.OnlineHighSchoolAchievement o, dbo.OnlineSchoolExtension e0
where o.extid=e0.id and e0.sid=@Sid ;

insert dbo.OnlineHighSchoolAchievement(Id,extId,year,keyundergraduate,undergraduate,count,fractionaline,CreateTime,Creator,ModifyDateTime,Modifier,IsValid,Completion)
select e.Id,e.extId,e.year,e.keyundergraduate,e.undergraduate,e.count,e.fractionaline,e.CreateTime,e.Creator,getdate(),@AuditorId,1,e.Completion from dbo.HighSchoolAchievement e
inner join dbo.SchoolExtension e0 on e0.id=e.extid and e0.IsValid=1 and e.IsValid=1
	where not exists(select 1 from dbo.OnlineHighSchoolAchievement o where o.id=e.id) and e0.sid=@Sid ;
	
update o set o.extId=e.extId,o.year=e.year,o.keyundergraduate=e.keyundergraduate,o.undergraduate=e.undergraduate,o.count=e.count,o.fractionaline=e.fractionaline,o.CreateTime=e.CreateTime,o.Creator=e.Creator,o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1,o.Completion=e.Completion
	from dbo.OnlineHighSchoolAchievement o, dbo.HighSchoolAchievement e
	where o.id=e.id and exists(select 1 from dbo.SchoolExtension e0 where e0.id=e.extid and e0.sid=@Sid and e0.IsValid=1 and e.IsValid=1) ; 
";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);
                    #endregion

                    #region OnlineMiddleSchoolAchievement
                    sql = @"
update o set o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=0 
from dbo.OnlineMiddleSchoolAchievement o, dbo.OnlineSchoolExtension e0
where o.extid=e0.id and e0.sid=@Sid ;

insert dbo.OnlineMiddleSchoolAchievement(Id,extId,year,keyrate,average,highest,ratio,CreateTime,Creator,ModifyDateTime,Modifier,IsValid,Completion)
select e.Id,e.extId,e.year,e.keyrate,e.average,e.highest,e.ratio,e.CreateTime,e.Creator,getdate(),@AuditorId,1,e.Completion from dbo.MiddleSchoolAchievement e
inner join dbo.SchoolExtension e0 on e0.id=e.extid and e0.IsValid=1 and e.IsValid=1
	where not exists(select 1 from dbo.OnlineMiddleSchoolAchievement o where o.id=e.id) and e0.sid=@Sid ;
	
update o set o.extId=e.extId,o.year=e.year,o.keyrate=e.keyrate,o.average=e.average,o.highest=e.highest,o.ratio=e.ratio,o.CreateTime=e.CreateTime,o.Creator=e.Creator,o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1,o.Completion=e.Completion
	from dbo.OnlineMiddleSchoolAchievement o, dbo.MiddleSchoolAchievement e
	where o.id=e.id and exists(select 1 from dbo.SchoolExtension e0 where e0.id=e.extid and e0.sid=@Sid and e0.IsValid=1 and e.IsValid=1) ; 
";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);
                    #endregion

                    #region OnlinePrimarySchoolAchievement
                    sql = @"
update o set o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=0 
from dbo.OnlinePrimarySchoolAchievement o, dbo.OnlineSchoolExtension e0
where o.extid=e0.id and e0.sid=@Sid ;

insert dbo.OnlinePrimarySchoolAchievement(Id,extId,year,link,CreateTime,Createor,ModifyDateTime,Modifier,IsValid,Completion)
select e.Id,e.extId,e.year,e.link,e.CreateTime,e.Createor,getdate(),@AuditorId,1,e.Completion from dbo.PrimarySchoolAchievement e
inner join dbo.SchoolExtension e0 on e0.id=e.extid and e0.IsValid=1 and e.IsValid=1
	where not exists(select 1 from dbo.OnlinePrimarySchoolAchievement o where o.id=e.id) and e0.sid=@Sid ;
	
update o set o.Id=e.Id,o.extId=e.extId,o.year=e.year,o.link=e.link,o.CreateTime=e.CreateTime,o.Createor=e.Createor,o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1,o.Completion=e.Completion
	from dbo.OnlinePrimarySchoolAchievement o, dbo.PrimarySchoolAchievement e
	where o.id=e.id and exists(select 1 from dbo.SchoolExtension e0 where e0.id=e.extid and e0.sid=@Sid and e0.IsValid=1 and e.IsValid=1) ; 
";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);
                    #endregion

                    #region OnlineKindergartenAchievement
                    sql = @"
update o set o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=0 
from dbo.OnlineKindergartenAchievement o, dbo.OnlineSchoolExtension e0
where o.extid=e0.id and e0.sid=@Sid ;

insert dbo.OnlineKindergartenAchievement(Id,extId,year,link,CreateTime,Creator,ModifyDateTime,Modifier,IsValid,Completion)
select e.Id,e.extId,e.year,e.link,e.CreateTime,e.Creator,getdate(),@AuditorId,1,e.Completion from dbo.KindergartenAchievement e
inner join dbo.SchoolExtension e0 on e0.id=e.extid and e0.IsValid=1 and e.IsValid=1
	where not exists(select 1 from dbo.OnlineKindergartenAchievement o where o.id=e.id) and e0.sid=@Sid ;
	
update o set o.Id=e.Id,o.extId=e.extId,o.year=e.year,o.link=e.link,o.CreateTime=e.CreateTime,o.Creator=e.Creator,o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1,o.Completion=e.Completion
	from dbo.OnlineKindergartenAchievement o, dbo.KindergartenAchievement e
	where o.id=e.id and exists(select 1 from dbo.SchoolExtension e0 where e0.id=e.extid and e0.sid=@Sid and e0.IsValid=1 and e.IsValid=1) ; 
";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);
                    #endregion                    

                    // alg1 - alg3

                    #region OnlineSchoolExtAlgTeQuality
                    sql = @"
update o set o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=0 
from dbo.OnlineSchoolExtAlgTeQuality o, dbo.OnlineSchoolExtension e0
where o.eid=e0.id and e0.sid=@Sid ;

insert dbo.OnlineSchoolExtAlgTeQuality(id,eid,TeacherCount,FgnTeacherCount,UndergduateOverCount,GduateOverCount,TeacherHonor,Creator,CreateTime,Modifier,ModifyDateTime,IsValid)
select e.id,e.eid,e.TeacherCount,e.FgnTeacherCount,e.UndergduateOverCount,e.GduateOverCount,e.TeacherHonor,e.Creator,e.CreateTime,@AuditorId,getdate(),1
from dbo.SchoolExtAlgTeQuality e
inner join dbo.SchoolExtension e0 on e0.id=e.eid and e0.IsValid=1 and e.IsValid=1
	where not exists(select 1 from dbo.OnlineSchoolExtAlgTeQuality o where o.id=e.id) and e0.sid=@Sid ;

update o set o.eid=e.eid,o.TeacherCount=e.TeacherCount,o.FgnTeacherCount=e.FgnTeacherCount,o.UndergduateOverCount=e.UndergduateOverCount,o.GduateOverCount=e.GduateOverCount,o.TeacherHonor=e.TeacherHonor,o.Creator=e.Creator,o.CreateTime=e.CreateTime,o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1
	from dbo.OnlineSchoolExtAlgTeQuality o, dbo.SchoolExtAlgTeQuality e
	where o.id=e.id and exists(select 1 from dbo.SchoolExtension e0 where e0.id=e.eid and e0.sid=@Sid and e0.IsValid=1 and e.IsValid=1) ; 	
";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);
                    #endregion

                    #region OnlineSchoolExtAlgCourseKind
                    sql = @"
update o set o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=0 
from dbo.OnlineSchoolExtAlgCourseKind o, dbo.OnlineSchoolExtension e0
where o.eid=e0.id and e0.sid=@Sid ;

insert dbo.OnlineSchoolExtAlgCourseKind(id,eid,SubjsJson,ArtsJson,SportsJson,ScienceJson,Creator,CreateTime,Modifier,ModifyDateTime,IsValid)
select e.id,e.eid,e.SubjsJson,e.ArtsJson,e.SportsJson,e.ScienceJson,e.Creator,e.CreateTime,@AuditorId,getdate(),1
from dbo.SchoolExtAlgCourseKind e
inner join dbo.SchoolExtension e0 on e0.id=e.eid and e0.IsValid=1 and e.IsValid=1
	where not exists(select 1 from dbo.OnlineSchoolExtAlgCourseKind o where o.id=e.id) and e0.sid=@Sid ;

update o set o.eid=e.eid,o.SubjsJson=e.SubjsJson,o.ArtsJson=e.ArtsJson,o.SportsJson=e.SportsJson,o.ScienceJson=e.ScienceJson,o.Creator=e.Creator,o.CreateTime=e.CreateTime,o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1
	from dbo.OnlineSchoolExtAlgCourseKind o, dbo.SchoolExtAlgCourseKind e
	where o.id=e.id and exists(select 1 from dbo.SchoolExtension e0 where e0.id=e.eid and e0.sid=@Sid and e0.IsValid=1 and e.IsValid=1) ; 	
";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);
                    #endregion

                    #region OnlineSchoolExtAlgHwf
                    sql = @"
update o set o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=0 
from dbo.OnlineSchoolExtAlgHwf o, dbo.OnlineSchoolExtension e0
where o.eid=e0.id and e0.sid=@Sid ;

insert dbo.OnlineSchoolExtAlgHwf(id,eid,Acreage,AcreageUnit,Inputamount,MoneyDiff,SchbusCount,HasSwimpool,SwimpoolWhere,SwimpoolTemperature,HasLodging,LodgingFacilities,LodgingAreaperp,LodgingPersionNum,LibyBookNum,LibyAreaperp,LibyBookper,HasPgd,PgdLength,HasCanteen,CanteenNum,CanteenAreaperp,CanteenHealthRate,HealthRoom,SteamRoom,Creator,CreateTime,Modifier,ModifyDateTime,IsValid)
select e.id,e.eid,e.Acreage,e.AcreageUnit,e.Inputamount,e.MoneyDiff,e.SchbusCount,e.HasSwimpool,e.SwimpoolWhere,e.SwimpoolTemperature,e.HasLodging,e.LodgingFacilities,e.LodgingAreaperp,e.LodgingPersionNum,e.LibyBookNum,e.LibyAreaperp,e.LibyBookper,e.HasPgd,e.PgdLength,e.HasCanteen,e.CanteenNum,e.CanteenAreaperp,e.CanteenHealthRate,e.HealthRoom,e.SteamRoom,e.Creator,e.CreateTime,@AuditorId,getdate(),1
from dbo.SchoolExtAlgHwf e
inner join dbo.SchoolExtension e0 on e0.id=e.eid and e0.IsValid=1 and e.IsValid=1
	where not exists(select 1 from dbo.OnlineSchoolExtAlgHwf o where o.id=e.id) and e0.sid=@Sid ;

update o set o.eid=e.eid,o.Acreage=e.Acreage,o.AcreageUnit=e.AcreageUnit,o.Inputamount=e.Inputamount,o.MoneyDiff=e.MoneyDiff,o.SchbusCount=e.SchbusCount,o.HasSwimpool=e.HasSwimpool,o.SwimpoolWhere=e.SwimpoolWhere,o.SwimpoolTemperature=e.SwimpoolTemperature,o.HasLodging=e.HasLodging,o.LodgingFacilities=e.LodgingFacilities,o.LodgingAreaperp=e.LodgingAreaperp,o.LodgingPersionNum=e.LodgingPersionNum,o.LibyBookNum=e.LibyBookNum,o.LibyAreaperp=e.LibyAreaperp,o.LibyBookper=e.LibyBookper,o.HasPgd=e.HasPgd,o.PgdLength=e.PgdLength,o.HasCanteen=e.HasCanteen,o.CanteenNum=e.CanteenNum,o.CanteenAreaperp=e.CanteenAreaperp,o.CanteenHealthRate=e.CanteenHealthRate,o.HealthRoom=e.HealthRoom,o.SteamRoom=e.SteamRoom,o.Creator=e.Creator,o.CreateTime=e.CreateTime,o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1
	from dbo.OnlineSchoolExtAlgHwf o, dbo.SchoolExtAlgHwf e
	where o.id=e.id and exists(select 1 from dbo.SchoolExtension e0 where e0.id=e.eid and e0.sid=@Sid and e0.IsValid=1 and e.IsValid=1) ; 	
";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);
                    #endregion

                    #region OnlineSchoolExtAlgAchievement
                    sql = @"
update o set o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=0 
from dbo.OnlineSchoolExtAlgAchievement o, dbo.OnlineSchoolExtension e0
where o.eid=e0.id and e0.sid=@Sid ;

insert dbo.OnlineSchoolExtAlgAchievement(id,eid,ExtamAvgscore,No1Count,CmstuCount,RecruitCount,Creator,CreateTime,Modifier,ModifyDateTime,IsValid)
select e.id,e.eid,e.ExtamAvgscore,e.No1Count,e.CmstuCount,e.RecruitCount,e.Creator,e.CreateTime,@AuditorId,getdate(),1
from dbo.SchoolExtAlgAchievement e
inner join dbo.SchoolExtension e0 on e0.id=e.eid and e0.IsValid=1 and e.IsValid=1
	where not exists(select 1 from dbo.OnlineSchoolExtAlgAchievement o where o.id=e.id) and e0.sid=@Sid ;

update o set o.eid=e.eid,o.ExtamAvgscore=e.ExtamAvgscore,o.No1Count=e.No1Count,o.CmstuCount=e.CmstuCount,o.RecruitCount=e.RecruitCount,o.Creator=e.Creator,o.CreateTime=e.CreateTime,o.ModifyDateTime=getdate(),o.Modifier=@AuditorId,o.IsValid=1
	from dbo.OnlineSchoolExtAlgAchievement o, dbo.SchoolExtAlgAchievement e
	where o.id=e.id and exists(select 1 from dbo.SchoolExtension e0 where e0.id=e.eid and e0.sid=@Sid and e0.IsValid=1 and e.IsValid=1) ; 	
";
                    unitOfWork.DbConnection.Execute(sql, new
                    {
                        req.AuditorId,
                        audit.Sid,
                    }, unitOfWork.DbTransaction);
                    #endregion

                    #region 年份字段
                    sql = @"
delete o from OnlineYearExtField o join SchoolExtension e on e.id=o.eid and e.IsValid=1 where e.sid=@Sid ;
insert OnlineYearExtField select y.* from YearExtField y join SchoolExtension e on e.id=y.eid where e.IsValid=1 and e.sid=@Sid ;
";
                    unitOfWork.DbConnection.Execute(sql, new { audit.Sid }, unitOfWork.DbTransaction);

                    foreach (var year in years4tb)
                    {
                        sql = $@"
IF OBJECT_ID('dbo.OnlineSchoolYearFieldContent_{year}',N'U') is not null 
BEGIN
update y set y.IsValid=0 from [OnlineSchoolYearFieldContent_{year}] y, schoolextension e where y.eid=e.id and e.sid=@Sid
;
merge into [OnlineSchoolYearFieldContent_{year}] y
using (select y.* from [SchoolYearFieldContent_{year}] y join schoolextension e with(nolock) on e.id=y.eid where e.IsValid=1 and e.sid=@Sid) y0 on y.id=y0.id
when not matched then
    insert (Id,year,eid,field,content,IsValid) values(y0.Id,y0.year,y0.eid,y0.field,y0.content,y0.IsValid)
when matched then
    update set y.year=y0.year,y.eid=y0.eid,y.field=y0.field,y.content=y0.content,y.IsValid=y0.IsValid
; END";
                        unitOfWork.DbConnection.Execute(sql, new { audit.Sid }, unitOfWork.DbTransaction, 1000 * 60);
                    }
                    #endregion
                }

                unitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                try { unitOfWork.Rollback(); } catch { }
                ex_datasync = ex;
            }

            if (!req.Fail && ex_datasync == null)
            {
                // 审核成功后同步有无校车......
                try
                {
                    sql = @"
                        begin try
	                        update e set e.HasSchoolBus=isnull(e2.HasSchoolBus,0)
	                        from OnlineSchoolExtension e,OnlineSchoolExtContent e2
	                        where e.id=e2.eid and e2.IsValid=1 and e.sid=@Sid
                        end try begin catch
	                        select 1
                        end catch
                        /*-- begin try
	                        update e set e.HasSchoolBus=isnull(e2.HasSchoolBus,0)
	                        from SchoolExtension e,OnlineSchoolExtContent e2
	                        where e.id=e2.eid and e2.IsValid=1 and e.sid=@Sid
                        end try begin catch
	                        select 1
                        end catch --*/
                    ";
                    unitOfWork.DbConnection.Execute(sql, new { audit.Sid });
                }
                catch { }
            }

            if (ex_datasync != null)
            {
                lock (auditOption.Sync)
                {
                    audit.Status = (byte)SchoolAuditStatus.InAudit;
                    audit.IsValid = true;
                    audit.Modifier = req.AuditorId;
                    //audit.ModifyDateTime = DateTime.Now;
                    repo_sgAudit.Update(audit);
                }

                throw ex_datasync;
            }

            ///
            /// 同步数据完成后, 变更审核单状态
            ///

            audit.Status = (byte)(req.Fail ? SchoolAuditStatus.Failed : SchoolAuditStatus.Success);
            //audit.AuditMessage = req.Fail ? req.Msg : "";
            audit.Modifier = req.AuditorId;
            audit.ModifyDateTime = DateTime.Now;
            audit.IsValid = true;

            var ainfo = new SchoolAuditorInfo();
            ainfo.Sid = audit.Sid;
            ainfo.AuditorUid = audit.Modifier;
            ainfo.Aid = audit.Id;
            ainfo.Status = audit.Status;
            ainfo.AuditTime = audit.ModifyDateTime;
            ainfo.IsValid = true;

            var error = await Retry_to_do(after_datasync_ok, 3, 1000);
            void after_datasync_ok()
            {
                lock (auditOption.Sync)
                {
                    try
                    {
                        unitOfWork.BeginTransaction();

                        //... //if (!req.CanDoAll || isnewAudit == true)
                        {
                            sql = $@"
                                    if not exists(select 1 from dbo.SchoolAuditorInfo where Sid=@Sid) begin
                                    insert dbo.[{nameof(SchoolAuditorInfo)}] ([{nameof(ainfo.Sid)}],[{nameof(ainfo.AuditorUid)}],[{nameof(ainfo.Aid)}],[{nameof(ainfo.Status)}],[{nameof(ainfo.AuditTime)}],[{nameof(ainfo.IsValid)}])     
                                    values(@Sid,@AuditorUid,@Aid,@Status,@AuditTime,@IsValid) ;
                                    end else begin
                                    update dbo.[{nameof(SchoolAuditorInfo)}] set [{nameof(ainfo.Sid)}]=@Sid,[{nameof(ainfo.AuditorUid)}]=@AuditorUid,[{nameof(ainfo.Aid)}]=@Aid,[{nameof(ainfo.Status)}]=@Status,[{nameof(ainfo.AuditTime)}]=@AuditTime,[{nameof(ainfo.IsValid)}]=@IsValid
                                    where Sid=@Sid and AuditorUid=@AuditorUid ;
                                    end
                                    ";
                            unitOfWork.DbConnection.Execute(sql, ainfo, unitOfWork.DbTransaction);
                        }

                        repo_sgAudit.Update(audit);

                        unitOfWork.CommitChanges();
                    }
                    catch (Exception ex)
                    {
                        try { unitOfWork.Rollback(); } catch { }
                        throw ex;
                    }
                }
            }

            if (error != null)
            {
                throw new FnResultException(AuditOption.Errcode_updateAuditStateAfterSync,
                    $"审核通过了但更新状态时失败:aid={audit.Id} ex={error.Message}", error);
            }

            SimpleQueue.Default.EnqueueThenRunOnChildScope(
                (_sp, o) => on_audit_completed(_sp.GetService<IMediator>(), (SchoolAudit)o, !req.Fail),
                audit
            );

            return Unit.Value;
        }

        static async Task on_audit_completed(IMediator mediator, SchoolAudit audit, bool auditOK)
        {
            if (auditOK) 
                await mediator.Publish(new OnlineSchools.SchoolOnlinedEvent { Aid = audit.Id, Sid = audit.Sid, T = audit.ModifyDateTime });

            await mediator.Publish(new SchoolUpdatedEvent { Sid = audit.Sid, UserId = audit.Modifier, Time = audit.ModifyDateTime, SchoolStatus = auditOK ? SchoolStatus.Success : SchoolStatus.Failed });    
        }

        async Task<Exception> Retry_to_do(Action func, int c, int ms)
        {
            c = c < 1 ? 1 : c;
            for (var i = 1; i <= c; i++)
            {
                try
                {
                    func();
                    return null;
                }
                catch (Exception ex)
                {
                    if (i == c) return ex;
                    else await Task.Delay(ms);
                }
            }
            return null;
        }
    }
}
