using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Application.Service.OnlineSchools;
using System.Buffers;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

namespace iSchool.Application.Service
{
    public class SchextsOnlinedEventHandler_baidu : INotificationHandler<SchextsOnlinedEvent>
    {
        ILog log;
        HttpClient http;
        IConfiguration config;

        public SchextsOnlinedEventHandler_baidu(ILog log, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            this.log = log;
            this.config = config;
            http = httpClientFactory.CreateClient("baidu_push");
        }

        public async Task Handle(SchextsOnlinedEvent e, CancellationToken cancellationToken)
        {
            var baseUrl = new[]
            {
                ("https://www.sxkid.com/school/detail/", config["baidu-tuiguang:batch:purl"]),
                ("https://m.sxkid.com/school/detail/", config["baidu-tuiguang:batch:purl"]),
            };

            foreach (var (burl, purl) in baseUrl)
            {
                var str = string.Join('\n', e.Exts.Where(_ => _.IsValid).Select(_ => burl + _.Eid.ToString("n")));
                var bys = Encoding.UTF8.GetBytes(str);
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
                    if (((int?)r["success"] ?? 0) <= 0) throw new FnResultException(-1, "学校上线后call百度推送成功0条");
                }
                catch (Exception ex)
                {
                    log.Error("学校上线后call百度推送api失败", ex);
                }
            }

        }
    }
}
