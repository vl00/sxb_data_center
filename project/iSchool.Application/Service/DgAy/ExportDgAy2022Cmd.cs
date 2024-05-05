using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.DgAy
{
    public class ExportDgAy2022Cmd : IRequest<ExportDgAy2022CmdResult>
    {
        /// <summary>文件id</summary>
        public string Id { get; set; }
    }

    public class ExportDgAy2022CmdResult
    {
        /// <summary>文件id</summary>
        public string Id { get; set; }
        /// <summary>错误s</summary>
        public string Errs { get; set; }
    }
}
