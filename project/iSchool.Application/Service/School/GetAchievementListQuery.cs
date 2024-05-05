using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using MediatR;
namespace iSchool.Application.Service
{
    public class GetAchievementListQuery : IRequest<List<KeyValueDto<string>>>
    {
        public GetAchievementListQuery()
        {
        }

        public GetAchievementListQuery(Guid extId, int year, byte grade)
        {
            ExtId = extId;
            Year = year;
            Grade = grade;
        }

        public Guid ExtId { get; set; }
        public int Year { get; set; }
        public byte Grade { get; set; }
    }
}
