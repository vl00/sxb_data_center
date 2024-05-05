using iSchool.Domain;
using iSchool.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class AddTag2CgyCommand : IRequest<FnResult<int>>
    {
        /// <summary>
        /// 一级分类 TagType
        /// </summary>
        public int Tag1Cgy { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
    }       
}
