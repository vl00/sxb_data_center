using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
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
using iSchool.Application.Service.OnlineSchool;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.BgServices;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Configuration;

namespace iSchool.Application.Service
{
    public class AfterSchoolOffDelEventHandler : INotificationHandler<AfterSchoolOffDelEvent>
    {
        IServiceProvider sp;
        IMediator mediator;

        public AfterSchoolOffDelEventHandler(IServiceProvider sp, IMediator mediator)
        {
            this.sp = sp;
            this.mediator = mediator;
        }

        public async Task Handle(AfterSchoolOffDelEvent e, CancellationToken cancellationToken)
        {
            await Handle_q(e);
            await mediator.Publish(new SchoolUpdatedEvent { Sid = e.Sid, IsDeleted = !e.IsOff, SchoolStatus = e.IsOff ? SchoolStatus.Failed : (SchoolStatus?)null });
        }

        async Task Handle_q(AfterSchoolOffDelEvent e)
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
where s.id=@sid {$"and s.[status]={SchoolStatus.Failed.ToInt()}".If(e.IsOff)} {"and s.IsValid=0".If(!e.IsOff)}
";
            var qs = unitOfWork.DbConnection.Query<(Guid Sid, Guid Eid, string SchName, string ExtName, bool IsValid)>(sql, e).ToArray();

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
                    props.Type = e.IsOff ? "iSchool.Application.Service.SchoolOfflineEvent" : "iSchool.Application.Service.OnlineSchools.DelOnlineSchoolEvent";
                    props.Headers = props.Headers ?? new Dictionary<string, object>();
                    props.Headers["rr"] = $"lyega_f{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}";
                    foreach (var cheader in config.GetSection("rabbit:pubs:schonoff:headers").GetChildren())
                    {
                        if (cheader.Value != null)
                            props.Headers[cheader.Key] = cheader.Value;
                    }

                    var msg = log.NewMsg(LogLevel.Info);
                    msg.Sid = e.Sid.ToString();
                    msg.Content = $"学校下线时rabbitmq推送ing...";
                    log.LogMsg(msg);
                    chanel.ConfirmSelect();
                    chanel.BasicPublish(config["rabbit:pubs:schonoff:exchange"], config["rabbit:pubs:schonoff:routeKey"], props, bys);
                    if (!chanel.WaitForConfirms()) throw new Exception("nack recv");
                    msg = log.NewMsg(LogLevel.Info);
                    msg.Sid = e.Sid.ToString();
                    msg.Content = $"学校下线时rabbitmq推送ok.";
                    log.LogMsg(msg);
                }
            }
            catch (Exception ex)
            {
                var msg = log.NewMsg(LogLevel.Error);
                msg.Sid = e.Sid.ToString();
                msg.Content = $"学校下线时rabbitmq推送出错了";
                msg.ErrorCode = $"{15672}";
                msg.Error = ex.Message;
                msg.StackTrace = ex.StackTrace;
                log.LogMsg(msg);
            }

            try
            {
                log.Info($"学校sid={e.Sid}{(e.IsOff ? "下线" : "被删除")}-->publishing event='OnlineSchools.SchextNoValidEvent'");

                await mediator.Publish(new OnlineSchools.SchextNoValidEvent
                {
                    Exts = x.Exts.Select(_ => (x.sid, _.Eid)).ToArray()
                });
            }
            catch { }
        }
    }
}
