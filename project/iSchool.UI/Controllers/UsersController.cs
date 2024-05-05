using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace iSchool.UI.Controllers
{
    /// <summary>
    /// 前端用户
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/<UsersController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }


        /// <summary>
        /// 根据用户名或手机号获取用户信息
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet("/[controller]/NameOrPhone2")]
        [ProducesResponseType(typeof(UserInfoByNameOrMobileResponse), 200)]
        public async Task<ResponseResult> NameOrPhone2(string keyword)
        {
            var data = await _mediator.Send(new UserInfoByNameOrMobileQuery()
            {
                Mobile = keyword,
                Name = keyword
            });
            return ResponseResult.Success(data);
        }

        /// <summary>
        /// 根据用户名或手机号获取用户信息
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet("/[controller]/NameOrPhone/{keyword}")]
        [ProducesResponseType(typeof(UserInfoByNameOrMobileResponse), 200)]
        public async Task<ResponseResult> NameOrPhone(string keyword)
        {
            var data = await _mediator.Send(new UserInfoByNameOrMobileQuery()
            {
                Mobile = keyword,
                Name = keyword
            });
            return ResponseResult.Success(data);
        }
    }
}
