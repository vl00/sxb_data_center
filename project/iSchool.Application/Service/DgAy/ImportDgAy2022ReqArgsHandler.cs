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
    public class ImportDgAy2022ReqArgsHandler : IRequestHandler<ImportDgAy2022ReqArgs, ImportDgAy2022ResResult>
    {
        readonly UnitOfWork _schoolUnitOfWork;
        readonly IMediator _mediator;
        readonly IServiceProvider services;
        readonly CSRedis.CSRedisClient redis;
        readonly IConfiguration _config;

        const string opt_cityName = "广州";
        const int opt_city = 440100;
        readonly List<string> Errors = new List<string>();
        HashSet<string> Qids = new HashSet<string>();
        HashSet<string> Optids = new HashSet<string>();
        readonly List<QaSel> Sels = new List<QaSel>();

        readonly List<DgAyQuestion> QuesLs = new List<DgAyQuestion>();
        readonly List<DgAyQuestionOption> OptsLs = new List<DgAyQuestionOption>();
        readonly List<DgAyToNextQuestion> DgAyToNextsLs = new List<DgAyToNextQuestion>();
        readonly List<DgAyAddressAndPrimarySchool> Ls_aaps = new List<DgAyAddressAndPrimarySchool>();
        readonly List<DgAySchPcyFile> Ls_SchPcyFiles = new List<DgAySchPcyFile>();

        public ImportDgAy2022ReqArgsHandler(IUnitOfWork schoolUnitOfWork, IMediator mediator, CSRedis.CSRedisClient redis, IConfiguration confg,
            IServiceProvider sp)
        {
            this._schoolUnitOfWork = (UnitOfWork)schoolUnitOfWork;
            this._mediator = mediator;
            this.services = sp;
            this.redis = redis;
            this._config = confg;
        }

        public async Task<ImportDgAy2022ResResult> Handle(ImportDgAy2022ReqArgs req, CancellationToken cancellationToken)
        {
            var res = new ImportDgAy2022ResResult();

            await Handle_Import(res, req, cancellationToken);

            if (res.Errs != null)
            {
                //await File.WriteAllTextAsync($"error-{req.Gid}.txt", res.Errs, Encoding.UTF8);
            }
            else
            {
                var dir = Path.Combine(Directory.GetCurrentDirectory(), "images/dgay");
                if (!Directory.Exists(dir)) try { Directory.CreateDirectory(dir); } catch { }
                try { File.Copy(req.FilePath, Path.Combine(dir, "last.xlsx"), true); } catch { }

                await _mediator.Send(new BgClearRedisCacheCmd
                {
                    Keys = new[] { BgCacheKeys.DgAyKeys }
                }).AwaitNoErr();
            }

            await redis.SetAsync(BgCacheKeys.DgAy_import_result.FormatWith(req.Gid), res, 60 * 30).AwaitNoErr();
            await redis.DelAsync(BgCacheKeys.DgAy_import);

            return res;
        }

        internal async Task Handle_Import(ImportDgAy2022ResResult res, ImportDgAy2022ReqArgs req, CancellationToken cancellationToken)
        {
            var m_excelPath = req.FilePath;
            var m_userid = req.UserId;
            var Now = DateTime.Now;
            var errs = new List<string>();
            var m_giid = req.Gid;
            await default(ValueTask);

            var x = await On_read_and_check_xlsx_datas(m_excelPath, Now).AwaitResOrErr();
            if (x.Error != null)
            {
                Errors.Add($"导入意外失败: {x.Error.Message}");
                goto LB_write_if_has_errors;
            }

            errs.Clear();
            await On_read_xlsx_AddressAndPrimarySchool(errs, m_excelPath, Now).AwaitResOrErr();
            if (errs.Any())
            {
                Errors.Add(@"-------------------------------------------------------------------------
sheet='地址与对应对口小学' 存在以下错误: 

" + string.Join("\n\n", errs));
            }

            errs.Clear();
            await On_read_xlsx_SchPcyFile(errs, m_excelPath, Now).AwaitResOrErr();
            if (errs.Any())
            {
                Errors.Add(@"-------------------------------------------------------------------------
sheet='政策文件' 存在以下错误: 

" + string.Join("\n\n", errs));
            }

            if (!Errors.Any() && 0 == (QuesLs.Count + OptsLs.Count + DgAyToNextsLs.Count + Ls_aaps.Count + Ls_SchPcyFiles.Count))
            {
                Errors.Add("文档没内容");
            }

            // if has errors
            LB_write_if_has_errors:
            if (Errors.Any())
            {
                res.Errs = string.Join("\n\n", Errors);
                return;
            }

            // check ok
            //Debugger.Break();
            //log.LogDebug("check ok json= {json}", (new { QuesLs, OptsLs, DgAyToNextsLs }).ToJsonString(camelCase: true, ignoreNull: true));

            await On_write_datas(m_giid, m_userid).AwaitNoErr();
        }

        internal async Task On_read_and_check_xlsx_datas(string excelPath, DateTime Now)
        {
            using var excelpkg = TryGetExcelPackage(excelPath, out _);
            if (excelpkg == null)
            {
                Errors.Add($"excel文件'{Path.GetFileName(excelPath)}'打不开");
                return;
            }
            if (excelpkg.Workbook.Worksheets["问题"] == null && excelpkg.Workbook.Worksheets["关联下一题"] == null)
            {
                return;
            }
            if (excelpkg.Workbook.Worksheets["问题"] == null || excelpkg.Workbook.Worksheets["关联下一题"] == null)
            {
                Errors.Add($"问题 和 关联下一题 需要同时导入");
                return;
            }
            foreach (var row1val in ExcelChecker.Sheet_ques_row1())
            {
                if (!ExcelChecker.CheckRow1(excelpkg.Workbook.Worksheets["问题"], row1val))
                {
                    Errors.Add($"问题 第一列被修改了");
                    break;
                }
            }
            foreach (var row1val in ExcelChecker.Sheet_nextq_row1())
            {
                if (!ExcelChecker.CheckRow1(excelpkg.Workbook.Worksheets["关联下一题"], row1val))
                {
                    Errors.Add($"关联下一题 第一列被修改了");
                    break;
                }
            }
            if (Errors.Any())
            {
                // 文档不符
                return;
            }

            var sheet = excelpkg.Workbook.Worksheets["问题"];
            for (var row = 2; row <= sheet.Dimension.Rows; row++)
            {
                if (CheckIfEmptyRow(sheet, row)) continue;
                var cell_qid = FindXlsxCellByRow1Field(sheet, row, "问题编号");
                var str_qid = cell_qid.Value?.CellStrValue();
                //
                // 找一个问题的连续row范围,该范围包含opts
                //
                var rowQ = row + 1;
                for (; rowQ <= sheet.Dimension.Rows; rowQ++)
                {
                    if (CheckIfEmptyRow(sheet, rowQ)) break;
                    var str_qid2 = sheet.Cells[rowQ, cell_qid.Col].Value?.CellStrValue();
                    if (str_qid2.IsNullOrEmpty() || str_qid2 == str_qid) continue;
                    break;
                }
                //
                var qid = long.TryParse(str_qid, out var _qid) ? _qid : -1;
                if (qid == -1)
                {
                    Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(cell_qid.Col)}列: 问题编号不是数字");
                }
                if (!Qids.Add(str_qid))
                {
                    Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(cell_qid.Col)}列: 问题编号重复了");
                    goto LB_next;
                }
                //                
                // ques
                var dto_q = new DgAyQuestion() { Id = qid, CreateTime = Now, IsValid = true };
                dto_q.Is1st = qid == 1;
                {
                    var qtitles = FromRange(row, rowQ - 1).Select(i => FindXlsxCellByRow1Field(sheet, i, "问题").Value.CellStrValue()).Where(_ => !_.IsNullOrEmpty()).ToArray();
                    if (!qtitles.Any()) Errors.Add($"{row}行: 问题为空");
                    else if (qtitles.Count() > 1) Errors.Add($"{row} - {rowQ - 1}行: 同一问题问题文字要一样");
                    else dto_q.Title = qtitles.First();

                    var qtypes = FromRange(row, rowQ - 1).Select(i => FindXlsxCellByRow1Field(sheet, i, "题型").Value.CellStrValue()).Where(_ => !_.IsNullOrEmpty()).ToArray();
                    if (!qtypes.Any()) Errors.Add($"{row}行: 请指定问题题型");
                    else if (qtypes.Count() > 1) Errors.Add($"{row} - {rowQ - 1}行: 同一问题题型不能指定多个");
                    else
                    {
                        var qtype = qtypes.First();
                        var qtypeEnum = EnumUtil.GetDescs<DgAyQuestionTypeEnum>().FirstOrDefault(_ => _.Desc == qtype);
                        if (qtypeEnum == default)
                        {
                            Errors.Add($"{row}行: 问题题型只能填入'{string.Join(" ", EnumUtil.GetDescs<DgAyQuestionTypeEnum>().Select(_ => _.Desc))}'");
                        }
                        else dto_q.Type = qtypeEnum.Value.ToByte();
                    }

                    var qmaxselecteds = FromRange(row, rowQ - 1).Select(i => FindXlsxCellByRow1Field(sheet, i, "最多可选多少选项").Value.CellStrValue()).Where(_ => !_.IsNullOrEmpty()).ToArray();
                    if (qmaxselecteds.Count() > 1) Errors.Add($"{row} - {rowQ - 1}行: 同一问题, 最多可选多少选项 不能指定多个");
                    else dto_q.MaxSelected = int.TryParse(qmaxselecteds.FirstOrDefault(), out var _qmx) ? _qmx : (int?)null;
                    if (dto_q.MaxSelected != null && dto_q.Type > 0)
                    {
                        if (dto_q.Type != DgAyQuestionTypeEnum.Ty2.ToByte())
                            Errors.Add($"{row}行: 题型为多选时,才能指定 最多可选多少选项");
                    }

                    var qfindfields = FromRange(row, rowQ - 1).Select(i => FindXlsxCellByRow1Field(sheet, i, "查民办字段").Value.CellStrValue()).Where(_ => !_.IsNullOrEmpty()).ToArray();
                    if (qfindfields.Count() > 1) Errors.Add($"{row} - {rowQ - 1}行: 同一问题, 查民办字段 不能指定多个");
                    else dto_q.FindField = qfindfields.FirstOrDefault();
                    if (dto_q.FindField != null)
                    {
                        if (!dto_q.FindField.In(ExcelChecker.Get_FindFields(), null))
                            Errors.Add($"{row}行: 查民办字段 只能填入 '{string.Join(" ", ExcelChecker.Get_FindFields())}'");

                        if (dto_q.Type == DgAyQuestionTypeEnum.Ty7.ToByte() && dto_q.FindField != "区")
                        {
                            Errors.Add($"{row}行: 题型为'地区单选'时, 查民办字段要为'区'");
                        }
                    }
                }
                QuesLs.Add(dto_q);
                //
                // opts
                for (var irow = row; irow < rowQ; irow++)
                {
                    var str_opt_id = FindXlsxCellByRow1Field(sheet, irow, "选项编号").Value.CellStrValue();
                    var opt_id = long.TryParse(str_opt_id, out var _opt_id) ? _opt_id : 0;
                    var opt_title = FindXlsxCellByRow1Field(sheet, irow, "选项").Value.CellStrValue();
                    var opt_point = FindXlsxCellByRow1Field(sheet, irow, "选项分数").Value.CellStrValue();
                    var opt_fieldfw = FindXlsxCellByRow1Field(sheet, irow, "民办字段数值").Value.CellStrValue();
                    if (opt_id <= 0)
                    {
                        // empty opt row and next
                        if (opt_title.IsNullOrEmpty() && opt_point.IsNullOrEmpty() && opt_fieldfw.IsNullOrEmpty())
                            continue;

                        if (!opt_title.IsNullOrEmpty() || !opt_point.IsNullOrEmpty() || !opt_fieldfw.IsNullOrEmpty())
                            Errors.Add($"{irow}行{ExcelCellAddress.GetColumnLetter(FindXlsxCellByRow1Field(sheet, irow, "选项编号").Col)}列: 选项编码不能为空");
                    }
                    if (opt_id > 0)
                    {
                        if (opt_title.IsNullOrEmpty())
                            Errors.Add($"{irow}行{ExcelCellAddress.GetColumnLetter(FindXlsxCellByRow1Field(sheet, irow, "选项").Col)}列: 选项不能为空");
                    }
                    if (!Optids.Add(str_opt_id))
                    {
                        Errors.Add($"{irow}行{ExcelCellAddress.GetColumnLetter(FindXlsxCellByRow1Field(sheet, irow, "选项编号").Col)}列: 选项编号重复了");
                    }
                    var dto_opt = new DgAyQuestionOption { Id = opt_id, Qid = qid, CreateTime = Now, IsValid = true };
                    dto_opt.Title = opt_title;
                    dto_opt.Sort = irow - row + 1;
                    dto_opt.Point = double.TryParse(opt_point, out var _pt) ? _pt : (double?)null;
                    if (dto_opt.Point == null && !opt_point.IsNullOrEmpty())
                    {
                        Errors.Add($"{irow}行{ExcelCellAddress.GetColumnLetter(FindXlsxCellByRow1Field(sheet, irow, "选项分数").Col)}列: 选项分数要为正整数");
                    }
                    // 判断分数与题型1
                    if (dto_opt.Point != null && !((int)dto_q.Type).In(3, 4))
                    {
                        Errors.Add($"{irow}行{ExcelCellAddress.GetColumnLetter(FindXlsxCellByRow1Field(sheet, irow, "选项分数").Col)}列: "
                            + $"题型为'{dto_q.Type.ToString().ToEnum<DgAyQuestionTypeEnum>().GetDesc()}'时, 不需要填入选项分数");
                    }
                    dto_opt.FindField = dto_q.FindField;
                    //
                    // ExcelChecker.Get_FindFields()
                    // FindFieldFwJx
                    switch (dto_opt.FindField)
                    {
                        case "区":
                            {
                                var val = FindXlsxCellByRow1Field(sheet, irow, "民办字段数值").Value.CellStrValue();
                                val = !string.IsNullOrEmpty(val) ? val : dto_opt.Title;
                                dto_opt.FindFieldFw = val;
                                var (code, ex) = await GetCityArea(opt_cityName, val).AwaitResOrErr();
                                if (ex != null)
                                {
                                    Errors.Add($"{irow}行{ExcelCellAddress.GetColumnLetter(FindXlsxCellByRow1Field(sheet, irow, "民办字段数值").Col)}列: {ex.Message}");
                                    break;
                                }
                                dto_opt.FindFieldFwJx = $"{{tb}}.Area={code}";
                            }
                            break;
                        case "学费":
                            {
                                var val = FindXlsxCellByRow1Field(sheet, irow, "民办字段数值").Value.CellStrValue();
                                if (!MathInterval.TryParse(val, out var ival))
                                {
                                    Errors.Add($"{irow}行{ExcelCellAddress.GetColumnLetter(FindXlsxCellByRow1Field(sheet, irow, "民办字段数值").Col)}列: 无效的学费范围");
                                    break;
                                }
                                dto_opt.FindFieldFw = val;
                                var s = "";
                                if (!double.IsInfinity(ival.A)) s += $"and {{tb}}.Tuition{(ival.Ia ? ">=" : ">")}{ival.A} ";
                                if (!double.IsInfinity(ival.B)) s += $"and {{tb}}.Tuition{(ival.Ib ? "<=" : "<")}{ival.B} ";
                                dto_opt.FindFieldFwJx = s.TrimStart("and ").Trim();
                            }
                            break;
                        case "走读寄宿":
                            {
                                var val = FindXlsxCellByRow1Field(sheet, irow, "民办字段数值").Value.CellStrValue();
                                var x = ExcelChecker.Get_LodgingSdExtern().FirstOrDefault(_ => _.Val == val);
                                if (x == default)
                                {
                                    Errors.Add($@"{irow}行{ExcelCellAddress.GetColumnLetter(FindXlsxCellByRow1Field(sheet, irow, "民办字段数值").Col)}列: 民办字段数值只能填 '{string.Join(" ",
                                        ExcelChecker.Get_LodgingSdExtern().Select(_ => _.Val))}' ");
                                    break;
                                }
                                dto_opt.FindFieldFw = x.Val;
                                dto_opt.FindFieldFwJx = x.Jx;
                            }
                            break;
                        default:
                            break;
                    }
                    OptsLs.Add(dto_opt);
                }
                // 
                {
                    var opts = OptsLs.Where(_ => _.Qid == qid);

                    var opt_titles = opts.GroupBy(_ => _.Title).Where(_ => _.Count() > 1).Select(_ => _.Key);
                    if (opt_titles.Any())
                    {
                        Errors.Add($"{row} - {rowQ - 1}行: 发现重复选项名:{string.Join(", ", opt_titles.Select(_ => $"'{_}'"))}");
                    }
                    if (dto_q.MaxSelected != null && dto_q.MaxSelected > opts.Count())
                    {
                        Errors.Add($"{row}行: 最多可选多少选项 超过选项数了");
                    }

                    if (((int)dto_q.Type).In(1, 2, 3, 4) && !opts.Any())
                    {
                        Errors.Add($"{row}行: 请录入问题的选项");
                    }

                    // 判断分数与题型2
                    if (((int)dto_q.Type).In(3, 4) && !opts.Any(_ => _.Point != null))
                    {
                        Errors.Add($"{row} - {rowQ - 1}行: 计分类型的题目请需要填入分数");
                    }
                }

                LB_next:
                row = rowQ - 1;
            }

            Qids = null;
            Optids = null;
            string err1 = null;

            if (Errors.Any())
            {
                err1 = $@"-------------------------------------------------------------------------
sheet='问题' 存在以下错误: 

";
                err1 += string.Join("\n\n", Errors);
                Errors.Clear();
            }

            sheet = excelpkg.Workbook.Worksheets["关联下一题"];
            for (var row = 2; row <= sheet.Dimension.Rows; row++)
            {
                var col_q = FindXlsxRowStringValues(sheet, 1, _ => _ == "问题编号").First().Col;
                var col_opt = FindXlsxRowStringValues(sheet, 1, _ => _ == "选项编号").First().Col;
                var col_next = FindXlsxRowStringValues(sheet, 1, _ => _ == "下一题编号").First().Col;
                if (!PArray(col_q, col_opt, col_next).Any(_ => !sheet.Cells[row, _].Value.CellStrValue().IsNullOrEmpty()))
                {
                    // is empty row
                    continue;
                }
                //
                var qaSel = new QaSel { Row = row };
                qaSel.Qid = long.TryParse(FindXlsxRowValueByUpward(sheet, row, col_q).Value, out var _qid) ? _qid : 0;
                if (qaSel.Qid <= 0)
                {
                    Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col_q)}列: 问题编号不能为空");
                    continue;
                }
                qaSel.Optid = long.TryParse(sheet.Cells[row, col_opt].Value.CellStrValue(), out var _Optid) ? _Optid : (long?)null;
                if ((qaSel.Optid ?? 0) <= 0 && !sheet.Cells[row, col_opt].Value.CellStrValue().IsNullOrEmpty())
                {
                    Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col_opt)}列: 选项编号要为正整数");
                    continue;
                }
                qaSel.NextQid = long.TryParse(sheet.Cells[row, col_next].Value.CellStrValue(), out var _NextQid) ? _NextQid : (long?)null;
                if ((qaSel.NextQid ?? 0) <= 0 && !sheet.Cells[row, col_next].Value.CellStrValue().IsNullOrEmpty())
                {
                    Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col_next)}列: 下一题编号要为正整数");
                    continue;
                }
                //
                if (!QuesLs.Any(_ => _.Id == qaSel.Qid))
                {
                    Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col_q)}列: 问题编号不存在");
                    continue;
                }
                if (qaSel.Optid != null)
                {
                    var opt = OptsLs.FirstOrDefault(_ => _.Id == qaSel.Optid);
                    if (opt == null)
                    {
                        Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col_opt)}列: 选项编号不存在");
                        continue;
                    }
                    if (opt.Qid != qaSel.Qid)
                    {
                        Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col_opt)}列: 选项编号与问题编号不匹配");
                        continue;
                    }
                }
                if (qaSel.NextQid != null && !QuesLs.Any(_ => _.Id == qaSel.NextQid))
                {
                    Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col_next)}列: 下一题编号不存在");
                    continue;
                }
                if (qaSel.NextQid == null && qaSel.Optid != null)
                {
                    // 选项编号不为空, 但下一题编号为空
                    Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col_next)}列: 请填写下一题编号");
                    continue;
                }
                if (Sels.Any(_ => _.Qid == qaSel.Qid && _.Optid == qaSel.Optid))
                {
                    Errors.Add($"{row}行: 不要重复指定相同的{(qaSel.Optid == null ? "问题" : "(问题和选项)")}的下一题");
                    continue;
                }
                //
                Sels.Add(qaSel);
            }
            // 
            // 多指定 - 问题指定了选项跳题,而又指定了题目跳题
            {
                var qas = Sels.GroupBy(_ => _.Qid).Where(qasel =>
                {
                    var scty1 = qasel.Any(x => x.Optid == null);
                    var scty2 = qasel.Any(x => x.Optid != null);
                    if (!scty1 && !scty2) return false;
                    return scty1 == scty2;
                });
                foreach (var qa in qas)
                {
                    var row = qa.Select(_ => _.Row);
                    if (!row.Any()) continue;
                    Errors.Add($"{row.Min()}-{row.Max()}行: 不要重复指定相同的问题(id={qa.Key})的下一题");
                }
            }
            //
            // 漏填 选项和下一题
            {
                var ls = Sels.Where(_ => _.Optid != null).GroupBy(_ => _.Qid).Select(_ => (_.Key, _));
                foreach (var (qid, items) in ls)
                {
                    var louxuanLs = OptsLs.Where(_ => _.Qid == qid).Where(x => !items.Any(_ => _.Optid == x.Id));
                    if (!louxuanLs.Any()) continue;
                    Errors.Add($"请指定 问题{qid} 的选项{string.Join(",", louxuanLs.Select(_ => _.Id))} 的下一题");
                }
            }
            //
            // 多选 目前只支持题跳题
            for (var ___ = true; ___; ___ = !___)
            {
                var qas = Sels.Where(x => QuesLs.Where(_ => _.Type == (byte)DgAyQuestionTypeEnum.Ty2).Any(_ => _.Id == x.Qid)).GroupBy(_ => _.Qid);
                if (!qas.Any()) continue;
                foreach (var qa in qas)
                {
                    var row = qa.Select(_ => _.Row);
                    if (!row.Any()) continue;
                    if (qa.Any(_ => _.Optid != null))
                    {
                        Errors.Add($"{row.Min()}-{row.Max()}行: 问题(id={qa.Key})题型为多选, 不支持选项跳题");
                    }
                }
            }
            //
            // 检查指定下一题在路径上是否循环
            for (var sels = Sels.ToList(); sels.Count > 0;)
            {
                var ls = new List<QaSel>();
                for (var qsel = sels[0]; ;)
                {
                    if (ls.Any(_ => _.Qid == qsel.Qid))
                    {
                        //* 由于填入顺序, 可能出现提示重复的循环路径
                        //* 只找到循环路径,没找完整路径

                        Errors.Add($"检查到关联循环:\n   "
                            + $"{(string.Join(" -> ", ls.Select(_ => $"(第{_.Row}行)问题{_.Qid}{(_.Optid != null ? $"+选项{_.Optid}" : "")}")))} -> (第{qsel.Row}行)问题{qsel.Qid}");
                        break;
                    }
                    ls.Add(qsel);
                    if (qsel.NextQid == null) break;
                    qsel = Sels!.FirstOrDefault(_ => _.Qid == qsel.NextQid)!;
                    if (qsel == default)
                    {
                        // qsel应该不会为null
                        // '下一题编号不存在'在之前已经判断了
                        break;
                    }
                }
                sels.RemoveAll(x => ls.Any(_ => _.Row == x.Row));
            }
            //
            // 检查(没关联跳转)除qid=1外的其他第一题, 目前只能有1个第一题
            {
                var nextQids = Sels.Where(_ => _.NextQid != null).Select(_ => _.NextQid.Value).Distinct().ToArray();
                if (nextQids.Any(_ => _ == 1))
                {
                    // 下一题编号 不能指定 第一题
                    Errors.Add($"{ExcelCellAddress.GetColumnLetter(FindXlsxRowStringValues(sheet, 1, _ => _ == "下一题编号").First().Col)}列: 不能填入问题编号1, 它是第一题");
                }
                var q1s = QuesLs.Where(_ => !_.Id.In(nextQids, null) && _.Id != 1);
                if (q1s.Any())
                {
                    Errors.Add($"检查到以下题目没填入到{ExcelCellAddress.GetColumnLetter(FindXlsxRowStringValues(sheet, 1, _ => _ == "下一题编号").First().Col)}列:\n"
                        + string.Join("\n", q1s.Select(_ => _.Id)));
                }
            }
            //
            // 预防配题导致出现'atype=0'的没内容报告
            if (!Errors.Any())
            {
                //// atype=1,2
                //for (var ___ = 1 != 2; ___; ___ = !___)
                //{
                //    var qa1 = Sels.FirstOrDefault(_ => _.Qid == 1 && _.Optid == 1);
                //    if (qa1 == null) continue;
                //    var ls = new List<QaSel> { qa1 };
                //    int c_atype1 = 0, c_atype2 = 0;
                //    foreach (var qa in FindNext(ls, qa1))
                //    {
                //        if (qa.NextQid != null) continue;
                //        // find a nxt path
                //        var is_found_aty = false;
                //        for (var i = ls.Count - 1; i >= 0; i--)
                //        {
                //            var qtype = QuesLs.FirstOrDefault(_ => _.Id == ls[i].Qid)?.Type is byte _b ? _b.ToEnum<DgAyQuestionTypeEnum>() : default;
                //            switch (qtype)
                //            {
                //                case DgAyQuestionTypeEnum.Ty5:
                //                    c_atype1 += 1;
                //                    is_found_aty = true;
                //                    i = -1;
                //                    break;
                //                case DgAyQuestionTypeEnum.Ty6:
                //                    c_atype2 += 1;
                //                    is_found_aty = true;
                //                    i = -1;
                //                    break;
                //            }
                //        }
                //        if (!is_found_aty)
                //        {
                //            Errors.Add("检查到对口统筹线路,没指定题型为'地址选择'或'地图定位'的题目,报告会完全为空内容,线路:\n  "
                //                + $"{(string.Join(" -> ", ls.Select(_ => $"(第{_.Row}行)问题{_.Qid}{(_.Optid != null ? $"+选项{_.Optid}" : "")}")))} ");
                //        }
                //    }
                //    if (c_atype1 == 0)
                //    {
                //        Errors.Add("检查到对口统筹线路,没指定题型为'地址选择'的题目,报告分析将无法进入'对口入学'流程.");
                //    }
                //    if (c_atype2 == 0)
                //    {
                //        Errors.Add("检查到对口统筹线路,没指定题型为'地图定位'的题目,报告分析将无法进入'统筹入学'流程.");
                //    }
                //}

                //// atype=3
                //for (var ___ = 3 == 3; ___; ___ = !___) 
                //{
                //    var qa2 = Sels.FirstOrDefault(_ => _.Qid == 1 && _.Optid == 2);
                //    if (qa2 == null) continue;
                //    var ls = new List<QaSel> { qa2 };
                //    foreach (var qa in FindNext(ls, qa2))
                //    {
                //        if (qa.NextQid != null) continue;
                //        // find a nxt path
                //        var is_found_aty = false;
                //        for (var i = ls.Count - 1; i >= 0; i--)
                //        {
                //            var qtype = QuesLs.FirstOrDefault(_ => _.Id == ls[i].Qid)?.Type is byte _b ? _b.ToEnum<DgAyQuestionTypeEnum>() : default;
                //            switch (qtype)
                //            {
                //                case DgAyQuestionTypeEnum.Ty3:
                //                    is_found_aty = true;
                //                    i = -1;
                //                    break;
                //                case DgAyQuestionTypeEnum.Ty4:
                //                    is_found_aty = true;
                //                    i = -1;
                //                    break;
                //            }
                //        }
                //        if (!is_found_aty)
                //        {
                //            Errors.Add("检查到积分入学线路,没指定题型为'计分类型'的题目,报告会为空内容,线路:\n  "
                //                + $"{(string.Join(" -> ", ls.Select(_ => $"(第{_.Row}行)问题{_.Qid}{(_.Optid != null ? $"+选项{_.Optid}" : "")}")))} ");
                //        }
                //    }
                //}

                //// atype=4
                //for (var ___ = 4 == 4; ___; ___ = !___) 
                //{
                //    var qa3 = Sels.FirstOrDefault(_ => _.Qid == 1 && _.Optid == 3);
                //    if (qa3 == null) continue;
                //    var ls = new List<QaSel> { qa3 };
                //    foreach (var qa in FindNext(ls, qa3))
                //    {
                //        if (qa.NextQid != null) continue;
                //        // find a nxt path
                //        var is_found_aty = false;
                //        for (var i = ls.Count - 1; i >= 0; i--)
                //        {
                //            var x_qa = QuesLs.FirstOrDefault(_ => _.Id == ls[i].Qid);
                //            if (string.IsNullOrEmpty(x_qa?.FindField)) continue;
                //            is_found_aty = true;
                //            break;
                //        }
                //        if (!is_found_aty)
                //        {
                //            Errors.Add("检查到查民办线路,题目s没指定查民办字段及其数值,报告会为空内容,线路:\n  "
                //                + $"{(string.Join(" -> ", ls.Select(_ => $"(第{_.Row}行)问题{_.Qid}{(_.Optid != null ? $"+选项{_.Optid}" : "")}")))} ");
                //        }
                //    }
                //}
            }
            //
            if (Errors.Any())
            {
                var err2 = $@"-------------------------------------------------------------------------
sheet='关联下一题' 存在以下错误: 

";
                err2 += string.Join("\n\n", Errors);
                Errors.Clear();
                Errors.Add(err2);
            }

            if (!err1.IsNullOrEmpty())
            {
                Errors.Insert(0, err1);
            }

            // ok for Sels and to DgAyToNextQuestion
            if (!Errors.Any())
            {
                DgAyToNextsLs.AddRange(Sels.Select(x =>
                {
                    var dto = new DgAyToNextQuestion();
                    dto.Id = Guid.NewGuid();
                    dto.IsValid = true;
                    dto.CreateTime = Now;
                    dto.Scid = x.Optid ?? x.Qid;
                    dto.Sctype = (byte)(x.Optid != null ? 1 : 2);
                    dto.NextQid = x.NextQid;
                    return dto;
                }));
            }
        }

        internal async Task On_read_xlsx_AddressAndPrimarySchool(List<string> Errors, string excelPath, DateTime Now)
        {
            using var excelpkg = TryGetExcelPackage(excelPath, out _);
            var sheet = excelpkg.Workbook.Worksheets["地址与对应对口小学"];
            if (sheet == null)
            {
                return;
            }
            foreach (var row1val in ExcelChecker.Sheet_AddressAndPrimarySchool())
            {
                if (!ExcelChecker.CheckRow1(sheet, row1val))
                {
                    Errors.Add($"第一列被修改了");
                    break;
                }
            }
            if (Errors.Any())
            {
                // 文档不符
                return;
            }

            for (var row = 2; row <= sheet.Dimension.Rows; row++)
            {
                if (CheckIfEmptyRow(sheet, row)) continue;
                var dto = new DgAyAddressAndPrimarySchool { Id = Guid.NewGuid(), IsValid = true };
                dto.Sort = row;
                dto.City = opt_city;

                var col_area = FindXlsxCellByRow1Field(sheet, 1, "区").Col;
                var areaName = FindXlsxRowValueByUpward(sheet, row, col_area).Value.CellStrValue();
                try
                {
                    dto.Area = (int)await GetCityArea(opt_cityName, areaName);
                }
                catch (Exception ex)
                {
                    dto.IsValid = false;
                    Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col_area)}列: {ex.Message}");
                }

                var x_address = FindXlsxCellByRow1Field(sheet, row, "地址");
                dto.Address = x_address.Value.CellStrValue();
                if (dto.Address.IsNullOrEmpty())
                {
                    dto.IsValid = false;
                    Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(x_address.Col)}列: 地址不能为空");
                }
                else if (dto.Area > 0)
                {
                    // 暂不支持判断 地址跟区 是否匹配
                }

                var x_eid = FindXlsxCellByRow1Field(sheet, row, "对口小学eid");
                dto.Eid = Guid.TryParse(x_eid.Value.CellStrValue(), out var _eid) ? _eid : (Guid?)null;
                if (dto.Eid == null || dto.Eid == Guid.Empty)
                {
                    dto.IsValid = false;
                    if (x_eid.Value.CellStrValue().IsNullOrEmpty()) Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(x_eid.Col)}列: 对口小学eid不能为空");
                    else Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(x_eid.Col)}列: 请录入正确的对口小学eid");
                }
                else // 是否是小学 
                {
                    var err = await CheckEid_is_PrimarySchool(dto.Eid.Value);
                    if (err != null)
                    {
                        dto.IsValid = false;
                        Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(x_eid.Col)}列: {err}");
                    }
                }
                // 一个区一个地址只能对应一个eid
                if (dto.Eid != null && Ls_aaps.Any(_ => _.Area == dto.Area && _.Address == dto.Address))
                {
                    dto.IsValid = false;
                    Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(x_eid.Col)}列: 一个区一个地址只能对应一个eid");
                }

                if (dto.IsValid) Ls_aaps.Add(dto);
            }
        }

        internal async Task On_read_xlsx_SchPcyFile(List<string> Errors, string excelPath, DateTime Now)
        {
            using var excelpkg = TryGetExcelPackage(excelPath, out _);
            var sheet = excelpkg.Workbook.Worksheets["政策文件"];
            if (sheet == null)
            {
                return;
            }
            var col1_1 = FindXlsxCellByRow1Field(sheet, 1, "小学入学方式").Col;
            var col1_2 = col1_1;
            if (col1_2 > 0)
            {
                foreach (var row1val in ExcelChecker.Sheet_SchPcyFile1())
                {
                    if (sheet.Cells[1, col1_2++].Value.CellStrValue() != row1val)
                    {
                        Errors.Add($"第一列 小学入学方式 部分被修改了");
                        break;
                    }
                }
            }
            var col2_1 = FindXlsxCellByRow1Field(sheet, 1, "初中升学").Col;
            var col2_2 = col2_1;
            if (col2_2 > 0)
            {
                foreach (var row1val in ExcelChecker.Sheet_SchPcyFile2())
                {
                    if (sheet.Cells[1, col2_2++].Value.CellStrValue() != row1val)
                    {
                        Errors.Add($"第一列 初中升学 部分被修改了");
                        break;
                    }
                }
            }
            if (Errors.Any())
            {
                // 文档不符
                return;
            }

            for (var row = 2; row <= sheet.Dimension.Rows; row++)
            {
                if (CheckIfEmptyRow(sheet, row, (_col, _v) =>
                {
                    var b = _col >= col1_1 && _col < col1_2;
                    if (!b) return true;
                    return _v == null || _v.ToString().IsNullOrEmpty();
                }))
                {
                    continue;
                }
                var dto = new DgAySchPcyFile { IsValid = true };
                dto.City = opt_city;
                var col = col1_1;
                {
                    var dto_type = EnumUtil.GetDescs<DgAySchPcyFileTypeEnum>().FirstOrDefault(_ => _.Desc == sheet.Cells[row, col].Value.CellStrValue()).Value;
                    dto.Type = dto_type.ToInt();
                    if (!(dto_type.In(DgAySchPcyFileTypeEnum.Cp, DgAySchPcyFileTypeEnum.Ov, DgAySchPcyFileTypeEnum.Jf)))
                    {
                        dto.IsValid = false;
                        Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col)}列: 小学入学方式 填错了");
                    }
                }
                col++;
                {
                    var areaName = sheet.Cells[row, col].Value.CellStrValue();
                    if (areaName.IsNullOrEmpty())
                    {
                        dto.IsValid = false;
                        Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col)}列: 没填写区");
                    }
                    else
                    {
                        try { dto.Area = (int)await GetCityArea(opt_cityName, areaName); }
                        catch (Exception ex)
                        {
                            dto.IsValid = false;
                            Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col)}列: {ex.Message}");
                        }
                    }
                }
                col++;
                {
                    dto.Year = int.TryParse(sheet.Cells[row, col].Value.CellStrValue(), out var _y) ? _y : -1;
                    if (dto.Year < 0)
                    {
                        dto.IsValid = false;
                        Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col)}列: 年份填错了");
                    }
                }
                col++;
                {
                    dto.Title = sheet.Cells[row, col].Value.CellStrValue();
                    if (dto.Title.IsNullOrEmpty())
                    {
                        dto.IsValid = false;
                        Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col)}列: 没填写小学政策");
                    }
                }
                col++;
                {
                    dto.Url = sheet.Cells[row, col].Value.CellStrValue();
                    if (dto.Url.IsNullOrEmpty())
                    {
                        dto.IsValid = false;
                        Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col)}列: 没填写url");
                    }
                    else if (!dto.Url.StartsWith("http://") && !dto.Url.StartsWith("https://"))
                    {
                        dto.IsValid = false;
                        Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col)}列: url填错了");
                    }
                }
                if (dto.IsValid)
                {
                    dto.Id = Guid.NewGuid();
                    Ls_SchPcyFiles.Add(dto);
                }
            }
            for (var row = 2; row <= sheet.Dimension.Rows; row++)
            {
                if (CheckIfEmptyRow(sheet, row, (_col, _v) =>
                {
                    var b = _col >= col2_1 && _col < col2_2;
                    if (!b) return true;
                    return _v == null || _v.ToString().IsNullOrEmpty();
                }))
                {
                    continue;
                }
                var dto = new DgAySchPcyFile { IsValid = true };
                dto.City = opt_city;
                var col = col2_1;
                {
                    var dto_type = EnumUtil.GetDescs<DgAySchPcyFileTypeEnum>().FirstOrDefault(_ => _.Desc == sheet.Cells[row, col].Value.CellStrValue()).Value;
                    dto.Type = dto_type.ToInt();
                    if (!(dto_type.In(DgAySchPcyFileTypeEnum.CpHeli, DgAySchPcyFileTypeEnum.CpPcAssign)))
                    {
                        dto.IsValid = false;
                        Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col)}列: 初中升学 填错了");
                    }
                }
                col++;
                {
                    var areaName = sheet.Cells[row, col].Value.CellStrValue();
                    if (areaName.IsNullOrEmpty())
                    {
                        dto.IsValid = false;
                        Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col)}列: 没填写区");
                    }
                    else
                    {
                        try { dto.Area = (int)await GetCityArea(opt_cityName, areaName); }
                        catch (Exception ex)
                        {
                            dto.IsValid = false;
                            Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col)}列: {ex.Message}");
                        }
                    }
                }
                col++;
                {
                    dto.Year = int.TryParse(sheet.Cells[row, col].Value.CellStrValue(), out var _y) ? _y : -1;
                    if (dto.Year < 0)
                    {
                        dto.IsValid = false;
                        Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col)}列: 年份填错了");
                    }
                }
                col++;
                {
                    dto.Title = sheet.Cells[row, col].Value.CellStrValue();
                    if (dto.Title.IsNullOrEmpty())
                    {
                        dto.IsValid = false;
                        Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col)}列: 没填写小升初政策");
                    }
                }
                col++;
                {
                    dto.Url = sheet.Cells[row, col].Value.CellStrValue();
                    if (dto.Url.IsNullOrEmpty())
                    {
                        dto.IsValid = false;
                        Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col)}列: 没填写url");
                    }
                    else if (!dto.Url.StartsWith("http://") && !dto.Url.StartsWith("https://"))
                    {
                        dto.IsValid = false;
                        Errors.Add($"{row}行{ExcelCellAddress.GetColumnLetter(col)}列: url填错了");
                    }
                }
                if (dto.IsValid)
                {
                    dto.Id = Guid.NewGuid();
                    Ls_SchPcyFiles.Add(dto);
                }
            }
        }

        internal async Task On_write_datas(string giid, Guid? userid)
        {
            var now = DateTime.Now;
            var gid = giid ?? $"{now:yyMMddHHmmssfff}{Guid.NewGuid().ToString().Replace("-", "")}";

            QuesLs.ForEach(x =>
            {
                x.CreateTime = now;
                x.Creator = userid;
            });
            OptsLs.ForEach(x =>
            {
                x.CreateTime = now;
                x.Creator = userid;
            });
            DgAyToNextsLs.ForEach(x =>
            {
                x.CreateTime = now;
                x.Creator = userid;
            });
            Ls_aaps.ForEach(x =>
            {
                x.CreateTime = now;
                x.Creator = userid;
            });

            var conn = _schoolUnitOfWork.DbConnection;

            await Policy.Handle<Exception>()
                .RetryForeverAsync(async ex =>
                {
                    //log.LogWarning(ex, "do backup error and retry later...");
                    await Task.Delay(1000);
                })
                .ExecuteAsync(async () =>
                {
                    var sql = "";
                    if (QuesLs.Any() || OptsLs.Any() || DgAyToNextsLs.Any())
                    {
                        sql += @"
insert DgAyTbLog(id,[time],userid,tbname,oldata,gid,method)
select newid(),@now,@userid,'DgAyQuestion',
(select * from DgAyQuestion where id=q.id for json path,WITHOUT_ARRAY_WRAPPER),
@gid,'导入'
from DgAyQuestion q 
---
insert DgAyTbLog(id,[time],userid,tbname,oldata,gid,method)
select newid(),@now,@userid,'DgAyQuestionOption',
(select * from DgAyQuestionOption where id=opt.id for json path,WITHOUT_ARRAY_WRAPPER),
@gid,'导入'
from DgAyQuestionOption opt
---
insert DgAyTbLog(id,[time],userid,tbname,oldata,gid,method)
select newid(),@now,@userid,'DgAyToNextQuestion',
(select * from DgAyToNextQuestion where id=nx.id for json path,WITHOUT_ARRAY_WRAPPER),
@gid,'导入'
from DgAyToNextQuestion nx
---";
                    }
                    if (Ls_aaps.Any())
                    {
                        sql += $@"
insert DgAyTbLog(id,[time],userid,tbname,oldata,gid,method)
select newid(),@now,@userid,'DgAyAddressAndPrimarySchool',
(select * from DgAyAddressAndPrimarySchool where id=d.id for json path,WITHOUT_ARRAY_WRAPPER),
@gid,'导入'
from DgAyAddressAndPrimarySchool d
---";
                    }
                    if (Ls_SchPcyFiles.Any())
                    {
                        sql += $@"
insert DgAyTbLog(id,[time],userid,tbname,oldata,gid,method)
select newid(),@now,@userid,'DgAySchPcyFile',
(select * from DgAySchPcyFile where id=d.id for json path,WITHOUT_ARRAY_WRAPPER),
@gid,'导入'
from DgAySchPcyFile d
---";
                    }
                    if (sql.IsNullOrEmpty()) return;
                    else
                    {
                        sql = $@"
delete from DgAyTbLog where gid=@gid
---
{sql}
";
                    }
                    await conn.ExecuteAsync(sql, new { gid, now, userid });
                });

            await Policy.Handle<Exception>()
                .RetryForeverAsync(async ex =>
                {
                    //log.LogWarning(ex, "do write error and retry later...");
                    await Task.Delay(1000);
                })
                .ExecuteAsync(async () =>
                {
                    using var ctx = new DbTranCtx(conn);
                    if (QuesLs.Any())
                    {
                        ctx.BeginTransaction();

                        var sql = $@"delete from DgAyQuestion";
                        await conn.ExecuteAsync(sql, null, ctx.CurrentTransaction);

                        await conn.InsertAsync(QuesLs, ctx.CurrentTransaction);

                        ctx.CommitTransaction();
                    }
                    if (OptsLs.Any())
                    {
                        ctx.BeginTransaction();

                        var sql = $@"delete from DgAyQuestionOption";
                        await conn.ExecuteAsync(sql, null, ctx.CurrentTransaction);

                        await conn.InsertAsync(OptsLs, ctx.CurrentTransaction);

                        ctx.CommitTransaction();
                    }
                    if (DgAyToNextsLs.Any())
                    {
                        ctx.BeginTransaction();

                        var sql = $@"delete from DgAyToNextQuestion";
                        await conn.ExecuteAsync(sql, null, ctx.CurrentTransaction);

                        await conn.InsertAsync(DgAyToNextsLs, ctx.CurrentTransaction);

                        ctx.CommitTransaction();
                    }
                    if (Ls_aaps.Any())
                    {
                        ctx.BeginTransaction();

                        var sql = $@"
update DgAyAddressAndPrimarySchool set IsValid=0, ModifyDateTime=@now, Modifier=@userid
where IsValid=1
";
                        await conn.ExecuteAsync(sql, new { now, userid }, ctx.CurrentTransaction);

                        await conn.InsertAsync(Ls_aaps, ctx.CurrentTransaction);

                        ctx.CommitTransaction();
                    }
                    if (Ls_SchPcyFiles.Any())
                    {
                        ctx.BeginTransaction();

                        var sql = $@"
update DgAySchPcyFile set IsValid=0, ModifyDateTime=@now, Modifier=@userid
where IsValid=1
";
                        await conn.ExecuteAsync(sql, new { now, userid }, ctx.CurrentTransaction);

                        await conn.InsertAsync(Ls_SchPcyFiles, ctx.CurrentTransaction);

                        ctx.CommitTransaction();
                    }
                });
        }

        async Task<long> GetCityArea(string cityName, string areaName)
        {
            while (true)
            {
                var conn = _schoolUnitOfWork.DbConnection;
                var sql = @"
select id from keyvalue where isvalid=1 and type=1 and left(name,2)=left(@areaName,2) and depth=3
and parentid=(select top 1 id from keyvalue where isvalid=1 and type=1 and left(name,2)=left(@cityName,2) and depth=2 )
";
                var (ls, ex) = await conn.QueryAsync<long>(sql, new { cityName, areaName }).AwaitResOrErr();
                if (ex != null)
                {
                    //log.LogError(ex, "get cityarea by name error.");
                    await Task.Delay(1000);
                    continue;
                }
                if (ls.Count() > 1) throw new Exception($"\n找到多个地区id: {string.Join(",", ls)}");
                if (ls.Count() == 0) throw new Exception($"找不到匹配的地区");
                return ls.First();
            }
        }

        async Task<string> CheckEid_is_PrimarySchool(Guid eid)
        {
            var conn = new System.Data.SqlClient.SqlConnection(_config["ConnectionStrings:SqlServerConnection-prod"]);
            return await Policy.Handle<Exception>()
                .RetryForeverAsync(async ex =>
                {
                    //log.LogWarning(ex, "do write error and retry later...");
                    await Task.Delay(1000);
                })
                .ExecuteAsync(async () =>
                {
                    var sql = $@"
select s.IsValid as IsValid1,e.IsValid as IsValid2,s.status,e.SchFtype
from school s join SchoolExtension e on s.id=e.sid
where e.id=@eid
";
                    var itm = await conn.QueryFirstOrDefaultAsync<(bool, bool, int Status, string SchFtype)>(sql, new { eid });
                    if (itm == default)
                    {
                        return "学部不存在";
                    }
                    var schftype = SchFType0.Parse(itm.SchFtype);
                    if (itm.SchFtype != "lx210")
                    {
                        // 地址匹配的小学类型不要限死是公办小学
                        //return "学部不是公办小学";
                    }
                    if (schftype.Grade != (byte)iSchool.Domain.Enum.SchoolGrade.PrimarySchool)
                    {
                        return "学部不是小学";
                    }
                    if (!(itm.Item1 && itm.Item2))
                    {
                        return "学部被删除了";
                    }
                    return null;
                });
        }

        IEnumerable<QaSel> FindNext(List<QaSel> ls, QaSel qa1)
        {
            if (qa1?.NextQid == null) yield break;
            var qas = Sels.Where(_ => _.Qid == qa1.NextQid).ToArray();
            foreach (var qa in qas)
            {
                ls.Add(qa);
                yield return qa;
                var c = ls.Count - 1;
                foreach (var _qa in FindNext(ls, qa))
                {
                    yield return _qa;
                }
                ls.RemoveRange(c, ls.Count - c);
            }
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

        class QaSel
        {
            public int Row { get; set; }
            public long Qid { get; set; }
            public long? Optid { get; set; }
            public long? NextQid { get; set; }
        }

        [DebuggerStepThrough]
        static T[] PArray<T>(params T[] arr) => arr;

        [DebuggerStepThrough]
        static IEnumerable<int> FromRange(int start, int end) => Enumerable.Range(start, end + 1 - start);


    }
}
