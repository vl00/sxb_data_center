using iSchool.Application.ViewModels;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class AddSchoolExtCommand : IRequest<HttpResponse<object>>
    {
        public AddSchoolExtCommand(SchoolExtensionDto dto, DataOperation operation, bool isAll)
        {
            Dto = dto;
            Operation = operation;
            this.isAll = isAll;
        }

        public SchoolExtensionDto Dto { get; set; }

        public DataOperation Operation { get; set; }

        public bool isAll { get; set; } = false;
    }
}
