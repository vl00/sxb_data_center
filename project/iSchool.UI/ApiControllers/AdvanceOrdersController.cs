using iSchool.Organization.Appliaction.Queries;
using iSchool.Organization.Appliaction.Queries.Models;
using iSchool.Organization.Appliaction.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace iSchool.UI.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvanceOrdersController : ControllerBase
    {
        private readonly IOrderQueries _orderQueries;

        public AdvanceOrdersController(IOrderQueries orderQueries)
        {
            _orderQueries = orderQueries ?? throw new ArgumentNullException(nameof(orderQueries));
        }


        /// <summary>
        /// 获取已支付过预定单详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("id")]
        [ProducesResponseType(typeof(AdvanceOrderDetailResponse), 200)]
        public async Task<ResponseResult> GetAdvanceOrderDetailAsync(Guid id)
        {
            var data = await _orderQueries.GetAdvanceOrderDetailAsync(id);
            return ResponseResult.Success(data);
        }
    }
}
