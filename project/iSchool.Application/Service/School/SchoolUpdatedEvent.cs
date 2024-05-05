using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace iSchool.Application.Service
{
    public class SchoolUpdatedEvent : INotification
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Time { get; set; } = DateTime.Now;

        public Guid Sid { get; set; }
        public Guid? Eid { get; set; }
        public SchoolStatus? SchoolStatus { get; set; }
        public Guid? UserId { get; set; }
        public bool IsDeleted { get; set; } = false;
    }

    public class SchoolUpdatedEventHandler : INotificationHandler<SchoolUpdatedEvent>
    {
        IMediator mediator;
        IServiceProvider sp;
        IHttpContextAccessor httpContextAccessor;
        AppSettings appSettings;
        HttpContext HttpContext => httpContextAccessor.HttpContext;

        //static readonly ConcurrentDictionary<Guid, SchoolUpdatedEvent> kvGids = new ConcurrentDictionary<Guid, SchoolUpdatedEvent>();

        public SchoolUpdatedEventHandler(IMediator mediator, IServiceProvider sp, AppSettings appSettings,
            IHttpContextAccessor httpContextAccessor)
        {
            this.mediator = mediator;
            this.sp = sp;
            this.httpContextAccessor = httpContextAccessor;
            this.appSettings = appSettings;
        }

        public async Task Handle(SchoolUpdatedEvent e, CancellationToken cancellation)
        {
            e.UserId = e.UserId ?? HttpContext.GetUserId();
            await default(ValueTask);

            if (!e.IsDeleted)
            {
                ///更新菜单缓存
                if (e.Eid != null && e.SchoolStatus.In(null, SchoolStatus.Edit, SchoolStatus.Failed))
                {
                    await mediator.Send(new GetSchoolExtMenuQuery { ExtId = e.Eid.Value, Reset = true });
                }
            }
            else
            {
                ///学校|学部被删除后
                //...
            }

            ///update ES
            do
            {
                var gid = e.Eid ?? e.Sid;
                //var gid = e.Sid;

                //if (kvGids.TryAdd(gid, e))
                //{
                //    _ = Task.Delay(1000 * 1).ContinueWith((_0) =>
                //    {
                //        kvGids.TryRemove(gid, out _);
                //    });

                //    _ = update_ES(sp, appSettings, e);
                //}
                _ = update_ES(sp, appSettings, e);
            }
            while (false);
        }

        static async Task update_ES(IServiceProvider sp, AppSettings appSettings, SchoolUpdatedEvent e)
        {
            var httpFactory = sp.GetService<IHttpClientFactory>();
            var log = sp.GetService<ILog>();

            using (var http = httpFactory.CreateClient(string.Empty))
            {
                HttpResponseMessage res = null;
                if (e.IsDeleted && e.Eid == null)
                {
                    res = await http.PostAsync($"{appSettings.ESearchUrl}/BGSchoolData/DeleteSchByIds",
                        new StringContent((new[] { e.Sid }).ToJsonString(), Encoding.UTF8, "application/json")
                    );
                }
                else
                {
                    res = await http.PostAsync($"{appSettings.ESearchUrl}/BGSchoolData/SchNewestData",
                        new StringContent((new[] { e.Sid }).ToJsonString(), Encoding.UTF8, "application/json")
                    );
                }
                try
                {
                    res.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    log.LogMsg(log.NewMsg(LogLevel.Error).Todo(logMsg =>
                    {
                        logMsg.Sid = e.Sid.ToString();
                        logMsg.Content = "up学校后通知es出错";
                        logMsg.Error = ex.Message;
                        logMsg.ErrorCode = "15500";
                        logMsg.StackTrace = ex.StackTrace;
                    }));
                    return;
                }
                var fr = (await res.Content.ReadAsStringAsync()).ToObject<FnResult<object>>();
                if (!fr.IsOk)
                {
                    log.LogMsg(log.NewMsg(LogLevel.Error).Todo(logMsg =>
                    {
                        logMsg.Sid = e.Sid.ToString();
                        logMsg.Content = "up学校后通知es出错";
                        logMsg.Error = fr.Msg;
                        logMsg.ErrorCode = "15500";
                    }));
                    return;
                }
                log.LogMsg(log.NewMsg(LogLevel.Info).Todo(logMsg =>
                {
                    logMsg.Sid = e.Sid.ToString();
                    logMsg.Content = "up学校后通知es成功";
                }));
            }
        }
    }
}
