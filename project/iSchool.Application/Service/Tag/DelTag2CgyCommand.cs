using iSchool.Domain;
using iSchool.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class DelTag2CgyCommand : IRequest
    {
        public int Tag1Cgy { get; set; }
        public int[] Tag2CgyIds { get; set; } = new int[0];
        public Guid UserId { get; set; }
    }
}
