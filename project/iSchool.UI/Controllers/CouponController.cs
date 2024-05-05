using iSchool.Infras;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction;
using iSchool.Organization.Appliaction.Queries;
using iSchool.Organization.Appliaction.RequestModels.Coupon;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels.Coupon;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.UI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class CouponController : Controller
    {
        private readonly ICouponQueries _couponQueries;
        private readonly IOrderQueries _orderQueries;
        private readonly IMediator _mediator;
        private readonly IConfiguration _config;
        public CouponController(IMediator mediator, ICouponQueries couponQueries, IConfiguration config, IOrderQueries orderQueries)
        {
            _mediator = mediator;
            _couponQueries = couponQueries;
            _config = config;
            _orderQueries = orderQueries;
        }

        [HttpGet]
        public IActionResult Coupons()
        {
            return View();
        }


        [HttpGet]
        [HttpPost]
        public async Task<ResponseResult> GetCouponInfos([FromBody][FromQuery] CouponInfosFilterQuery filterQuery)
        {
            var couponInfos = await _mediator.Send(filterQuery);

            return ResponseResult.Success(couponInfos);
        }

        [HttpPost]
        public async Task<ResponseResult> AddCouponInfo([FromBody] AddCouponInfoCommand add)
        {
            add.Creator = HttpContext.GetUserId();
            if (!ModelState.IsValid)
            {
                return ResponseResult.Failed(ModelState.SelectMany(s => s.Value.Errors.Select(e => e.ErrorMessage)));
            }
            if (await _mediator.Send(add))
            {

                return ResponseResult.Success("保存成功。");
            }
            else
            {
                return ResponseResult.Success("保存失败。");
            }

        }

        [HttpPost]
        public async Task<ResponseResult> UpdateCouponInfo([FromBody] UpdateCouponInfoCommand update)
        {
            update.Updator = HttpContext.GetUserId();
            if (!ModelState.IsValid)
            {
                return ResponseResult.Failed(ModelState.SelectMany(s => s.Value.Errors.Select(e => e.ErrorMessage)));
            }
            if (await _mediator.Send(update))
            {

                return ResponseResult.Success("保存成功。");
            }
            else
            {
                return ResponseResult.Success("保存失败。");
            }

        }

        [HttpGet]
        public async Task<ResponseResult> CouponInfoOffline([FromQuery] CouponInfoOfflineCommand command)
        {
            if (await _mediator.Send(command))
            {

                return ResponseResult.Success("保存成功。");
            }
            else
            {
                return ResponseResult.Success("保存失败。");
            }
        }

        [HttpGet]
        public async Task<ResponseResult> CouponInfoOnline([FromQuery] CouponInfoOnlineCommand command)
        {
            if (await _mediator.Send(command))
            {

                return ResponseResult.Success("保存成功。");
            }
            else
            {
                return ResponseResult.Success("保存失败。");
            }
        }

        [HttpGet]
        public async Task<ResponseResult> CouponLoseEfficacy([FromQuery] CouponLoseEfficacyCommand command)
        {
            if (await _mediator.Send(command))
            {

                return ResponseResult.Success("保存成功。");
            }
            else
            {
                return ResponseResult.Success("保存失败。");
            }
        }

        [HttpGet]
        [HttpPost]
        public async Task<ResponseResult> SearchSKUs([FromBody][FromQuery] SKUEnableRangeQuery query)
        {
            var skus = await _mediator.Send(query);
            var courseGroups = skus.GroupBy(sku => sku.CourseId).Select(g =>
            {
                return new
                {
                    CourseName = g.First().CourseName,
                    SKUS = g.ToList()
                };

            });
            return ResponseResult.Success(courseGroups);
        }

        [HttpGet]
        [HttpPost]
        public async Task<ResponseResult> SearchCourseBrand([FromBody][FromQuery] CourseBrandEnableRangeQuery query)
        {
            var skus = await _mediator.Send(query);
            return ResponseResult.Success(skus);
        }

        [HttpGet]
        public async Task<ResponseResult> GetGoodsTypes([FromQuery] GoodsTypeQuery query)
        {
            var skus = await _mediator.Send(query);
            return ResponseResult.Success(skus);
        }



        /// <summary>
        /// 优惠券领取列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(CouponReceiveSummariesResponse), 200)]
        public async Task<ResponseResult> GetCouponReceivePageAsync([FromBody] CouponReceiveQueryModel queryModel)
        {
            var pagination = await _couponQueries.GetCouponReceivePageAsync(queryModel);
            return ResponseResult.Success(pagination);
        }

        /// <summary>
        /// 优惠券领取详情
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(CouponReceiveDetailResponse), 200)]
        public async Task<ResponseResult> GetCouponReceiveDetailAsync(Guid id)
        {
            var result = await _couponQueries.GetCouponReceiveDetailAsync(id);
            return ResponseResult.Success(result);
        }



        /// <summary>
        /// 到处优惠券领取列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult> ExportExcelCouponReceiveAsync([FromBody] CouponReceiveQueryModel queryModel)
        {
            queryModel.PageIndex = 1;
            queryModel.PageSize = int.MaxValue;
            var pagination = await _couponQueries.GetCouponReceivePageAsync(queryModel);
            var data = ReflectHelper.MapperProperty<CouponReceiveSummariesResponse, CouponReceiveResponseExportExcel>(pagination.Data.Select(s => s)).ToList();
            AssembleOrderInfo(ref data);

            var helper = NPOIHelper.NPOIHelperBuild.GetHelper();
            helper.Add("sheet1", data);


            //以后上传到cos
            //string fileName = $"{Guid.NewGuid():n}.xlsx";
            string fileName = $"{queryModel.GetCacheKey()}.xlsx";
            var lPath = Path.Combine(_config["AppSettings:XlsxDir"], fileName);//"./wwwroot/images/temp/xlsx"
            var filePath = Path.Combine(AppContext.BaseDirectory, lPath);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            using (var fileStream = System.IO.File.Create(filePath))
            {
                helper.WorkBook.Write(fileStream);
            }
            string removeWwwRootPath = lPath.Replace("./wwwroot", "");
            return ResponseResult.Success(new
            {
                FileName = $"{Request.Scheme}://{Request.Host}{removeWwwRootPath}"
            });
        }

        private void AssembleOrderInfo(ref List<CouponReceiveResponseExportExcel> data)
        {
            //商品详情字符串
            var detailsString = new StringBuilder();
            //实付金额字符串
            var paymentsString = new StringBuilder();
            //折扣字符串
            var discountsString = new StringBuilder();
            //原价字符串
            var orgTotalAmountsString = new StringBuilder();
            foreach (var item in data)
            {
                //兼容未支付订单, 支付失败订单OrderId不为空 
                if (item.Status != Organization.Domain.AggregateModel.CouponReceiveAggregate.CouponReceiveStateExt.Used)
                {
                    item.OrderId = null;
                    continue;
                }
                if (item.OrderId == null)
                {
                    continue;
                }
                var order = _orderQueries.GetAdvanceOrderDetailAsync(item.OrderId.Value).GetAwaiter().GetResult();
                if (order == null)
                {
                    continue;
                }
                item.AdvanceOrderNo = order.AdvanceOrderNo;
                item.OrderPaymentTime = order.PaymentTime;


                detailsString.Clear();
                paymentsString.Clear();
                discountsString.Clear();
                orgTotalAmountsString.Clear();
                foreach (var detail in order.OrderDetails)
                {
                    //商品+属性
                    detailsString.Append(detail.Name);
                    int i = 0;
                    foreach (var propName in detail.PropItemNames)
                    {
                        detailsString.Append("+").Append("属性").Append(++i).Append("：").Append(propName);
                    }
                    detailsString.Append("；");

                    orgTotalAmountsString.Append(detail.OriTotalAmount.ToString("#0.00")).Append("；");
                    discountsString.Append(detail.Discount.ToString("#0.00")).Append("；");
                    paymentsString.Append(detail.Payment.ToString("#0.00")).Append("；");
                }

                //价格
                item.OrderDetailsString = detailsString.ToString();
                item.OrderOrgTotalAmountsString = orgTotalAmountsString.ToString();
                item.OrderDiscountsString = discountsString.ToString();
                item.OrderPaymentsString = paymentsString.ToString();
            }
        }


        /// <summary>
        /// 发放优惠券给指定用户
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult> GrantCouponReceive([FromBody] CreateCouponReceiveCommand command)
        {
            try
            {
                command.Remark = "后台发放";
                command.OriginType =  CouponReceiveOriginType.FromSystem;
                if (command.UserId == default)
                {
                    return ResponseResult.Failed("请选择发放给谁");
                }

                var couponReceive = await _mediator.Send(command);
                return ResponseResult.Success("OK");
            }
            catch (Exception ex)
            {
                return ResponseResult.Failed(ex.Message);
            }

        }


        /// <summary>
        /// 供其它业务调用指定发券功能
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseResult> GrantCouponReceiveFromBusiness([FromBody] CreateCouponReceiveCommand command)
        {
            try
            {
                command.Remark = $"[来自其它业务发放]-{command.Remark}";
                command.OriginType = CouponReceiveOriginType.FromSystem;
                if (command.UserId == default)
                {
                    return ResponseResult.Failed("请选择发放给谁");
                }

                var couponReceive = await _mediator.Send(command);
                return ResponseResult.Success("OK");
            }
            catch (Exception ex)
            {
                return ResponseResult.Failed(ex.Message);
            }

        }

        /// <summary>
        /// 批量发送优惠券
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult> BatchGrantCouponReceive([FromBody] BatchCreateCouponReceiveCommand command)
        {
            command.SenderID = HttpContext.GetUserId();
            return await _mediator.Send(command);
        }



        /// <summary>
        /// 扫描即将过期的券
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseResult> ScanWillExpireCouponReceives()
        {

            var couponreceives = await _couponQueries.GetWillExpireCouponReceives();
            int failingCounter = 0;
            foreach (var couponreceive in couponreceives)
            {
                try
                {
                    await _mediator.Send(new SendWillExpireMsgNotifyCommand() { CouponReceiveId = couponreceive.Id });
                }
                catch
                {
                    failingCounter++;
                }
            }
            return ResponseResult.Success($"couponreceives count={couponreceives.Count()};failing count={failingCounter}");
        }



        [HttpGet]
        public async Task<ResponseResult> GetCouponInfoItem(Guid id)
        {
            var item = await _couponQueries.GetCouponInfoItem(id);
            return ResponseResult.Success(item);


        }

    }
}
