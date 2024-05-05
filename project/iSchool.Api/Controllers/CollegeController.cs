using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Application.Service;
using iSchool.Application.Service.Colleges;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iSchool.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class CollegeController : Controller
    {
        IMediator _mediator;

        public CollegeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 根据id查询大学
        /// </summary>
        /// <returns></returns>
        [HttpGet(nameof(GetByIds))]
        public IFnResult GetByIds([FromQuery]Guid[] ids)
        {
            var req = new CollegeQuery { Ids = ids };
            var res = _mediator.Send(req).Result;
            return FnResult.OK(res);
        }
        /// <summary>
        /// 根据id查询大学
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(GetByIds))]
        public IFnResult GetByIds2([FromBody]Guid[] ids)
        {
            var req = new CollegeQuery { Ids = ids };
            var res = _mediator.Send(req).Result;
            return FnResult.OK(res);
        }

        /// <summary>
        /// 根据名称模糊查询大学
        /// </summary>
        /// <returns></returns>
        [HttpGet(nameof(GetByName))]
        public IFnResult GetByName([FromQuery]CollegeNameQuery req)
        {
            var res = _mediator.Send(req).Result;
            return FnResult.OK(res);
        }

        /// <summary>
        /// 根据名字获取大学
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        [HttpPost(nameof(GetlistByName))]
        public IFnResult GetlistByName([FromBody]string[] names)
        {
            var res = _mediator.Send(new CollegeNameListQuery { Names = names }).Result;
            return FnResult.OK(res);
        }
    }
}