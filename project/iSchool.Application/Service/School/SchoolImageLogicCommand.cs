using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Modles;
using MediatR;

namespace iSchool.Application.Service
{
    public class SchoolImageLogicCommand : IRequest<bool>
    {
        public List<VueUploadImgArry> UploadImgArry { get; set; }
        public Guid Eid { get; set; }

        //public List<byte> Types { get; set; } = new List<byte>();

        public Guid UserId { get; set; }
    }
}
