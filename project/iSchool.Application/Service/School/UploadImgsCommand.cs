using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Application.ViewModels;
using iSchool.Domain.Modles;
using MediatR;

namespace iSchool.Application.Service
{
    public class UploadImgsCommand : IRequest<bool>
    {
        public IEnumerable<UploadImgDto> Imgs { get; set; }
        public Guid Eid { get; set; }
        public Guid UserId { get; set; }
    }
}
