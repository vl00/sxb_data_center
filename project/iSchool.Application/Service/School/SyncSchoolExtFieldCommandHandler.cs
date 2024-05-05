using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using iSchool.Domain.Enum;
using System.Linq;
using AutoMapper;
using iSchool.Application.ViewModels;

namespace iSchool.Application.Service
{
    public class SyncSchoolExtFieldCommandHandler : IRequestHandler<SyncSchoolExtFieldCommand, HttpResponse<string>>
    {
        private readonly IRepository<SchoolExtension> _schoolExtensionRepository;
        private ILog _log;
        private UnitOfWork UnitOfWork { get; set; }
        private readonly IRepository<SchoolVideo> _schoolvideoRepository;
        private readonly IMapper _mapper;


        public SyncSchoolExtFieldCommandHandler(IRepository<SchoolExtension> schoolExtensionRepository,
            ILog log, IMapper mapper,
            IUnitOfWork unitOfWork, IRepository<SchoolVideo> schoolvideoRepository)
        {
            _schoolExtensionRepository = schoolExtensionRepository;
            _log = log;
            _schoolvideoRepository = schoolvideoRepository;
            _mapper = mapper;
            UnitOfWork = (UnitOfWork)unitOfWork;
        }

        public Task<HttpResponse<string>> Handle(SyncSchoolExtFieldCommand request, CancellationToken cancellationToken)
        {
            var sid = request.Sid;


            //先将当前学校所有分部查询出来
            var exts = _schoolExtensionRepository.GetAll(p => p.IsValid == true && p.Sid == request.Sid );
            var extIds = exts.Select(p => p.Id).ToList();

            //如果更改的是学校视频
            if (request.IsVideo == true)
            {
                //整体逻辑，先删除(item.IsValid = false;)对应分部对应类型的所有视频 ，然后根据前端传入的视频路径，统一批量新增  

                var strVideoUrls = request.Fields[0].Value.ToString();
                var listVideoUrls = new List<string>();
                if (strVideoUrls!= "\"[]\"")
                {
                    listVideoUrls=JsonSerializationHelper.JSONToObject<List<string>>(strVideoUrls);
                }
               
                //var extVideos = UnitOfWork.DbConnection.Query<SchoolVideo>($"select * from {request.fieldItem.DBtable} where IsValid=@IsValid and eid = @Eid and Type=@Type and VideoUrl in @VideoUrls",
                //new { IsValid = true, request.Eid, Type= request.fieldItem.VideoType, VideoUrls=listVideoUrls });
                //var extVideos = _schoolvideoRepository.GetAll(p => p.Eid == request.Eid && p.IsValid == true && p.Type == request.fieldItem.VideoType && listVideoUrls.Contains(p.VideoUrl)).ToList();
                var videos = _schoolvideoRepository.GetAll(p => extIds.Contains(p.Eid) && p.IsValid == true && p.Type == (byte)request.fieldItem.VideoType);
                
                try
                {
                    //批量删除
                    foreach (var item in videos)
                    {
                        item.IsValid = false;
                    }

                    UnitOfWork.BeginTransaction();

                    //批量添加
                    if (listVideoUrls != null && listVideoUrls.Count > 0)
                    {
                        
                        var newVideos = new List<SchoolVideo>();
                        foreach (var item in extIds)
                        {
                            foreach (var videoUrl in listVideoUrls)
                            {
                                SchoolVideo schoolVideo = new SchoolVideo()
                                {
                                    //Id = Guid.NewGuid(),
                                    //Eid = item,
                                    //Modifier = request.UserId,
                                    //ModifyDateTime = DateTime.Now,
                                    //IsValid = true,
                                    //Completion = video.Completion,
                                    //Creator = video.Creator,
                                    //CreateTime = video.CreateTime,
                                    //IsOutSide = video.IsOutSide,
                                    //Type = video.Type,
                                    //Sort = video.Sort,
                                    //VideoDesc = video.VideoDesc,
                                    //VideoUrl = video.VideoUrl
                                    Id = Guid.NewGuid(),
                                    Eid =item,
                                    Completion = 1,
                                    IsOutSide = false,
                                    Sort = 99,
                                    //Type = (byte)VideoType.Profile,
                                    Type = (byte)(VideoType)request.fieldItem.VideoType,
                                    VideoUrl = videoUrl,
                                    CreateTime = DateTime.Now,
                                    Creator = request.UserId,
                                    ModifyDateTime = DateTime.Now,
                                    Modifier = request.UserId,
                                    IsValid = true,
                                    VideoDesc = ""
                                };
                                newVideos.Add(schoolVideo);
                            }
                        }
                        _schoolvideoRepository.BatchInsert(newVideos);
                    }
                   

                   
                    _schoolvideoRepository.BatchUpdate(videos.ToList());
                    
                    UnitOfWork.CommitChanges();
                    return Task.FromResult(new HttpResponse<string> { State = 200 });
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                    UnitOfWork.Rollback();
                    return Task.FromResult(new HttpResponse<string> { State = 500 });
                }
            }
            else
            {
                //先将所有学部当前table eid list查询出来
                //如果存在update 否则  insert
                var selectExtData = UnitOfWork.DbConnection.Query<Guid>($"select eid from {request.fieldItem.DBtable} where IsValid=@IsValid and eid in @Eids",
                    new { IsValid = true, Eids = extIds });

                //同步到其他分布是否有数据
                try
                {
                    UnitOfWork.BeginTransaction();
                    foreach (var eid in extIds)
                    {
                        var selectData = selectExtData.Count(p => p == eid);
                        if (selectData == 0)
                        {
                            //不存在则新增操作
                            //生成新增sql语句
                            var sql = new StringBuilder();
                            sql.Append($"insert {request.fieldItem.DBtable} (");
                            sql.Append($"id,eid,CreateTime,Creator,ModifyDateTime,Modifier,IsValid,Completion,{string.Join(',', request.Fields.Select(p => p.Key))}");
                            sql.Append($") values (@id,@eid,@time,@userid,@time,@userid,1,0");
                            foreach (var field in request.Fields)
                            {
                                sql.Append(",");
                                if (field.Value == null && request.Data[field.Key] == null)
                                {
                                    sql.Append("null");
                                    continue;
                                }
                                else if (field.Value == null && request.Data[field.Key] != null)
                                {
                                    //同步数据
                                    field.Value = request.Data[field.Key];
                                }

                                if (new int[] { (int)FielDataType.Double, (int)FielDataType.Int }.Contains(request.fieldItem.DataType))
                                {
                                    sql.Append(field.Value.ToString());
                                }
                                else if ((int)FielDataType.Bool == request.fieldItem.DataType)
                                {
                                    string check = field.Value.Equals("true") ? 1.ToString() : field.Value.Equals("false") ? 0.ToString() : "null";
                                    sql.Append(check);
                                }
                                else
                                {
                                    sql.Append($"'{field.Value}'");
                                }
                            }
                            sql.Append(")");
                            UnitOfWork.DbConnection.Execute(sql.ToString(), new { id = Guid.NewGuid(), eid = eid, time = DateTime.Now, userid = request.UserId }, UnitOfWork.DbTransaction);
                        }
                        else
                        {
                            //修改操作
                            //修改相应的字段
                            foreach (var item in request.Fields)
                            {
                                var sql = new StringBuilder();
                                if (item.Value != null)
                                {
                                    //根据数据类型生成对应的sql语句
                                    //修改同步
                                    if (new int[] { (int)FielDataType.Double, (int)FielDataType.Int }.Contains(request.fieldItem.DataType))
                                    {
                                        sql.Append($"update {request.Table} set {item.Key}={item.Value},ModifyDateTime=@time,Modifier=@modifier where IsValid=1 and eid=@extid");
                                    }
                                    else if ((int)FielDataType.Bool == request.fieldItem.DataType)
                                    {
                                        string check = item.Value.Equals("true") ? 1.ToString() : item.Value.Equals("false") ? 0.ToString() : "null";
                                        sql.Append($"update {request.Table} set {item.Key}={check},ModifyDateTime=@time,Modifier=@modifier  where IsValid=1 and eid=@extid");
                                    }
                                    else
                                    {
                                        sql.Append($"update {request.Table} set {item.Key}='{item.Value.ToString()}',ModifyDateTime=@time,Modifier=@modifier  where IsValid=1 and eid=@extid");
                                    }
                                }
                                else
                                {
                                    if(item.Key.Equals("lodging") || item.Key.Equals("sdextern"))
                                    {
                                        string check = item.Value==null? "null" : item.Value.Equals("true") ? 1.ToString() : item.Value.Equals("false") ? 0.ToString() : "null";
                                        sql.Append($"update {request.Table} set {item.Key}={check},ModifyDateTime=@time,Modifier=@modifier  where IsValid=1 and eid=@extid");

                                    }
                                    else
                                    {
                                        //仅仅只是同步
                                        sql.Append($"update {request.Table} set {item.Key}=(select top 1 {item.Key} from {request.Table} where eid=@Eid AND IsValid=1) ,ModifyDateTime=@time,Modifier=@modifier  where IsValid=1 and eid=@extid");

                                    }
                                }
                                UnitOfWork.DbConnection.Execute(sql.ToString(), new { Eid = request.Eid, time = DateTime.Now, modifier = request.UserId, extid = eid }, UnitOfWork.DbTransaction);
                            }
                        }
                    }
                    UnitOfWork.CommitChanges();
                    return Task.FromResult(new HttpResponse<string> { State = 200 });
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                    UnitOfWork.Rollback();
                    return Task.FromResult(new HttpResponse<string> { State = 500 });
                }
            }

        }
    }
}

