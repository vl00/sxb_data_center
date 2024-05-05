using CSRedis;
using iSchool.Api.ModelBinders;
using iSchool.BgServices;
using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Infras;
using iSchool.Infrastructure;
using iSchool.Application.Service;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace iSchool.UI.ApiControllers
{
    [Route("api/test2/[Action]")]
    [ApiController]
    public class Test2Controller : ControllerBase
    {
        readonly IMediator _mediator;
        readonly CSRedisClient redis;

        public Test2Controller(CSRedisClient redis,
            IMediator _mediator)
        {
            this._mediator = _mediator;
            this.redis = redis;
        }

        /// <summary>
        /// clear caches
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<Res2Result> ClearCaches(string[] keys)
        {
            await _mediator.Send(new BgClearRedisCacheCmd
            {
                Keys = keys
            });
            return Res2Result.Success();
        }
    }
}
