using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Application.Service.OnlineSchools;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Application.ViewModels;
using iSchool.Domain;
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
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace iSchool.Application.Service
{
    public class SchoolOnlinedEventHandler_S3to7Empty //: INotificationHandler<SchoolOnlinedEvent>
    {
        IServiceProvider sp;

        public SchoolOnlinedEventHandler_S3to7Empty(IServiceProvider sp)
        {
            this.sp = sp.CreateScope().ServiceProvider;
        }

        public Task Handle(SchoolOnlinedEvent e, CancellationToken cancellationToken)
        {
            SimpleQueue.Default.Enqueue(Handle_core);
            
            async void Handle_core()
            {
                var log = sp.GetService<ILog>();
                var http = sp.GetService<IHttpClientFactory>().CreateClient(string.Empty);
                var appsettings = sp.GetService<AppSettings>();
                var config = sp.GetService<IConfiguration>();
                var mediator = sp.GetService<IMediator>();
                try
                {
                    //var req = new HttpRequestMessage(HttpMethod.Post, $"{appsettings.DataApiUrl}/jobs/CmptCheckOnlineSchoolS3to7Empty");
                    //req.Headers.Set(config["auth:jobs:HeaderKey"], $"{config["auth:jobs:U"]},{config["auth:jobs:P"]}");
                    //req.Content = new StringContent(new { sid = e.Sid }.ToJsonString(), Encoding.UTF8, "application/json");
                    //var res = await http.SendAsync(req);
                    //res.EnsureSuccessStatusCode();

                    await mediator.Send(new Jobs.CheckOnlineSchoolS3to7Empty_Command { Sid = e.Sid });
                }
                catch (Exception ex)
                {
                    //log.Error(new FnResultException(-37, "S3to7Empty", ex)); //not work
                    log.Error("S3to7Empty", ex, -37);
                }
                finally
                {
                    sp.Dispose();
                }
            }

            return Task.CompletedTask;
        }
    }
}
