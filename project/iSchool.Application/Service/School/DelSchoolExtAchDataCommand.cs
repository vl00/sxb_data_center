using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Modles;
using MediatR;
namespace iSchool.Application.Service
{

    public class DelSchoolExtAchDataCommand : IRequest<HttpResponse<string>>
    {
        public DelSchoolExtAchDataCommand(int year, Guid extId, Guid? userId)
        {
            Year = year;
            ExtId = extId;
            UserId = userId;
        }

        public int Year { get; set; }

        public Guid ExtId { get; set; }

        public Guid? UserId { get; set; }
    }
}
