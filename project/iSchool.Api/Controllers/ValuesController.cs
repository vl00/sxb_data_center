using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.Course;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iSchool.Api.Controllers
{
    /// <summary>
    /// 测试api而添加[AllowAnonymous]
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ValuesController : ControllerBase
    {
        IMediator _mediator;
        public ValuesController(IMediator mediator)
        {
            _mediator = mediator;
        }


        #region 根据体验课Id,查询大课列表
        /// <summary>
        /// 【课程分销后台专用】大课信息
        /// </summary>
        /// <param name="courseId">体验课Id</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<BigCourseResponse>), 200)]
        public async Task<ResponseResult> Get(Guid courseId)
        {
            var response = await _mediator.Send(new BigCourseByIdQuery() { CourseId = courseId });
            return ResponseResult.Success(response);
        }
        #endregion

        ///// <summary>
        ///// GET api/values
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public ActionResult<IEnumerable<string>> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/values/5
        //[HttpGet("{id}")]
        //public ActionResult<string> Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/values
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
