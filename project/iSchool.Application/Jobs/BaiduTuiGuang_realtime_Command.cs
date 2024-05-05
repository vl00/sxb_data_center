using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Jobs
{
    public class BaiduTuiGuang_realtime_Command : IRequest
    {
        public string City { get; set; } //'110100','310100','440100','440300'
        public string Lv { get; set; } //and lv='a'
    }
}
