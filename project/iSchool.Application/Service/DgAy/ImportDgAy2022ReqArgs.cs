using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.DgAy
{
    public class ImportDgAy2022ReqArgs : IRequest<ImportDgAy2022ResResult>
    {
        public string FilePath { get; set; }
        public Guid UserId { get; set; }
        public string Gid { get; set; }
    }

    public class ImportDgAy2022ResResult
    {
        /// <summary>错误s</summary>
        public string Errs { get; set; }
    }
}
