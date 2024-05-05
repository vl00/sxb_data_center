using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;

namespace iSchool.Application.Service
{
    public class AddSchoolExtCommandHandler : IRequestHandler<AddSchoolExtCommand, HttpResponse<object>>
    {
        private readonly IRepository<School> _schoolRepository;
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork;
        private readonly IRepository<SchoolExtension> _schoolextensionRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IRepository<GeneralTagBind> _generalTagBindRepository;
        private readonly IRepository<GeneralTag> _generalTagRepository;
        private readonly ILog _log;
        private readonly IMediator _mediator;

        public AddSchoolExtCommandHandler(IRepository<School> schoolRepository, IMapper mapper,
            IUnitOfWork unitOfWork, IRepository<SchoolExtension> schoolextensionRepository,
            ITagRepository tagRepository, IRepository<GeneralTagBind> generalTagBindRepository,
            IRepository<GeneralTag> generalTagRepository, ILog log, IMediator mediator)
        {
            _schoolRepository = schoolRepository;
            _mapper = mapper;
            _unitOfWork = (UnitOfWork)unitOfWork;
            _schoolextensionRepository = schoolextensionRepository;
            _tagRepository = tagRepository;
            _generalTagBindRepository = generalTagBindRepository;
            _generalTagRepository = generalTagRepository;
            _log = log;
            _mediator = mediator;
        }

        public Task<HttpResponse<object>> Handle(AddSchoolExtCommand request, CancellationToken cancellationToken)
        {
            //先判断tag是否存在
            var tags = string.IsNullOrEmpty(request.Dto.Tags) ?
                new string[0] : request.Dto.Tags.Split(",");
            //存在的tag list
            var existTags = !string.IsNullOrEmpty(request.Dto.Tags) ?
                _generalTagRepository.GetInArray(tags, "name").ToList() :
                new System.Collections.Generic.List<GeneralTag>();

            var extId = request.Dto.ExtId ?? Guid.NewGuid();
            var school = _schoolRepository.GetIsValid(p => p.Id == request.Dto.Sid);
            if (school == null)
                return Task.FromResult(new HttpResponse<object> { State = 400, Message = "学校不存在" });
            if (school.Status.In((byte)SchoolStatus.Success, (byte)SchoolStatus.InAudit))
                return Task.FromResult(new HttpResponse<object> { State = 400, Message = "学校当前状态无法修改" });

            if (request.Dto.ClaimedAmapEid == Guid.Empty)
            {
                return Task.FromResult(new HttpResponse<object> { State = 400, Message = "无效的高德id" });
            }

            try
            {
                _unitOfWork.BeginTransaction();

                SchoolExtension schoolExtension = null;
                if (request.Operation == DataOperation.Insert)
                {
                    //添加
                    schoolExtension = _mapper.Map<SchoolExtension>(request.Dto);
                    schoolExtension.AllowEdit = false;
                    schoolExtension.Id = extId;
                    schoolExtension.CreateTime = DateTime.Now;
                    schoolExtension.ModifyDateTime = DateTime.Now;
                    schoolExtension.Creator = request.Dto.UserId;
                    schoolExtension.Modifier = request.Dto.UserId;
                    //添加学校分部
                    _schoolextensionRepository.Insert(schoolExtension);
                }
                else
                {
                    //修改
                    schoolExtension = _schoolextensionRepository.GetIsValid(p =>
                         p.Id == request.Dto.ExtId.Value && p.Sid == request.Dto.Sid);

                    if (schoolExtension == null)
                        return Task.FromResult(new HttpResponse<object> { State = 400, Message = "当前分部不存在" });

                    schoolExtension.Name = request.Dto.Name;
                    schoolExtension.NickName = request.Dto.NickName;
                    if (schoolExtension.AllowEdit == true)
                    {
                        schoolExtension.SchFtype = request.Dto.SchFtype;
                        schoolExtension.Type = request.Dto.Type;
                        schoolExtension.Grade = request.Dto.Grade;
                        schoolExtension.Chinese = request.Dto.Chinese;
                        schoolExtension.Diglossia = request.Dto.Diglossia;
                        schoolExtension.Discount = request.Dto.Discount;
                    }
                    schoolExtension.AllowEdit = false;
                    schoolExtension.Source = JsonSerializationHelper.Serialize(request.Dto.Source.ToList());
                    //来源上传的附件
                    schoolExtension.SourceAttachments = request.Dto.SourceAttachments;
                    schoolExtension.Weixin = request.Dto.Weixin;
                    if (schoolExtension.Creator.In(Guid.Empty, null))
                    {
                        schoolExtension.Creator = request.Dto.UserId;
                        schoolExtension.CreateTime = DateTime.Now;
                    } 
                    schoolExtension.ModifyDateTime = DateTime.Now;
                    schoolExtension.Modifier = request.Dto.UserId;
                    schoolExtension.ExtIntro = request.Dto.ExtIntro;
                    //修改学部
                    _schoolextensionRepository.Update(schoolExtension);
                    //删除学部tag
                    _tagRepository.DeleteTagByDataId(extId, 2);
                }

                schoolExtension.ClaimedAmapEid = request.Dto.ClaimedAmapEid;

                var _i = _unitOfWork.DbConnection.Execute($@"
update dbo.{nameof(SchoolExtension)} set ClaimedAmapEid=@ClaimedAmapEid where Id=@Id 
{"and not exists(select 1 from dbo.SchoolExtension e where e.ClaimedAmapEid=@ClaimedAmapEid and e.Id<>@Id and e.IsValid=1)".If(schoolExtension.ClaimedAmapEid != null)}
                    ", schoolExtension, _unitOfWork.DbTransaction);
                if (_i < 1)
                {
                    throw new FnResultException(1111, $"已存在的高德id '{schoolExtension.ClaimedAmapEid}'");
                }

                //tag 操作
                if (tags.Count() > 0)
                {
                    //学校标签管理
                    foreach (var tag in tags)
                    {
                        var existTag = existTags.FirstOrDefault(p => p.Name == tag);
                        if (existTag != null)
                        {
                            //已有标签
                            GeneralTagBind bind = new GeneralTagBind
                            {
                                DataId = request.Dto.ExtId,
                                DataType = 2,
                                Ms = false,
                                TagId = existTag.Id
                            };
                            _generalTagBindRepository.Insert(bind);
                        }
                        else
                        {
                            //新标签
                            GeneralTag tagItem = new GeneralTag
                            {
                                Id = Guid.NewGuid(),
                                Name = tag
                            };
                            _generalTagRepository.Insert(tagItem);

                            GeneralTagBind bind = new GeneralTagBind()
                            {
                                DataId = request.Dto.ExtId,
                                TagId = tagItem.Id,
                                Ms = false,
                                DataType = 2//v3学校
                            };
                            _generalTagBindRepository.Insert(bind);
                        }
                    }
                }
                _unitOfWork.CommitChanges();

                // 旧学校数据 添加/修改学部 就是认领学校
                if (school.Status == (byte)SchoolStatus.Initial || school.Creator == Guid.Empty)
                {
                    _mediator.Send(new ClaimSchoolCommand { Sid = school.Id, UserId = request.Dto.UserId.Value }).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                var errmsg = "操作失败！";
                if (ex is FnResultException fex)
                {
                    errmsg = fex.Message;
                    if (fex.Code == 1111) _log.Warn(ex);
                }
                else _log.Error(ex);

                return Task.FromResult(new HttpResponse<object> { State = 400, Message = errmsg });
            }

            _mediator.Publish(new SchoolUpdatedEvent { Sid = school.Id, Eid = extId, SchoolStatus = (SchoolStatus)school.Status }).Wait();
            return Task.FromResult(new HttpResponse<object> { State = 200, Result = extId });
        }
    }
}
