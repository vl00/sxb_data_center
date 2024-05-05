using iSchool.Application.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class GetTag2CgysQuery : IRequest<List<Tag2Cgy>>
    {
        public int Tag1Cgy { get; set; }
    }

    public class Tag2Cgy
    {
        public int? Id { get; set; }
        public string Name { get; set; }
    }
}
