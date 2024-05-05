using System;
using System.Linq;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Domain;
using iSchool.Domain.Enum;
using System.Collections.Generic;
using iSchool.Infrastructure;
using System.Data.SqlClient;

namespace iSchool.Application.Service
{
    public class AddSchoolCommandHandler : IRequestHandler<AddSchoolCommand, Guid?>
    {
        private readonly IRepository<School> _schoolRepository;
        private readonly IRepository<GeneralTag> _generalTagRepository;
        private readonly IRepository<GeneralTagBind> _generalTagBindRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITagRepository _tagRepository;
        private readonly ISchoolExtReposiory _schoolExtReposiory;
        private readonly ILog _log;
        private readonly IMediator _mediator;
        private readonly IServiceProvider _sp;

        public AddSchoolCommandHandler(IRepository<School> schoolRepository,
            IRepository<GeneralTag> generalTagRepository,
            IRepository<GeneralTagBind> generalTagBinkRepository,
            IUnitOfWork unitOfWork, ILog log,
            ITagRepository tagRepository, ISchoolExtReposiory schoolExtReposiory,
            IMediator mediator, IServiceProvider sp)
        {
            _schoolRepository = schoolRepository;
            _generalTagRepository = generalTagRepository;
            _generalTagBindRepository = generalTagBinkRepository;
            _unitOfWork = unitOfWork;
            _tagRepository = tagRepository;
            _schoolExtReposiory = schoolExtReposiory;
            _log = log;
            _mediator = mediator;
            _sp = sp;
        }

        public Task<Guid?> Handle(AddSchoolCommand request, CancellationToken cancellationToken)
        {
            Guid? id = null;
            //先判断tag是否存在
            var tags = string.IsNullOrEmpty(request.Tags) ?
                new string[0] : request.Tags.Split(",");
            //存在的tag list
            var existTags = !string.IsNullOrEmpty(request.Tags) ?
                _generalTagRepository.GetInArray(tags, "name").ToList() :
                new System.Collections.Generic.List<GeneralTag>();

            float completion = 0;
            if (request.Status == 2)
            {
                var extList = _schoolExtReposiory.GetSimpleExt(request.Sid);
                if (extList.Count > 0)
                    completion = (float)extList.Sum(p => p.Completion) / extList.Count();
            }

            School school = null;
            try
            {
                _unitOfWork.BeginTransaction();
                //新增
                if (request.Status == 1)
                {
                    school = new School
                    {
                        Id = request.Sid,
                        Intro = request.Intro,
                        Name = request.Name,
                        Name_e = request.EName,
                        Show = true,
                        Logo = request.Logo,
                        Status = (byte)SchoolStatus.Edit,
                        Website = request.WebSite,
                        IsValid = true,
                        CreateTime = DateTime.Now,
                        ModifyDateTime = DateTime.Now,
                        Creator = request.UserId,
                        Modifier = request.UserId,
                        Completion = completion,
                        EduSysType = request.EduSysType,
                    };
                    _schoolRepository.Insert(school);
                }
                //修改
                else if (request.Status == 2)
                {
                    //修改学校信息
                    school = _schoolRepository.GetIsValid(p => p.Id == request.Sid);
                    if (school.Status == (byte)SchoolStatus.InAudit)
                    {
                        throw new CustomResponseException("学校已提交审核！", 4000);
                    }
                    if (!new byte?[] { (byte)SchoolStatus.Edit, (byte)SchoolStatus.Failed, (byte)SchoolStatus.Initial }.Contains(school.Status))
                    {
                        throw new CustomResponseException("当前学校状态无法修改！");
                    }
                    if (school != null)
                    {
                        //如果当前学校未认领
                        if (school.Status == (byte)SchoolStatus.Initial)
                        {
                            school.Creator = request.UserId;
                            school.CreateTime = DateTime.Now;
                            school.Status = (byte)SchoolStatus.Edit;
                        }
                        school.Intro = request.Intro?.Trim();
                        school.Name = request.Name;
                        school.Name_e = request.EName;
                        school.Logo = request.Logo;
                        school.Website = request.WebSite;
                        school.Modifier = request.UserId;
                        school.ModifyDateTime = DateTime.Now;
                        school.Completion = completion;
                        school.EduSysType = request.EduSysType;
                        _schoolRepository.Update(school);

                        //删除当前学校的所有标签绑定
                        _tagRepository.DeleteTagByDataId(request.Sid, 2);
                    }
                    else
                    {
                        throw new CustomResponseException("学校不存在");
                    }
                }

                //学校标签管理
                if (tags.Count() > 0)
                {                 
                    foreach (var tag in tags)
                    {
                        var existTag = existTags.FirstOrDefault(p => p.Name == tag);
                        if (existTag != null)
                        {
                            //已有标签
                            GeneralTagBind bind = new GeneralTagBind
                            {
                                DataId = request.Sid,
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
                                DataId = request.Sid,
                                TagId = tagItem.Id,
                                Ms = false,
                                DataType = 2//v3学校
                            };
                            _generalTagBindRepository.Insert(bind);
                        }
                    }
                }

                id = request.Sid;
                _unitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                _log.Warn(ex);  
                _unitOfWork.Rollback();

                if (ex is SqlException sqlex)
                {
                    // select * from [master].[sys].[messages] where text like '%重复%'
                    if (sqlex.Number.In(2601, 2627))
                        ex = new CustomResponseException(ex.Message, 1111);
                }
                throw ex;
            }

            SimpleQueue.Default.EnqueueThenRunOnChildScope(
                (sp, e) => _mediator.Publish(e),
                new SchoolUpdatedEvent { Sid = request.Sid, UserId = request.UserId, SchoolStatus = (SchoolStatus)school.Status }
            );
            return Task.FromResult(id);

        }
    }
}
