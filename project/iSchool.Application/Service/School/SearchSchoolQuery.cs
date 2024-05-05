﻿using iSchool.Application.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class SearchSchoolQuery : IRequest<List<KeyValueDto<Guid>>>
    {
        public SearchSchoolQuery()
        {
        }

        public SearchSchoolQuery(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public int Top { get; set; }

        public byte Grade { get; set; }

        public bool IsCollage { get; set; } = false;

        public bool IsOnline { get; set; } = true;

        public bool ContainExt { get; set; } = true;
    }
}
