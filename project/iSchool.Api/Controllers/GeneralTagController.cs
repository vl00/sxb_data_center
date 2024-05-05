using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Application.Service;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace iSchool.Api.Controllers
{
    /// <summary>
    /// 测试api而添加[AllowAnonymous]
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class GeneralTagController : Controller
    {
        IMediator _mediator;

        public GeneralTagController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 根据id查标签
        /// </summary>
        /// <returns></returns>
        [HttpGet(nameof(GetByIds))]
        public FnResult<GeneralTag[]> GetByIds([FromQuery]Guid[] ids)
        {
            var req = new FindGeneralTagsByIdsQuery { Ids = ids };
            var res = _mediator.Send(req).Result;
            return FnResult.OK(res);
        }
        [HttpPost(nameof(GetByIds))]
        public FnResult<GeneralTag[]> GetByIds_post([FromBody]Guid[] ids) => GetByIds(ids);

        /// <summary>
        /// 根据name模糊查标签
        /// </summary>
        /// <returns></returns>
        [HttpGet("name/{name}/{like=true}")]
        public FnResult<GeneralTag[]> GetByName(string name, bool like)
        {
            name = HttpUtility.UrlDecode(name);
            var req = new FindGeneralTagsByNameQuery { Name = name, Like = like };
            var res = _mediator.Send(req).Result;
            return FnResult.OK(res);
        }

        /// <summary>
        /// 添加标签
        /// </summary>
        /// <returns></returns>
        [HttpPost(nameof(AddTags))]
        public FnResult<List<GeneralTag>> AddTags([FromBody]string[] names)
        {
            var cmd = new AddGeneralTagsCommand { Names = names };
            var res = _mediator.Send(cmd).Result;
            return FnResult.OK(res);
        }
    }
}