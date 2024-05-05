using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Linq;
using iSchool.Application.Service.OnlineSchool;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;

namespace iSchool.Application.Service
{
    public class AfterSchoolOffDelEventHandler_delcache : INotificationHandler<OnlineSchools.SchextNoValidEvent>
    {
        CSRedis.CSRedisClient redis;
        ILog log;

        public AfterSchoolOffDelEventHandler_delcache(ILog log, CSRedis.CSRedisClient redis)
        {
            this.log = log;
            this.redis = redis;
        }

        public async Task Handle(OnlineSchools.SchextNoValidEvent e, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    var p = redis.StartPipe();
                    foreach (var g in e.Exts.GroupBy(_ => _.Sid))
                    {
                        var sid = g.Key;
                        p.Del($"ext:adData:{sid}");
                        p.Del($"ext:adTag:{sid}");
                    }
                    p.EndPipe();
                });
            }
            catch (Exception ex)
            {
                log.Error($"redis delete key error", ex, 2);
            }

            try
            {
                await Task.Factory.StartNew(() =>
                {
                    var p = redis.StartPipe();
                    foreach (var (sid, eid) in e.Exts)
                    {
                        p.Del($"ext:sid:{eid}");
                    }
                    p.EndPipe();
                });
            }
            catch (Exception ex)
            {
                log.Error($"redis delete key error", ex, 2);
            }

            try
            {
                await Task.Factory.StartNew(() =>
                {
                    var p = redis.StartPipe();
                    foreach (var (sid, eid) in e.Exts)
                    {
                        p.Del($"splext:{eid.ToString("n")}");
                        p.Del($"splext:score:{eid.ToString("n")}");
                    }
                    p.EndPipe();
                });
            }
            catch (Exception ex)
            {
                log.Error($"redis delete key error", ex, 2);
            }
        }
    }
}
