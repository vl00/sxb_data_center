using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain;
using iSchool.Domain.Enum;
using MediatR;

namespace iSchool.Application.Service
{
    public class AddGeneralTagsCommand : IRequest<List<GeneralTag>>
    {
        public string[] Names { get; set; }
    }
}
