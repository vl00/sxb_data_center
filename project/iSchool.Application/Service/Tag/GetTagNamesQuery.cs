﻿using iSchool.Application.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class GetTagNamesQuery : IRequest<TagItem[]>
    {
        public Guid[] Ids { get; set; }
    }
}
