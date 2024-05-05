using System;
using System.Collections.Generic;
using System.Text;
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
    public class ClaimSchoolCommandHandler : IRequestHandler<ClaimSchoolCommand, HttpResponse<string>>
    {
        private readonly IRepository<School> _schoolRepository;
        readonly IServiceProvider sp;

        public ClaimSchoolCommandHandler(IRepository<School> schoolRepository, IServiceProvider sp)
        {
            _schoolRepository = schoolRepository;
            this.sp = sp;
        }

        public Task<HttpResponse<string>> Handle(ClaimSchoolCommand request, CancellationToken cancellationToken)
        {
            var school = _schoolRepository.GetIsValid(p => p.Id == request.Sid);
            if (school == null)
            {
                return Task.FromResult(new HttpResponse<string> { State = 404, Message = "没有找到当前学校！" });
            }
            if (school.Status != (byte)SchoolStatus.Initial)
            {
                return Task.FromResult(new HttpResponse<string> { State = 200, Message = "当前学校不需要认领" });
            }
            school.Modifier = request.UserId;
            school.ModifyDateTime = DateTime.Now;
            school.CreateTime = DateTime.Now;
            school.Creator = request.UserId;
            school.Status = (byte)SchoolStatus.Edit;
            _schoolRepository.Update(school);

            SimpleQueue.Default.EnqueueThenRunOnChildScope(sp, 
                (_sp, e) => _sp.GetService<IMediator>().Publish(e),
                new SchoolUpdatedEvent { Sid = school.Id }
            );

            return Task.FromResult(new HttpResponse<string> { State = 200 });
        }
    }
}
