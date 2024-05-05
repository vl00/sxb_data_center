using iSchool.Domain;
using iSchool.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{   
    public class UpTag2CgyCommand : IRequest
    {
        public int Tag2Cgy { get; set; }
        public string Name { get; set; }
    }    
}
