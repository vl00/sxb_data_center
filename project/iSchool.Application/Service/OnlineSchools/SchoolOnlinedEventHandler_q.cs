using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Application.Service.OnlineSchools;
using iSchool.BgServices;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace iSchool.Application.Service
{
    public class SchoolOnlinedEventHandler_q : INotificationHandler<SchoolOnlinedEvent>
    {
        IServiceProvider _sp;

        public SchoolOnlinedEventHandler_q(IServiceProvider sp)
        {
            this._sp = sp;
        }

        public Task Handle(SchoolOnlinedEvent e, CancellationToken cancellationToken)
        {
            return Handle_core(_sp, e);
        }

        static async Task Handle_core(IServiceProvider sp, SchoolOnlinedEvent e)
        {
            var log = sp.GetService<ILog>();
            var rabbit = sp.GetService<RabbitMQConnectionForPublish>();
            var unitOfWork = sp.GetService<IUnitOfWork>() as UnitOfWork;
            var config = sp.GetService<IConfiguration>();
            await Task.CompletedTask;

            var sql = $@"
select e.Sid as Item1,e.id as Item2,s.Name as Item3,e.name as Item4,e.IsValid as Item5
from OnlineSchool s 
inner join OnlineSchoolExtension e on s.id=e.sid
where s.id=@sid and s.[status]={SchoolStatus.Success.ToInt()} and s.IsValid=1
";
            var qs = unitOfWork.DbConnection.Query<(Guid Sid, Guid Eid, string SchName, string ExtName, bool IsValid)>(sql, e).ToArray();
            if (qs.Length <= 0) return;

            var x = new
            {
                sid = e.Sid,
                SchName = qs.Length > 0 ? qs[0].SchName : "",
                Exts = qs.Select(_ => new
                {
                    _.Eid,
                    _.ExtName,
                    _.IsValid,
                })
                .ToArray(),
                t = DateTime.Now,
            };

            
            try
            {
                using (var chanel = rabbit.OpenChannel())
                {
                    var bys = Encoding.UTF8.GetBytes(x.ToJsonString(true));
                    var props = chanel.CreateBasicProperties();
                    props.DeliveryMode = 2;
                    props.Type = "iSchool.Application.Service.SchoolOnlinedEvent";
                    props.Headers = props.Headers ?? new Dictionary<string, object>();
                    props.Headers["rr"] = $"lyega_{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}";
                    foreach (var cheader in config.GetSection("rabbit:pubs:schonoff:headers").GetChildren())
                    {
                        if (cheader.Value != null)
                            props.Headers[cheader.Key] = cheader.Value;
                    }

                    var msg = log.NewMsg(LogLevel.Info);
                    msg.Sid = e.Sid.ToString();
                    msg.Content = $"学校上线时rabbitmq推送ing...";
                    log.LogMsg(msg);
                    chanel.ConfirmSelect();
                    chanel.BasicPublish(config["rabbit:pubs:schonoff:exchange"], config["rabbit:pubs:schonoff:routeKey"], props, bys);
                    if (!chanel.WaitForConfirms()) throw new Exception("nack recv");
                    msg = log.NewMsg(LogLevel.Info);
                    msg.Sid = e.Sid.ToString();
                    msg.Content = $"学校上线时rabbitmq推送ok.";
                    log.LogMsg(msg);
                }
            }
            catch (Exception ex)
            {
                var msg = log.NewMsg(LogLevel.Error);
                msg.Sid = e.Sid.ToString();
                msg.Content = $"学校上线时rabbitmq推送出错了";
                msg.ErrorCode = $"{15672}";
                msg.Error = ex.Message;
                msg.StackTrace = ex.StackTrace;
                log.LogMsg(msg);                
            }

            try
            {
                log.Info($"学校sid={e.Sid}上线-->publishing event='OnlineSchools.SchextsOnlinedEvent'");
                await sp.GetService<IMediator>().Publish(new OnlineSchools.SchextsOnlinedEvent
                {
                    Exts = x.Exts.Select(_ => (x.sid, _.Eid, _.IsValid)).ToArray(),
                    T = e.T,
                });
            }
            catch { }
        }
    }
}
