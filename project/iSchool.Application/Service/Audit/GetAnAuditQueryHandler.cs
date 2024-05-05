using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using iSchool.Infrastructure.Dapper;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;

namespace iSchool.Application.Service.Audit
{
    public class GetAnAuditQueryHandler : IRequestHandler<GetAnAuditQuery, AnAuditQueryResult>
    {
        UnitOfWork unitOfWork;
        IRepository<School> schoolRepository;
        IRepository<SchoolAudit> schoolAuditRepository;
        IMediator mediator;
        AuditOption auditOption;

        public GetAnAuditQueryHandler(IUnitOfWork unitOfWork, IMediator mediator,
            AuditOption auditOption,
            IRepository<School> schoolRepository, 
            IRepository<SchoolAudit> schoolAuditRepository)
        {
            this.auditOption = auditOption;
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.mediator = mediator;
            this.schoolRepository = schoolRepository;
            this.schoolAuditRepository = schoolAuditRepository;
        }

        public async Task<AnAuditQueryResult> Handle(GetAnAuditQuery req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            SchoolAudit audit = null;
            School school = null;
            string sql = null;
            bool? isnewAudit = null;

            lock (auditOption.Sync)
            {
                audit = schoolAuditRepository.Get(req.Id);
                if (audit == null) throw new FnResultException(AuditOption.Errcode_NotExists, $"无效的audit id={req.Id}");
            }

            school = schoolRepository.Get(audit.Sid);
            if (school == null) throw new Exception($"找不到对应的学校school id={audit.Sid}");

            do
            {
                if (req.IsRead) break;

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
                    /// 当前审核人获取这个审核单后在XX分钟内,如因意外情况离开可重进
                    /// 审核单被a获取后超过了XX分钟, 视为未审核, b可获取
                    if ((DateTime.Now - audit.ModifyDateTime).TotalMinutes <= auditOption.AtAuditMin)
                    {
                        if (audit.Modifier == req.AuditorId)
                            break;
                        else
                            throw new FnResultException(AuditOption.Errcode_Handing, $"已正在处理中");
                    }
                }

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

                // 状态由 UnAudit (含中间状态过期重进) 更新到 InAudit
                lock (auditOption.Sync)
                {
                    var prevModifyDateTime = audit.ModifyDateTime;
                    audit.Status = (byte)SchoolAuditStatus.InAudit;
                    audit.IsValid = true;
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
                        throw new FnResultException(AuditOption.Errcode_Handing, $"已正在处理中");
                }
            }
            while (false);
           
            //_ = mediator.Send(new CheckInAuditExpireCommand());

            var res = new AnAuditQueryResult();
            res.IsPreGet = req.IsPreGet;
            res.Id = audit.Id;
            res.SchoolId = audit.Sid;
            res.CurrAuditStatus = (SchoolAuditStatus)audit.Status;
            res.CurrAuditMessage = audit.AuditMessage;
            res.CurrAuditorId = audit.Modifier;
            res.SchoolName = school.Name;
            res.SchoolName_e = school.Name_e;
            res.Logo = school.Logo;
            res.WebSite = school.Website;
            res.Info = school.Intro;
            res.EduSysType = school.EduSysType;

            if (res.IsPreGet) return res;

            sql = @"
select t.name from dbo.GeneralTag t 
inner join GeneralTagBind b on t.id=b.tagID
where b.dataID=@schoolId and b.dataType=2
";
            res.Tags = unitOfWork.DbConnection.Query<string>(sql, new { schoolId = school.Id }).ToArray();

            sql = @"
select e.id as ExtId,e.name as ExtName,e.Completion
--((isnull(c1.Completion,0)+isnull(c2.Completion,0)+isnull(c3.Completion,0)+isnull(c4.Completion,0)+isnull(c5.Completion,0)+isnull(c6.Completion,0))/6)as Completion
from dbo.SchoolExtension e
--left join dbo.SchoolExtContent c1 on e.id=c1.eid and c1.IsValid=1 
--left join dbo.SchoolExtRecruit c2 on e.id=c2.eid and c2.IsValid=1 
--left join dbo.SchoolExtCourse c3 on e.id=c3.eid and c3.IsValid=1 
--left join dbo.SchoolExtCharge c4 on e.id=c4.eid and c4.IsValid=1 
--left join dbo.SchoolExtQuality c5 on e.id=c5.eid and c5.IsValid=1 
--left join dbo.SchoolExtlife c6 on e.id=c6.eid and c6.IsValid=1 
where e.IsValid=1 
and e.sid=@schoolId
";
            res.Exts = unitOfWork.DbConnection.Query<AuditSchoolExt>(sql, new { schoolId = school.Id }).ToArray();

            return res;
        }
    }
}
