using iSchool.Application.ViewModels;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class AddSchoolExtQualityCommand : IRequest<HttpResponse<object>>
    {
        public SchoolExtQualityDto Dto { get; set; }        

        public Guid CurrentUserId { get; set; }        
    }
}
