using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Application.ApiService.Schools;
using iSchool.Application.Service;
using iSchool.Application.Service.OnlineSchool;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iSchool.Api.Controllers
{
    /// <summary>
    /// !!!外网显示的学校=内网录入的学部onlineschoolextension
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class SchoolController : Controller
    {
        IMediator _mediator;

        public SchoolController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 根据id查询学校
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(GetByIds))]
        public IFnResult GetByIds([FromBody]Guid[] ids)
        {
            var req = new OESchoolQuery { Ids = ids };
            var res = _mediator.Send(req).Result;
            return FnResult.OK(res);
        }

        /// <summary>
        /// 根据名称模糊查询学校
        /// </summary>
        /// <returns></returns>
        [HttpGet(nameof(GetByName))]
        public IFnResult GetByName([FromQuery]OESchoolNameQuery req)
        {
            var res = _mediator.Send(req).Result;
            return FnResult.OK(res);
        }

        /// <summary>
        /// 根据名字获取学校
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        [HttpPost(nameof(GetlistByName))]
        public IFnResult GetlistByName([FromBody]string[] names)
        {
            var res = _mediator.Send(new OESchoolNameListQuery { Names = names }).Result;
            return FnResult.OK(res);
        }

        /// <summary>
        /// 根据招录学校/部id查询升学到本校的学校/部人数
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(GetAchievementsBy))]
        public async Task<FnResult<RecruitSchoolQueryResult[]>> GetAchievementsBy([FromBody]RecruitSchoolQuery req)
        {
            var res = await _mediator.Send(req);
            return FnResult.OK(res);
        }

        /// <summary>
        /// 根据最终类型获取学校
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost(nameof(GetByFinalType))]
        public async Task<IFnResult> GetByFinalType(OESchoolByFTypeQuery req)
        {
            var res = await _mediator.Send(req);
            return FnResult.OK(res);
        }
    }
}