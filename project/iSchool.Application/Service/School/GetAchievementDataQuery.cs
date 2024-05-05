using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Modles;
using MediatR;
namespace iSchool.Application.Service
{
    public class GetAchievementDataQuery : IRequest<HttpResponse<object>>
    {
        public GetAchievementDataQuery(int year, Guid extId)
        {
            Year = year;
            ExtId = extId;
        }

        public int Year { get; set; }
        public Guid ExtId { get; set; }
    }
}
