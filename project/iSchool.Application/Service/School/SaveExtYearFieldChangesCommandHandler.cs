using Dapper;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Application.Service
{
    public class SaveExtYearFieldChangesCommandHandler : IRequestHandler<SaveExtYearFieldChangesCommand>
    {
        IMediator _mediator;
        UnitOfWork _unitOfWork;
        ILog log;
        static readonly SemaphoreSlim ssm = new SemaphoreSlim(1, 1);

        public SaveExtYearFieldChangesCommandHandler(IMediator mediator, IUnitOfWork unitOfWork, ILog log)
        {
            _mediator = mediator;
            _unitOfWork = (UnitOfWork)unitOfWork;
            this.log = log;
        }

        public async Task<Unit> Handle(SaveExtYearFieldChangesCommand cmd, CancellationToken cancellationToken)
        {
            if (cmd.Yearslist == null || !cmd.Yearslist.Any()) return default;

            var loadedyears = new HashSet<int>();
            foreach (var yearchange in YearChange.CombinChange(cmd.Yearslist))
            {
                //Check if the year is legal
                if (yearchange.Year.ToString().Length < 4) continue;

                // try to create tb 'SchoolYearFieldContent_{Year}'
                if (loadedyears.Add(yearchange.Year))
                {
                    try
                    {
                        await ssm.WaitAsync(cancellationToken);
                        await _mediator.Send(new Jobs.ExecSqlFileCommand { Fname = "try_to_create_part_tb_SchoolYearFieldContent", Args = new[] { yearchange.Year.ToString() } });
                    }
                    finally
                    {
                        ssm.Release();
                    }
                }

                var sql = "select top 1 * from YearExtField where eid=@eid and field=@field";
                var yrs = await _unitOfWork.DbConnection.QueryFirstOrDefaultAsync<YearExtField>(sql, new DynamicParameters(yearchange).Set("eid", cmd.Eid));
                var oldLatest = yrs?.Latest;

                try
                {
                    switch (1)
                    {
                        case 1 when yearchange.Act == YearChange.Act_remove: //del
                            {
                                sql = $"update SchoolYearFieldContent_{yearchange.Year} set IsValid=0 where year=@year and eid=@eid and field=@field";
                                await _unitOfWork.DbConnection.ExecuteAsync(sql, new DynamicParameters(yearchange).Set("eid", cmd.Eid));

                                if (yrs != null)
                                {
                                    var arr = yrs.Years == null ? new List<int>() : yrs.Years.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(_ => int.Parse(_)).ToList();
                                    arr.RemoveAll(_ => _ == yearchange.Year);
                                    yrs.Years = string.Join(',', arr);
                                    yrs.Latest = !arr.Any() ? (int?)null : arr.Max();

                                    sql = !arr.Any() ? "delete from YearExtField where eid=@eid and field=@field"
                                        : (@"update YearExtField set years=@years,latest=@latest where eid=@eid and field=@field");
                                    await _unitOfWork.DbConnection.ExecuteAsync(sql, yrs);
                                }
                            }
                            break;
                        case 1 when yearchange.Act < 0: //not exists at all
                            break;
                        default: // add + update
                            {
                                sql = $@"
merge into SchoolYearFieldContent_{yearchange.Year} y 
using (select @eid as eid, @field as field, @year as year) e on y.eid=e.eid and y.field=e.field and y.year=e.year
when not matched then
	insert (id,eid,field,year,content,IsValid) values(newid(),@eid,@field,@year,@content,1)
when matched then
	update set y.content=@content,y.IsValid=1
;";
                                await _unitOfWork.DbConnection.ExecuteAsync(sql, new DynamicParameters(yearchange).Set("eid", cmd.Eid));

                                if (yrs == null) yrs = new YearExtField { Eid = cmd.Eid, Field = yearchange.Field };
                                // up years+latest
                                {
                                    var arr = string.IsNullOrEmpty(yrs.Years) ? Enumerable.Repeat(yearchange.Year, 1)
                                        : yrs.Years.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(_ => int.Parse(_)).Union(Enumerable.Repeat(yearchange.Year, 1)).Distinct().OrderBy(_ => _);

                                    yrs.Years = string.Join(',', arr);
                                    yrs.Latest = !arr.Any() ? (int?)null : arr.Max();
                                }
                                sql = $@"
merge into YearExtField y 
using (select @eid as eid, @field as field) e on y.eid=e.eid and y.field=e.field
when not matched then
	insert (eid,field,years,latest) values(@eid,@field,@years,@latest)
when matched then
	update set y.years=@years,y.latest=@latest
;";
                                await _unitOfWork.DbConnection.ExecuteAsync(sql, yrs);
                            }
                            break;
                    }

                    // update lastest-year-datas to base-db-table
                    // 调用前必须表中先有一条数据
                    if (oldLatest != yrs?.Latest || yrs?.Latest == yearchange.Year)
                    {
                        var yf = GetAllFields().Where(_ => string.Equals(_.e, yearchange.Field, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (yf != default && yf.f != null)
                        {
                            sql = $@"
update ex set ex.{yf.e}={(yrs?.Latest == null ? "null" : $"isnull((select top 1 {(yf.dbty.IsNullOrEmpty() ? "y.content" : $"try_convert({yf.dbty},y.content)")} from SchoolYearFieldContent_{yrs.Latest} y where y.IsValid=1 and y.eid=ex.eid and y.field='{yf.f}'),ex.{yf.e})")}
from {yf.tb} ex
where ex.IsValid=1 and ex.eid='{yrs.Eid}'
";
                            await _unitOfWork.DbConnection.ExecuteAsync(sql);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.LogMsg(log.NewMsg(LogLevel.Error).Todo(logmsg =>
                    {
                        logmsg.Sid = $"{cmd.Sid}";
                        logmsg.Content = "年份字段更新出错";
                        logmsg.Error = $"年份字段更新出错 {ex.Message}";
                        logmsg.ErrorCode = "202020";
                        logmsg.StackTrace = ex.StackTrace;
                    }));
                }
            }

            return Unit.Value;
        }
        
        static IEnumerable<(string e, string tb, string f, string dbty)> GetAllFields()
        {
            yield return (e: "Age", tb: "dbo.SchoolExtRecruit",
                f: SchoolExtFiledYearTag.Age.GetName(), dbty: "real");

            yield return (e: "MaxAge", tb: "dbo.SchoolExtRecruit",
                f: SchoolExtFiledYearTag.MaxAge.GetName(), dbty: "real");

            yield return (e: "Count", tb: "dbo.SchoolExtRecruit",
                f: SchoolExtFiledYearTag.Count.GetName(), dbty: "int");

            yield return (e: "Data", tb: "dbo.SchoolExtRecruit",
                f: SchoolExtFiledYearTag.Data.GetName(), dbty: "");

            yield return (e: "Contact", tb: "dbo.SchoolExtRecruit",
                f: SchoolExtFiledYearTag.Contact.GetName(), dbty: "");

            yield return (e: "Scholarship", tb: "dbo.SchoolExtRecruit",
                f: SchoolExtFiledYearTag.Scholarship.GetName(), dbty: "");

            yield return (e: "Target", tb: "dbo.SchoolExtRecruit",
                f: SchoolExtFiledYearTag.Target.GetName(), dbty: "");

            yield return (e: "Subjects", tb: "dbo.SchoolExtRecruit",
                f: SchoolExtFiledYearTag.Subjects.GetName(), dbty: "");

            yield return (e: "Pastexam", tb: "dbo.SchoolExtRecruit",
                f: SchoolExtFiledYearTag.Pastexam.GetName(), dbty: null);

            yield return (e: "Date", tb: "dbo.SchoolExtRecruit",
                f:SchoolExtFiledYearTag.Date.GetName(),dbty:"");

            yield return (e: "Point", tb: "dbo.SchoolExtRecruit",
                f: SchoolExtFiledYearTag.Point.GetName(), dbty: "");

            yield return (e: "Counterpart", tb: "dbo.SchoolExtContent",
                f: SchoolExtFiledYearTag.Counterpart.GetName(), dbty: "");

            yield return (e: "Otherfee", tb: "dbo.SchoolExtCharge",
                f: SchoolExtFiledYearTag.Otherfee.GetName(), dbty: "");

            yield return (e: "Range", tb: "dbo.SchoolExtContent",
                f: SchoolExtFiledYearTag.Range.GetName(), dbty: null);

            yield return (e: "Applicationfee", tb: "dbo.SchoolExtCharge",
                f: SchoolExtFiledYearTag.Applicationfee.GetName(), dbty: "float");

            yield return (e: "Tuition", tb: "dbo.SchoolExtCharge",
                f: SchoolExtFiledYearTag.Tuition.GetName(), dbty: "float");
        }
    }
}
