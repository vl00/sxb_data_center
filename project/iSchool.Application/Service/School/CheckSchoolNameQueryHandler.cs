using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using MediatR;

namespace iSchool.Application.Service
{
    public class CheckSchoolNameQueryHandler : IRequestHandler<CheckSchoolNameQuery, HttpResponse<string>>
    {
        private readonly IRepository<School> _schoolRepository;

        public CheckSchoolNameQueryHandler(IRepository<School> schoolRepository)
        {
            _schoolRepository = schoolRepository;
        }

        public Task<HttpResponse<string>> Handle(CheckSchoolNameQuery request, CancellationToken cancellationToken)
        {
            School school = null;
            var name = request.Name.Trim();
            if (request.Sid == Guid.Empty)
                school = _schoolRepository.GetIsValid(p => p.Name == name);
            else
                school = _schoolRepository.GetIsValid(p => p.Name == name && p.Id != request.Sid);

            if (school == null)
            {
                return Task.FromResult(new HttpResponse<string> { State = 200 });
            }
            else
            {
                return Task.FromResult(new HttpResponse<string> { State = 500, Message = "该学校已经存在！" });
            }
        }
    }
}
