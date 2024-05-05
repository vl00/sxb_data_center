using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.OrgService_bg.RequestModels;
using iSchool.Organization.Appliaction.OrgService_bg.ResponseModels;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service;
using iSchool.Organization.Appliaction.Service.Organization;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.UI.ApiControllers
{
    /// <summary>
    /// 后台用户
    /// </summary>
    [Route("api/bg/user/[action]")]
    [ApiController]
    public class BgUserController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration config;

        public BgUserController(IMediator mediator, IConfiguration config)
        {
            _mediator = mediator;
            this.config = config;
        }

        /// <summary>
        /// get后台用户信息
        /// </summary>
        /// <returns></returns>
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(BgUserInfo), 200)]
        public async Task<Res2Result> Info()
        {
            await default(ValueTask);
            var userInfo = HttpContext.GetUserInfo();
            var r = new BgUserInfo();
            if (userInfo != null)
            {
                r.Id = userInfo.Id;
                r.Displayname = userInfo.Displayname;
            }
            return Res2Result.Success(r);
        }
    }
}
