using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Authorization;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using SchoolGrade = iSchool.Domain.Enum.SchoolGrade;
using static iSchool.Infrastructure.ObjectHelper;

namespace iSchool.Application.Service.Jobs
{
    public class BaiduTuiGuang_realtime_CommandHandler : IRequestHandler<BaiduTuiGuang_realtime_Command>
    {
        public BaiduTuiGuang_realtime_CommandHandler(IServiceProvider sp, ILog log, IConfiguration config, IHttpClientFactory httpClientFactory,
            IUnitOfWork unitOfWork)
        {
            this.sp = sp;
            this.log = log;
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.config = config;
            this.http = httpClientFactory.CreateClient("baidu_push");
        }

        IConfiguration config;
        IServiceProvider sp;
        ILog log;
        UnitOfWork unitOfWork;
        HttpClient http;

        public async Task<Unit> Handle(BaiduTuiGuang_realtime_Command cmd, CancellationToken cancellationToken)
        {
            var sql = $@"
delete from Lyega_olschext_BaiduZZ_realtime where eid not in(select eid from Lyega_OLschextSimpleInfo)

insert Lyega_olschext_BaiduZZ_realtime
select s.eid,s.sid,s.schname,s.extname,e.Completion,s.ModifyTime,e.grade,e.type,e.discount,e.diglossia,e.chinese,s.SchFType,s.province,s.city,s.area,
(case when e.Completion>=0.9 then 'a' 
when e.Completion<0.9 and e.Completion>=0.8 then 'b++'
when e.Completion<0.8 and e.Completion>=0.7 then 'b+'
when not(e.grade in({SchoolGrade.PrimarySchool.ToInt()},{SchoolGrade.JuniorMiddleSchool.ToInt()}) and e.type={SchoolType.Public.ToInt()}) and e.Completion<0.7 and e.Completion>=0.6 then 'b'
when e.grade in({SchoolGrade.PrimarySchool.ToInt()},{SchoolGrade.JuniorMiddleSchool.ToInt()}) and e.type={SchoolType.Public.ToInt()} and e.Completion<0.7 and e.Completion>=0.4 then 'c'
when not(e.grade in({SchoolGrade.PrimarySchool.ToInt()},{SchoolGrade.JuniorMiddleSchool.ToInt()}) and e.type={SchoolType.Public.ToInt()}) and e.Completion<0.6 and e.Completion>=0.4 then 'c'
when e.Completion<0.4 then 'd'
else null end)as lv,
0 as IsDoned 
--into Lyega_olschext_BaiduZZ_realtime
from dbo.Lyega_OLschextSimpleInfo s
inner join SchoolExtension e on e.id=s.eid
where e.IsValid=1 and s.eid not in(select eid from Lyega_olschext_BaiduZZ_realtime)
";
            await Task.WhenAny(Task.Delay(1000 * 3), unitOfWork.DbConnection.ExecuteAsync(sql));
            await OnRunAsync(cmd);
            return default;
        }

        async Task OnRunAsync(BaiduTuiGuang_realtime_Command cmd)
        {
            config = config.GetSection("baidu-tuiguang:realtime");
            var purl = config["purl"];
            var _lv = cmd.Lv;
            var _city = cmd.City;
            var _burls = config.GetSection("burls").Get<Dictionary<string, string>>();
            await default(ValueTask);

            var conn = unitOfWork.DbConnection;
            var is_end = false;

            var sql = $@"
select top 1 * from Lyega_olschext_BaiduZZ_realtime 
where isdoned<{_burls.Count} {$"{_lv}".If(!string.IsNullOrEmpty(_lv))}
{$"and city in({_city})".If(!string.IsNullOrEmpty(_city))}
order by lv asc,Completion desc,ModifyTime desc
";
            while (!is_end)
            {
                Guid? eid = null;
                var i = 0;
                await Retry_to_do(async () =>
                {
                    var dy = await conn.QueryFirstOrDefaultAsync(sql);
                    eid = (Guid?)dy?.eid;
                    i = ((int?)dy?.IsDoned ?? 0) + 1;
                }, null, 1000, ex =>
                {
                    log.Warn(ex);
                });
                if (eid == null) break;

                for (; i <= _burls.Count; i++)
                {
                    var (success_realtime, remain_realtime) = await tui(eid.Value, i);
                    if (success_realtime != 0)
                    {
                        await Retry_to_do(async () =>
                        {
                            await conn.ExecuteAsync($"update Lyega_olschext_BaiduZZ_realtime set IsDoned={i} where eid=@id and IsDoned={i - 1} ;", new { id = eid.Value });
                        }, null, 1000, ex =>
                        {
                            log.Warn(ex);
                        });

                        log.Info($"百度推广realtime,剩余{remain_realtime}条, 学部id='{eid.Value}' {_burls.ElementAtOrDefault(i - 1).Key.Substring(_burls.ElementAtOrDefault(i - 1).Key.IndexOf('.') + 1)} ok.");
                    }
                    if (remain_realtime == 0)
                    {
                        is_end = true;
                        break;
                    }
                }
            }

            async Task<(int success_realtime, int remain_realtime)> tui(Guid ext, int ii)
            {
                var bys = Encoding.UTF8.GetBytes(_burls.ElementAtOrDefault(ii - 1).Value + ext.ToString("n"));
                var (success_realtime, remain_realtime) = (-1, -1);
                while (true)
                {
                    try
                    {
                        var req = new HttpRequestMessage(HttpMethod.Post, purl);
                        req.Content = new ByteArrayContent(bys);
                        req.Content.Headers.Set("Content-Type", "text/plain");

                        var res = await http.SendAsync(req);
                        res.EnsureSuccessStatusCode();
                        var r = (await res.Content.ReadAsStringAsync()).ToObject<JToken>();

                        if (r["error_code"] != null) throw new FnResultException((int)r["error_code"], (string)r["error_msg"]);
                        if (r["error"] != null) throw new FnResultException((int)r["error"], (string)r["message"]);
                        remain_realtime = Tryv(() => (int)r["remain_realtime"], -1);
                        success_realtime = Tryv(() => (int)r["success_realtime"], -1);
                        break;
                    }
                    catch (Exception ex)
                    {
                        log.Error("百度推广api-realtime失败", ex);
                        await Task.Delay(1200);
                    }
                }
                return (success_realtime, remain_realtime);
            }
        }

        static async Task<Exception> Retry_to_do(Func<Task> func, int? c, int ms, Action<Exception> onerror = null)
        {
            var cc = c == null ? c : c < 1 ? 1 : c;
            for (var i = 1; (cc == null || i <= cc); i++)
            {
                try
                {
                    await func();
                    break;
                }
                catch (FnResultException ex)
                {
                    onerror?.Invoke(ex);
                    return ex;
                }
                catch (Exception ex)
                {
                    onerror?.Invoke(ex);
                    if (cc != null && i == cc.Value) return ex;
                    else if (ms > 0) await Task.Delay(ms);
                }
            }
            return null;
        }
    }
}
