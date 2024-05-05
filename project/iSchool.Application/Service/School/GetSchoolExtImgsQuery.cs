using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using iSchool.Domain.Modles;
using MediatR;

namespace iSchool.Application.Service
{
    public class GetSchoolExtImgsQuery : IRequest<Dictionary<byte, UploadImgDto>>
    {        
        public Guid Eid { get; set; }
        public byte[] Types { get; set; }
    }


}
