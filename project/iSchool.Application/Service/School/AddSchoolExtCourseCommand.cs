using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Modles;
using MediatR;

namespace iSchool.Application.Service
{
    public class AddSchoolExtCourseCommand : IRequest<HttpResponse<string>>
    {
        public string Courses { get; set; }
        public string Characteristic { get; set; }
        public string Authentication { get; set; }
        public Guid UserId { get; set; }
        public Guid Eid { get; set; }
        public Guid Sid { get; set; }
        public float Completion { get; set; } = 0;
    }
}
