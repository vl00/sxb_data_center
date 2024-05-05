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
using Microsoft.Extensions.DependencyInjection;

namespace iSchool.Application.Service
{
    /// <summary>
    /// 
    /// </summary>
    public class DelSchoolCommadHandler : IRequestHandler<DelSchoolCommad, HttpResponse<string>>
    {

        private UnitOfWork UnitOfWork { get; set; }
        private readonly IRepository<School> _schoolRepository;
        private readonly IRepository<SchoolExtension> _schoolExtensionRepository;
        private readonly IRepository<Domain.OnlineSchool> _onlineSchoolRepository;
        private readonly IRepository<OnlineSchoolExtension> _onlineSchoolExtensionRepository;
        private readonly ILog _log;

        public DelSchoolCommadHandler(IUnitOfWork unitOfWork, 
            IRepository<School> schoolRepository, IRepository<SchoolExtension> schoolExtensionRepository,
            IRepository<Domain.OnlineSchool> onlineSchoolRepository, IRepository<OnlineSchoolExtension> onlineSchoolExtensionRepository,
            ILog log)
        {
            UnitOfWork = (UnitOfWork)unitOfWork;
            _schoolRepository = schoolRepository;
            _schoolExtensionRepository = schoolExtensionRepository;
            _log = log;
            this._onlineSchoolRepository = onlineSchoolRepository;
            this._onlineSchoolExtensionRepository = onlineSchoolExtensionRepository;
        }


        public Task<HttpResponse<string>> Handle(DelSchoolCommad request, CancellationToken cancellationToken)
        {
            var school = _schoolRepository.GetIsValid(p => p.Id == request.SId);
            if (school == null)
            {
                return Task.FromResult(new HttpResponse<string> { State = 500, Message = "学校已删除" });
            }
            if (!new List<byte> { (byte)SchoolStatus.Edit, (byte)SchoolStatus.Failed, (byte)SchoolStatus.Initial }.Contains(school.Status.Value))
            {
                return Task.FromResult(new HttpResponse<string> { State = 500, Message = "当前学校状态不能删除" });
            }
            if (!request.IsAll)
            {
                if (school.Creator != request.UserId)
                {
                    return Task.FromResult(new HttpResponse<string> { State = 500, Message = "当前角色不能删除该学校" });
                }
            }

            var extList = _schoolExtensionRepository.GetAll(p => p.IsValid == true && p.Sid == request.SId);
            var onlineSchool = _onlineSchoolRepository.GetIsValid(p => p.Id == request.SId);
            var extList1 = _onlineSchoolExtensionRepository.GetAll(p => p.IsValid == true && p.Sid == request.SId) ?? Enumerable.Empty<OnlineSchoolExtension>();

            try
            {
                UnitOfWork.BeginTransaction();
                school.IsValid = false;
                school.Modifier = request.UserId;
                school.ModifyDateTime = DateTime.Now;
                _schoolRepository.Update(school);
                foreach (var item in extList)
                {
                    item.IsValid = false;
                    item.ModifyDateTime = DateTime.Now;
                    item.Modifier = request.UserId;
                    _schoolExtensionRepository.Update(item);
                }
                if (onlineSchool != null)
                {
                    onlineSchool.IsValid = false;
                    onlineSchool.Modifier = request.UserId;
                    onlineSchool.ModifyDateTime = DateTime.Now;
                    _onlineSchoolRepository.Update(onlineSchool);
                }
                foreach (var item in extList1)
                {
                    item.IsValid = false;
                    item.ModifyDateTime = DateTime.Now;
                    item.Modifier = request.UserId;
                    _onlineSchoolExtensionRepository.Update(item);
                }
                UnitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                UnitOfWork.Rollback();
                return Task.FromResult(new HttpResponse<string> { State = 500, Message = "删除失败！" });
            }

            SimpleQueue.Default.EnqueueThenRunOnChildScope(
                (_sp, e) => _sp.GetService<IMediator>().Publish(e),
                new SchoolUpdatedEvent { Sid = request.SId, IsDeleted = true, UserId = request.UserId }
            );

            return Task.FromResult(new HttpResponse<string> { State = 200 });
        }
    }
}
