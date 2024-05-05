using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool
{
    public class SmLogUserOperationMiddleware
    {
        readonly RequestDelegate next;

        public SmLogUserOperationMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try { await next(context); }
            finally 
            {
                var services = context.RequestServices;
                var smLogUserOperation = services.GetService<SmLogUserOperation>();
                if (smLogUserOperation?.Time != null && smLogUserOperation.UserId != default)
                {
                    if (smLogUserOperation.Params != null || smLogUserOperation.Oldata != null)
                        AsyncUtils.StartNew(smLogUserOperation);
                }
            }
        }

    }
}
