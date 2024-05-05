using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Modles;
using MediatR;

namespace iSchool.Application.Service
{
    public class ExaminationSchoolCommand:IRequest<HttpResponse<string>>
    {
        public Guid Sid { get; set; }
        public bool IsAll { get; set; }
        public Guid UserId { get; set; }
    }
}
