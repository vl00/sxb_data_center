using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Application.Service
{
    public class DelExtensionCommandHandler : IRequestHandler<DelExtensionCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISchoolExtReposiory _schoolextensionRepository;
        private readonly IRepository<School> _schoolRepository;
        private readonly ILog _log;
        private readonly IMediator _mediator;
        private readonly IServiceProvider _sp;

        public DelExtensionCommandHandler(IUnitOfWork unitOfWork, IMediator mediator, IServiceProvider sp,
            ISchoolExtReposiory schoolextensionRepository, ILog log, IRepository<School> schoolRepository)
        {
            _unitOfWork = unitOfWork;
            _schoolextensionRepository = schoolextensionRepository;
            _schoolRepository = schoolRepository;
            _log = log;
            _mediator = mediator;
            _sp = sp;
        }

        public Task<bool> Handle(DelExtensionCommand request, CancellationToken cancellationToken)
        {
            var school = _schoolRepository.GetIsValid(p => p.Id == request.Sid);
            if (!request.IsAll)
            {
                if (school?.Creator != request.UserId && school.Status != (byte)SchoolStatus.Initial)
                    throw new CustomResponseException("不允许删除学校分部！", 500);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                if (school.Creator == Guid.Empty && school.Status == (byte)SchoolStatus.Initial)
                {
                    school.Status = (byte)SchoolStatus.Edit;
                    school.Creator = request.UserId;
                    school.Modifier = request.UserId;
                    school.ModifyDateTime = DateTime.Now;
                    _schoolRepository.Update(school);
                }
                foreach (var item in request.ExtensionIds)
                {
                    _schoolextensionRepository.DelSchoolExt(request.Sid, item, request.UserId);
                }
                _unitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _unitOfWork.Rollback();
                return Task.FromResult(false);
            }

            foreach (var ext in request.ExtensionIds)
            {
                SimpleQueue.Default.EnqueueThenRunOnChildScope(_sp,
                    (sp, e) => _mediator.Publish(e),
                    new SchoolUpdatedEvent { Sid = request.Sid, Eid = ext, IsDeleted = true, UserId = request.UserId }
                );
            }
            return Task.FromResult(true);
        }
    }
}
