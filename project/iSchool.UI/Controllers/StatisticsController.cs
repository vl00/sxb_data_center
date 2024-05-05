using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.UI.Controllers
{


    [AllowAnonymous]
    public class StatisticsController : Controller
    {
        private readonly IMediator _mediator;

        public StatisticsController(IMediator mediator)
        {
            _mediator = mediator;
        }



        /// <summary>
        /// 销售数据数据统计
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Sales(int day)
        {
            var data = await _mediator.Send(new SalesStatisticsQuery { Day = day });
            return Json(ResponseResult.Success(data));

        }
    }
}
