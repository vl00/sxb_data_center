using MediatR;
using System;
using System.Collections.Generic;
using System.IO;

namespace iSchool.Application.Service.CollegeDirectory
{
    public class ExcelImportCommand : IRequest<bool>
    {
        public Stream Excel { get; set; }
        public Guid UserId { get; set; }
    }
}
