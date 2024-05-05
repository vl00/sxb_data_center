using AutoMapper;
using Common;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Domain;
using iSchool.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using Polly;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Common.EpplusHelper;

namespace iSchool.Application.Service.DgAy
{
    public class ExportDgAy2022CmdHandler : IRequestHandler<ExportDgAy2022Cmd, ExportDgAy2022CmdResult>
    {
        readonly UnitOfWork _schoolUnitOfWork;
        readonly IMediator _mediator;
        readonly IServiceProvider services;
        readonly CSRedis.CSRedisClient redis;
        readonly IConfiguration config;

        const string opt_cityName = "广州";
        const int opt_city = 440100;
        const int row_tosave = 1000;

        public ExportDgAy2022CmdHandler(IUnitOfWork schoolUnitOfWork, IMediator mediator, CSRedis.CSRedisClient redis,
            IConfiguration config,
            IServiceProvider sp)
        {
            this._schoolUnitOfWork = (UnitOfWork)schoolUnitOfWork;
            this._mediator = mediator;
            this.services = sp;
            this.redis = redis;
            this.config = config;
        }

        public async Task<ExportDgAy2022CmdResult> Handle(ExportDgAy2022Cmd cmd, CancellationToken cancellationToken)
        {
            //$"{DateTime.Now:yyMMddHHmmss}{Guid.NewGuid():n}"
            var res = new ExportDgAy2022CmdResult { Id = cmd.Id };

            try
            {
                await Handle_Export(res, cmd, cancellationToken);
            }
            catch (Exception ex)
            {
                res.Errs = ex.Message + "\n" + ex.StackTrace;
            }

            await redis.SetAsync(BgCacheKeys.DgAy_export_result.FormatWith(res.Id), res, 60 * 30).AwaitNoErr();
            await redis.LockExReleaseAsync(BgCacheKeys.DgAy_import, res.Id);

            return res;
        }

        internal async Task Handle_Export(ExportDgAy2022CmdResult res, ExportDgAy2022Cmd cmd, CancellationToken cancellationToken)
        {
            var m_xlsx0 = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), $"App_Data/dgay2022.xlsx")).Replace('\\', '/');
            var m_excelPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, config["AppSettings:XlsxDir"], $"{res.Id}.xlsx")).Replace('\\', '/');
            var row = 1;
            await Task.CompletedTask;

            var excelpkg = new ExcelPackage { File = new FileInfo(m_excelPath) };
            if (File.Exists(m_xlsx0))
            {
                File.Copy(m_xlsx0, m_excelPath, true);
                excelpkg.Dispose();
                excelpkg = TryGetExcelPackage(m_excelPath, out _);
            }

            var curr_sheet_name = "问题";
            row = 1;
            {
                var sheet = excelpkg.WorkSheetGetOrAdd(curr_sheet_name);
                var col = 1;
                foreach (var row1val in ExcelChecker.Sheet_ques_row1())
                {
                    sheet.Cells[row, col].Value = row1val;
                    col++;
                }
                row++;
            }
            long? prev_qid = null;
            await foreach (var dto in GetDgAyQuesOpts())
            {
                var col = 1;
                var qid = prev_qid == dto.Id ? (long?)null : dto.Id;
                var sheet = excelpkg.Workbook.Worksheets[curr_sheet_name];
                sheet.Cells[row, col++].Value = $"{qid}";
                sheet.Cells[row, col++].Value = $"{(qid == null ? "" : dto.Title)}";
                sheet.Cells[row, col++].Value = $"{(qid == null ? "" : dto.Type.ToEnum<DgAyQuestionTypeEnum>().GetDesc())}";
                sheet.Cells[row, col++].Value = $"{(qid == null ? "" : dto.MaxSelected?.ToString())}";
                sheet.Cells[row, col++].Value = $"{dto.OptId}";
                sheet.Cells[row, col++].Value = $"{dto.OptTitle}";
                sheet.Cells[row, col++].Value = $"{DoubleToString(dto.Point)}";
                sheet.Cells[row, col++].Value = $"{(qid == null ? "" : dto.FindField)}";
                sheet.Cells[row, col++].Value = $"{dto.FindFieldFw}";
                row++;
                prev_qid = dto.Id;

                if (row % row_tosave == 0) excelpkg = await SaveAndGetNewer(excelpkg);
            }            
            excelpkg = await SaveAndGetNewer(excelpkg);

            curr_sheet_name = "关联下一题";
            row = 1;                    
            {
                var sheet = excelpkg.WorkSheetGetOrAdd(curr_sheet_name);
                var col = 1;
                foreach (var row1val in ExcelChecker.Sheet_nextq_row1())
                {
                    sheet.Cells[row, col].Value = row1val;
                    col++;
                }
                row++;
            }
            long? prev_qid2 = null;
            await foreach (var dto in GetDgAyToNextQ())
            {
                var col = 1;
                var qid = prev_qid2 == dto.Qid ? (long?)null : dto.Qid;
                var sheet = excelpkg.Workbook.Worksheets[curr_sheet_name];
                sheet.Cells[row, col++].Value = $"{qid}";
                sheet.Cells[row, col++].Value = $"{dto.Optid}";
                sheet.Cells[row, col++].Value = $"{dto.NextQid}";
                row++;
                prev_qid2 = dto.Qid;

                if (row % row_tosave == 0) excelpkg = await SaveAndGetNewer(excelpkg);
            }
            excelpkg = await SaveAndGetNewer(excelpkg);

            curr_sheet_name = "地址与对应对口小学";
            row = 1;
            {
                var sheet = excelpkg.WorkSheetGetOrAdd(curr_sheet_name);
                var col = 1;
                foreach (var row1val in ExcelChecker.Sheet_AddressAndPrimarySchool())
                {
                    sheet.Cells[row, col].Value = row1val;
                    col++;
                }
                row++;
            }
            string prev_areaName = null;
            await foreach (var dto in GetDgAyAddressAndPrimarySchool())
            {
                var col = 1;
                var areaName = prev_areaName == dto.AreaName ? (string)null : dto.AreaName;
                var sheet = excelpkg.Workbook.Worksheets[curr_sheet_name];
                sheet.Cells[row, col++].Value = $"{areaName}";
                sheet.Cells[row, col++].Value = $"{dto.Address}";
                sheet.Cells[row, col++].Value = $"{dto.Eid}";
                row++;
                prev_areaName = dto.AreaName;

                if (row % row_tosave == 0) excelpkg = await SaveAndGetNewer(excelpkg);
            }
            excelpkg = await SaveAndGetNewer(excelpkg);

            curr_sheet_name = "政策文件";
            row = 1;
            var col_p2 = 0;
            {
                var sheet = excelpkg.WorkSheetGetOrAdd(curr_sheet_name);
                var col = 1;
                foreach (var row1val in ExcelChecker.Sheet_SchPcyFile1())
                {
                    sheet.Cells[row, col].Value = row1val;
                    col++;
                }
                col_p2 = col = col + 8;
                foreach (var row1val in ExcelChecker.Sheet_SchPcyFile2())
                {
                    sheet.Cells[row, col].Value = row1val;
                    col++;
                }
                row++;
            }
            row = 2;
            await foreach (var dto in GetDgAySchPcyFile(new[] { DgAySchPcyFileTypeEnum.Cp, DgAySchPcyFileTypeEnum.Ov, DgAySchPcyFileTypeEnum.Jf }))
            {
                var col = 1;
                var sheet = excelpkg.Workbook.Worksheets[curr_sheet_name];
                sheet.Cells[row, col++].Value = $"{dto.Type.ToEnum<DgAySchPcyFileTypeEnum>().GetDesc()}";
                sheet.Cells[row, col++].Value = $"{dto.AreaName}";
                sheet.Cells[row, col++].Value = $"{dto.Year}";
                sheet.Cells[row, col++].Value = $"{dto.Title}";
                sheet.Cells[row, col++].Value = $"{dto.Url}";
                sheet.Cells[row, col++].Value = $" ";
                row++;

                if (row % row_tosave == 0) excelpkg = await SaveAndGetNewer(excelpkg);
            }
            excelpkg = await SaveAndGetNewer(excelpkg);
            row = 2;
            await foreach (var dto in GetDgAySchPcyFile(new[] { DgAySchPcyFileTypeEnum.CpHeli, DgAySchPcyFileTypeEnum.CpPcAssign }))
            {
                var col = col_p2;
                var sheet = excelpkg.Workbook.Worksheets[curr_sheet_name];
                sheet.Cells[row, col++].Value = $"{dto.Type.ToEnum<DgAySchPcyFileTypeEnum>().GetDesc()}";
                sheet.Cells[row, col++].Value = $"{dto.AreaName}";
                sheet.Cells[row, col++].Value = $"{dto.Year}";
                sheet.Cells[row, col++].Value = $"{dto.Title}";
                sheet.Cells[row, col++].Value = $"{dto.Url}";
                sheet.Cells[row, col++].Value = $" ";
                row++;

                if (row % row_tosave == 0) excelpkg = await SaveAndGetNewer(excelpkg);
            }
            excelpkg = await SaveAndGetNewer(excelpkg);

            await SaveAndDispose(excelpkg);
        }

        internal static async Task<ExcelPackage> SaveAndGetNewer(ExcelPackage excelpkg)
        {
            var fileInfo = excelpkg.File;
            await SaveAndDispose(excelpkg);
            excelpkg = TryGetExcelPackage(fileInfo, out var ex_xlsx);
            if (ex_xlsx != null) throw new CustomResponseException("保存后重新打开出错");
            return excelpkg;
        }

        private async IAsyncEnumerable<Sheet1Dto> GetDgAyQuesOpts()
        {
            var conn = _schoolUnitOfWork.DbConnection;
            int pi = 1, ps = 10;
            for (var has_more = true; has_more;)
            {
                var sql = @"
select q.Id,q.Title,q.Type,q.MaxSelected,opt.id as optid,opt.title as optTitle,opt.point,
q.FindField,opt.FindFieldFw
from DgAyQuestion q
left join DgAyQuestionOption opt on q.id=opt.qid and opt.IsValid=1
where q.IsValid=1
order by q.Is1st desc,q.id,opt.sort
OFFSET (@pi-1)*@ps ROWS FETCH NEXT (@ps+1) ROWS ONLY
";
                var ls = await Policy<Sheet1Dto[]>.Handle<Exception>()
                    .RetryForeverAsync(async x =>
                    {
                        var ex = x.Exception;
                        //log.LogWarning(ex, "do write error and retry later...");
                        await Task.Delay(1000);
                    })
                    .ExecuteAsync(async () =>
                    {
                        var ls0 = await conn.QueryAsync<Sheet1Dto>(sql, new { pi, ps });
                        return ls0.ToArray();
                    });

                foreach (var itm in ls.Take(ps))
                    yield return itm;

                has_more = ls.Length > ps;
                pi++;
            }
        }

        private async IAsyncEnumerable<Sheet2Dto> GetDgAyToNextQ()
        {
            var conn = _schoolUnitOfWork.DbConnection;
            int pi = 1, ps = 10;
            for (var has_more = true; has_more;)
            {
                var sql = @"
select * from(
	select distinct q.Id as qid,--nxt.sctype,nxt.scid,
	(case when nxt.sctype=1 then nxt.scid else null end) as optid,
	nxt.nextqid
	from DgAyQuestion q
	left join DgAyQuestionOption opt on q.id=opt.qid and opt.IsValid=1
	left join DgAyToNextQuestion nxt on nxt.IsValid=1 
		and ((nxt.sctype=1 and nxt.scid=opt.id) or (nxt.sctype=2 and nxt.scid=q.id))
	where q.IsValid=1
)T order by qid,optid,nextqid
OFFSET (@pi-1)*@ps ROWS FETCH NEXT (@ps+1) ROWS ONLY
";
                var ls = await Policy<Sheet2Dto[]>.Handle<Exception>()
                    .RetryForeverAsync(async x =>
                    {
                        var ex = x.Exception;
                        //log.LogWarning(ex, "do write error and retry later...");
                        await Task.Delay(1000);
                    })
                    .ExecuteAsync(async () =>
                    {
                        var ls0 = await conn.QueryAsync<Sheet2Dto>(sql, new { pi, ps });
                        return ls0.ToArray();
                    });

                foreach (var itm in ls.Take(ps))
                    yield return itm;

                has_more = ls.Length > ps;
                pi++;
            }
        }

        private async IAsyncEnumerable<Sheet3Dto> GetDgAyAddressAndPrimarySchool()
        {
            var conn = _schoolUnitOfWork.DbConnection;
            int pi = 1, ps = 10;
            for (var has_more = true; has_more;)
            {
                var sql = $@"
select k3.name as areaname,d.Address,d.eid,d.area,d.sort
from DgAyAddressAndPrimarySchool d
join KeyValue k3 on k3.IsValid=1 and k3.type=1 and k3.id=d.area
where d.IsValid=1 and d.city={opt_city}
order by d.sort
OFFSET (@pi-1)*@ps ROWS FETCH NEXT (@ps+1) ROWS ONLY
";
                var ls = await Policy<Sheet3Dto[]>.Handle<Exception>()
                    .RetryForeverAsync(async x =>
                    {
                        var ex = x.Exception;
                        //log.LogWarning(ex, "do write error and retry later...");
                        await Task.Delay(1000);
                    })
                    .ExecuteAsync(async () =>
                    {
                        var ls0 = await conn.QueryAsync<Sheet3Dto>(sql, new { pi, ps });
                        return ls0.ToArray();
                    });

                foreach (var itm in ls.Take(ps))
                    yield return itm;

                has_more = ls.Length > ps;
                pi++;
            }
        }

        private async IAsyncEnumerable<Sheet4Dto> GetDgAySchPcyFile(DgAySchPcyFileTypeEnum[] types)
        {
            var conn = _schoolUnitOfWork.DbConnection;
            int pi = 1, ps = 10;
            for (var has_more = true; has_more;)
            {
                var sql = $@"
select d.type,k3.name as areaname,d.year,d.title,d.url,d.area
from DgAySchPcyFile d
join KeyValue k3 on k3.IsValid=1 and k3.type=1 and k3.id=d.area
where d.IsValid=1 and d.city={opt_city} and d.type in @types
order by d.area,d.type,d.id
OFFSET (@pi-1)*@ps ROWS FETCH NEXT (@ps+1) ROWS ONLY
";
                var ls = await Policy<Sheet4Dto[]>.Handle<Exception>()
                    .RetryForeverAsync(async x =>
                    {
                        var ex = x.Exception;
                        //log.LogWarning(ex, "do write error and retry later...");
                        await Task.Delay(1000);
                    })
                    .ExecuteAsync(async () =>
                    {
                        var ls0 = await conn.QueryAsync<Sheet4Dto>(sql, new { pi, ps, types = types.Select(_ => _.ToInt()) });
                        return ls0.ToArray();
                    });

                foreach (var itm in ls.Take(ps))
                    yield return itm;

                has_more = ls.Length > ps;
                pi++;
            }
        }

        static string DoubleToString(double? v)
        {
            if (v == null) return null;
            if (v == 0) return "0";
            var str = $"{v.Value}";
            if (str.IndexOf('.') == -1) return str;
            str = str.TrimEnd('0');
            if (str.Length > 0 && str[^1] == '.') return str[..^1];
            return str;
        }

        static class ExcelChecker
        {
            /// <summary>sheet='问题'</summary>
            public static IEnumerable<string> Sheet_ques_row1()
            {
                yield return "问题编号";
                yield return "问题";
                yield return "题型";
                yield return "最多可选多少选项";
                yield return "选项编号";
                yield return "选项";
                yield return "选项分数";
                yield return "查民办字段";
                yield return "民办字段数值";
            }

            /// <summary>sheet='关联下一题'</summary>
            public static IEnumerable<string> Sheet_nextq_row1()
            {
                yield return "问题编号";
                yield return "选项编号";
                yield return "下一题编号";
            }

            public static IEnumerable<string> Get_FindFields()
            {
                yield return "区";
                yield return "学费";
                yield return "走读寄宿";
            }

            public static IEnumerable<(string Val, string Jx)> Get_LodgingSdExtern()
            {
                yield return ("走读", "{tb}.lodging=0 and {tb}.sdextern=1");
                yield return ("寄宿", "{tb}.lodging=1 and {tb}.sdextern=0");
                yield return ("可走读可寄宿", "{tb}.lodging=1 and {tb}.sdextern=1");
            }

            public static bool CheckRow1(ExcelWorksheet sheet, string field)
            {
                var x = FindXlsxRowStringValues(sheet, 1, _ => field.Equals(_));
                return x.Count() == 1;
            }

            /// <summary>sheet='地址与对应对口小学'</summary>
            public static IEnumerable<string> Sheet_AddressAndPrimarySchool()
            {
                yield return "区";
                yield return "地址";
                yield return "对口小学eid";
            }

            /// <summary>sheet='政策文件'</summary>
            public static IEnumerable<string> Sheet_SchPcyFile1()
            {
                yield return "小学入学方式";
                yield return "区";
                yield return "年份";
                yield return "小学政策";
                yield return "url";
            }
            /// <summary>sheet='政策文件'</summary>
            public static IEnumerable<string> Sheet_SchPcyFile2()
            {
                yield return "初中升学";
                yield return "区";
                yield return "年份";
                yield return "小升初政策";
                yield return "url";
            }
        }

        class Sheet1Dto
        {
            /// <summary>
            /// 问题编号
            /// </summary> 
            [ExplicitKey]
            public long Id { get; set; }

            /// <summary>
            /// 
            /// </summary> 
            public string Title { get; set; }

            /// <summary>
            /// 题型
            /// </summary> 
            public byte Type { get; set; }

            /// <summary>
            /// 题型=多选 时最多可选多少选项
            /// </summary> 
            public int? MaxSelected { get; set; }

            public long? OptId { get; set; }

            public string OptTitle { get; set; }

            public double? Point { get; set; }

            public string FindField { get; set; }

            public string FindFieldFw { get; set; }
        }
        class Sheet2Dto
        {
            public long Qid { get; set; }
            public long? Optid { get; set; }
            public long? NextQid { get; set; }
        }
        class Sheet3Dto
        {
            public string AreaName { get; set; }
            public string Address { get; set; }
            public Guid? Eid { get; set; }

            public long? area { get; set; }
            public int? sort { get; set; }
        }
        class Sheet4Dto
        {
            public int Type { get; set; }
            public string AreaName { get; set; }
            public int Year { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }

            public long area { get; set; }
        }


    }
}
