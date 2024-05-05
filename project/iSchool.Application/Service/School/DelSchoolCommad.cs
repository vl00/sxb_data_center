using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Modles;
using MediatR;

namespace iSchool.Application.Service
{
    public class DelSchoolCommad : IRequest<HttpResponse<string>>
    {
        public Guid SId { get; set; }
        public Guid UserId { get; set; }
        public bool IsAll { get; set; }
    }
}
