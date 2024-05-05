using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Modles;
using MediatR;

namespace iSchool.Application.Service
{
    public class AddSchoolAchListCommand : IRequest<HttpResponse<string>>
    {
        public AddSchoolAchListCommand()
        {
        }

        public AddSchoolAchListCommand(SchoolAchItem[] data, Guid extId, int year, float completion)
        {
            Data = data;
            ExtId = extId;
            Year = year;
            Completion = completion;
        }

        public SchoolAchItem[] Data { get; set; }
        public Guid ExtId { get; set; }
        public int Year { get; set; }
        public float Completion { get; set; }
        public Guid UserId { get; set; }

    }
    public class SchoolAchItem
    {
        public Guid Id { get; set; }
        public int Count { get; set; }
    }

}
