using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;

namespace iSchool.Application.Service
{
    public class ExaminationSchoolCommandHandler : IRequestHandler<ExaminationSchoolCommand, HttpResponse<string>>
    {
        private readonly IRepository<School> _schoolRepository;
        private readonly ISchoolExtReposiory _schoolExtReposiory;
        private readonly IRepository<SchoolAudit> _schoolAuditRepository;
        private UnitOfWork UnitOfWork { get; set; }
        private readonly ILog _log;
        private readonly IMediator _mediator;
        private readonly IServiceProvider _sp;

        public ExaminationSchoolCommandHandler(
            IRepository<School> schoolRepository, ISchoolExtReposiory schoolExtReposiory, IRepository<SchoolAudit> schoolAuditRepository,
            IUnitOfWork unitOfWork, ILog log,
            IMediator mediator, IServiceProvider sp)
        {
            _schoolRepository = schoolRepository;
            _schoolExtReposiory = schoolExtReposiory;
            _schoolAuditRepository = schoolAuditRepository;
            UnitOfWork = (UnitOfWork)unitOfWork;
            _log = log;
            _mediator = mediator;
            _sp = sp;
        }

        public Task<HttpResponse<string>> Handle(ExaminationSchoolCommand request, CancellationToken cancellationToken)
        {
            byte? editStatus = (byte?)SchoolStatus.Edit, failedStatus = (byte?)SchoolStatus.Failed;

            //学校状态是编辑中或者失败的，才能提交审核
            var school = _schoolRepository.GetIsValid(p => (p.Status == editStatus
            || p.Status == failedStatus) && p.Id == request.Sid);

            if (school == null)
                return Task.FromResult(new HttpResponse<string> { State = 404, Message = "未找到该学校或未认领该学校" });
            if (!request.IsAll)
            {
                if (school.Creator != request.UserId)
                    return Task.FromResult(new HttpResponse<string> { State = 500, Message = "不能提交其他人的学校" });
            }

            //var extList = _schoolExtReposiory.GetSimpleExt(request.Sid);
            //var completion = extList != null ? (float)extList.Sum(p => p.Completion) / extList.Count() : 0;

            var oldAudit = UnitOfWork.DbConnection.QueryFirstOrDefault<SchoolAudit>($@"
select top 1 * from SchoolAudit where sid=@Sid and isvalid=1 and status in({SchoolAuditStatus.Success.ToInt()},{SchoolAuditStatus.Failed.ToInt()})
order by createtime desc
                    ", request);

            var auditCreator = oldAudit == null ? Guid.Empty : oldAudit.Creator;
            var auditModifier = oldAudit == null ? Guid.Empty : oldAudit.Modifier;

            try
            {
                UnitOfWork.BeginTransaction();
                school.Status = (byte)SchoolStatus.InAudit;
                school.Modifier = request.UserId;
                school.ModifyDateTime = DateTime.Now;
                //school.Completion = completion;
                _schoolRepository.Update(school);
                SchoolAudit audit = new SchoolAudit
                {
                    Id = Guid.NewGuid(),
                    CreateTime = DateTime.Now,
                    Creator = auditCreator,
                    Modifier = auditModifier,
                    IsValid = true,
                    Sid = request.Sid,
                    AuditMessage = "",
                    ModifyDateTime = DateTime.Now,
                    Status = (byte)SchoolAuditStatus.UnAudit
                };
                _schoolAuditRepository.Insert(audit);
                UnitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                UnitOfWork.Rollback();
                return Task.FromResult(new HttpResponse<string> { State = 500, Message = "提交审核失败！" });
            }

            SimpleQueue.Default.EnqueueThenRunOnChildScope(_sp,
                    (sp, e) => _mediator.Publish(e),
                    new SchoolUpdatedEvent { Sid = request.Sid, UserId = request.UserId, SchoolStatus = (SchoolStatus)school.Status }
                );

            return Task.FromResult(new HttpResponse<string> { State = 200 });
        }
    }
}
