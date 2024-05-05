using AutoMapper;
using Dapper;
using iSchool.Domain;
using iSchool.Infrastructure;
using MediatR;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static iSchool.Infrastructure.ObjectHelper;

namespace iSchool.Application.Service.Audit
{
    public class ExportDeletedSchextDpwdCommandHandler : IRequestHandler<ExportDeletedSchextDpwdCommand, byte[]>
    {
        IMediator mediator;
        IMapper mapper;

        public ExportDeletedSchextDpwdCommandHandler(IMediator mediator, IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        public async Task<byte[]> Handle(ExportDeletedSchextDpwdCommand cmd, CancellationToken cancellationToken)
        {
            var pg = await mediator.Send(mapper.Map<DpwdRecorrQuery>(cmd));
            var items = pg.CurrentPageItems;

            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("Sheet1");
                var row = 1;
                sheet.Cells[row, 1].Value = "学校ID";
                sheet.Cells[row, 2].Value = "学部ID";
                sheet.Cells[row, 3].Value = "学部全称";
                sheet.Cells[row, 4].Value = "关联点评ID";
                sheet.Cells[row, 5].Value = "点评内容";
                sheet.Cells[row, 6].Value = "删除时间";
                sheet.Cells[row, 7].Value = "类型";
                sheet.Cells[row, 8].Value = "修改后的学部id";
                sheet.Column(8).Width = 50;
                foreach (var item in items)
                {
                    row++;
                    sheet.Row(row).Height = 15;
                    sheet.Cells[row, 1].Value = $"{item.Sid}";
                    sheet.Cells[row, 2].Value = $"{item.Eid}";
                    sheet.Cells[row, 3].Value = $"{item.Sname + (string.IsNullOrEmpty(item.Ename) ? "" : "-" + item.Ename)}";
                    sheet.Cells[row, 4].Value = $"{item.Dwid}";
                    sheet.Cells[row, 5].Value = $"{item.Content}";
                    sheet.Cells[row, 6].Value = $"{item.DelTime.ToString("yyyy-MM-dd HH:mm:ss")}";
                    sheet.Cells[row, 7].Value = $"{(item.Dtype == 1 ? "点评" : item.Dtype == 2 ? "问答" : "")}";
                }
                return package.GetAsByteArray();
            }
        }
    }
}
