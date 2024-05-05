using DotNetCore.CAP;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.IntegrationEvents;
using iSchool.Organization.Appliaction.IntegrationEvents.Events;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.RequestModels.Aftersales;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using iSchool.Organization.Appliaction.ViewModels.Aftersales;
using iSchool.Organization.Domain.Enum;

namespace iSchool.UI.Controllers
{
    public class AftersalesController : Controller
    {
        IMediator _mediator;

        public AftersalesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ResponseResult> GetAftersales([FromBody] AftersalesFilterQuery filter)
        {
            var aftersalesCollection = await _mediator.Send(filter);

            return ResponseResult.Success(aftersalesCollection);
        }


        /// <summary>
        /// 物流详情
        /// </summary>
        /// <dhCode>兑换码</dhCode>
        /// <nu>快递单号</nu>  
        /// <companyCode>快递公司编号</companyCode>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseResult> ExpressDetail(string nu, string companyCode)
        {
            var result = await _mediator.Send(new GetKuaidiDetailsByTxc17972ApiQuery() { Com = companyCode, Nu = nu });
            ViewBag.Succeed = false;
            if (result.Errcode == 0)
            {

                return ResponseResult.Success(new
                {
                    IsCompleted = result.IsCompleted,
                    LogisticsItems = result?.Items?.ToList()
                });
            }
            else
            {
                return ResponseResult.Failed(result.Errmsg);
            }

        }


        /// <summary>
        /// 审核失败
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<ResponseResult> AuditFail(AuditFailCommand command)
        {
            Guid userId = HttpContext.GetUserId();
            command.Auditor = userId;
            bool flag = await _mediator.Send(command);

            if (flag)
            {
                return ResponseResult.Success("操作成功。");
            }
            else
            {
                return ResponseResult.Failed("操作失败。");
            }

        }

        /// <summary>
        /// 审核成功
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<ResponseResult> AuditSuccess([FromBody]AuditSuccessCommand command)
        {
            try
            {
                Guid userId = HttpContext.GetUserId();
                command.Auditor = userId;
                bool flag = await _mediator.Send(command);
                if (flag)
                {
                    return ResponseResult.Success("操作成功。");
                }
                else
                {
                    return ResponseResult.Failed("操作失败。");
                }
            }
            catch (Exception ex)
            {
                return ResponseResult.Failed(ex.Message);
            }

        }

        /// <summary>
        /// 修改寄回地址
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult> ModifyAddress([FromBody] UpdateSendBackAddressCommand command)
        {
            if (!ModelState.IsValid) return ResponseResult.Failed("参数不完整");
            var orderRefund = await _mediator.Send(new OrderRefundQuery() { OrderRefundId = command.OrderRefundId });
            if (orderRefund == null) return ResponseResult.Failed("找不到售后记录。");
            if (orderRefund.Status != 12) return ResponseResult.Failed("当前售后状态不可修改寄回地址。");
            Guid userId = HttpContext.GetUserId();
            command.Auditor = userId;
            bool flag = await _mediator.Send(command);
            if (flag)
            {
                return ResponseResult.Success("保存成功");
            }
            else
            {
                return ResponseResult.Failed("保存失败");
            }


        }



        /// <summary>
        /// 退款并更新订单状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public ResponseResult BackGroundRefund([FromBody][FromQuery] BackGroundRefundCommand request)
        {
            request.Auditor = Guid.Empty;
            var result = _mediator.Send(request).Result;
            if (result)
            {
                return ResponseResult.Success("退款成功");
            }
            else
            {
                return ResponseResult.Failed("退款失败");
            }
        }



        public async Task<IActionResult> ExportAftersales([FromBody]AftersalesFilterQuery filter)
        {
            filter.Page = 1;
            filter.PageSize = int.MaxValue;
            AftersalesCollection aftersalesCollection = await _mediator.Send(filter);


            IEnumerable<(string r1c, Func<int, int, Aftersales, object> wr)> Wcell()
            {
                Aftersales data = new Aftersales();


                yield return ("退款编号", (row, col, data) => data.Number);
                yield return ("退货商品", (row, col, data) =>
                {
                    if (data.SKU != null)
                    {
                        return $"{data.SKU.GoodsName }+{data.SKU.PropName}";
                    }
                    return "";
                }
                );
                yield return ("退货数量", (row, col, data) => data.ReturnCount);
                yield return ("售后类型", (row, col, data) => data.Type.GetDesc());
                yield return ("实付金额", (row, col, data) => data.PayAmount);
                yield return ("申请退货金额", (row, col, data) => data.ApplyRefundAmount);
                yield return ("实退金额", (row, col, data) => data.RefundAmount);
                yield return ("申请时间", (row, col, data) => data.ApplyDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                yield return ("退货理由", (row, col, data) => data.Reason?.GetDesc());
                yield return ("退款人昵称", (row, col, data) => data.RefundUserNickName);
                yield return ("退货人手机号", (row, col, data) => data.RefundUserPhoneNumber);
                yield return ("关联订单号", (row, col, data) => data.OrderNumber);
                yield return ("订单状态", (row, col, data) => ((OrderStatusV2)data.OrderInfo.State).GetDesc());
                yield return ("售后状态", (row, col, data) => data.State.GetDesc());
                yield return ("退回物流单号", (row, col, data) => {
                    if (data.ExpressInfo != null)
                    {
                        return $"{data.ExpressInfo.Name}:{data.ExpressInfo.Code}";
                    }
                    return ""; });
                yield return ("第一次审核状态", (row, col, data) =>
                {
                    if (data.FirstAuditResult != null)
                    {
                        return data.FirstAuditResult.State.GetDesc();
                    }
                    return "";
                });
                yield return ("第一次审核人", (row, col, data) =>
                {
                    if (data.FirstAuditResult != null)
                    {
                        return data.FirstAuditResult.AuditorName;
                    }
                    return "";
                });
                yield return ("不通过原因", (row, col, data) =>
                {
                    if (data.FirstAuditResult != null)
                    {
                        return data.FirstAuditResult.Remark;
                    }
                    return "";
                });
                yield return ("第二次审核状态", (row, col, data) =>
                {
                    if (data.SecondAuditResult != null)
                    {
                        return data.SecondAuditResult.State.GetDesc();
                    }
                    return "";
                });
                yield return ("第二次审核人", (row, col, data) =>
                {
                    if (data.SecondAuditResult != null)
                    {
                        return data.SecondAuditResult.AuditorName;
                    }
                    return "";
                });
                yield return ("不通过原因", (row, col, data) =>
                {
                    if (data.SecondAuditResult != null)
                    {
                        return data.SecondAuditResult.Remark;
                    }
                    return "";
                });
                yield return ("售后完成时间", (row, col, data) =>
                {
                    return data.FinishTime?.ToString("yyyy-MM-dd HH:mm:ss");

                }
                );

            }
            var stream = new MemoryStream();

            using var package = new ExcelPackage(stream);
            {
                var sheet = package.Workbook.Worksheets.Add("Sheet1");
                int row = 1, col = 1;
                foreach (var (r1c, _) in Wcell())
                {
                    sheet.Cells[row, col++].Value = r1c;
                }
                foreach (var item in aftersalesCollection.Datas)
                {
                    row++; col = 1;
                    foreach (var (_, wr) in Wcell())
                    {
                        sheet.Cells[row, col].Value = wr(row, col, item)?.ToString();
                        col++;
                    }
                }
                package.Save();
            }
            stream.Position = 0;
            return File(stream, "application/octet-stream",
                    $"售后筛选列表{DateTime.Now:yyyyMMdd}.xlsx", false);

        }

        public async Task<ResponseResult> GetSupplierAddress(Guid orderRefundId)
        {
           var address =  await _mediator.Send(new SupplierAddresssQuery() { OrderRefundId = orderRefundId });
            return ResponseResult.Success(address);
        
        }

        //public async Task<ResponseResult> TestOrderRefundQuery(Guid id)
        //{
        //   var a = await  _mediator.Send(new OrderRefundQuery() { OrderRefundId = id });

        //    return ResponseResult.Failed(a);
        //}

        //public ResponseResult ShowTime([FromServices] IOrganizationIntegrationEventService organizationIntegrationEventService)
        //{
        //    organizationIntegrationEventService.PublishEvent(new OrderRefundAuditSuccessIntegrationEvent());
        //    return ResponseResult.Success("ok");
        //}

        //[NonAction]
        //[CapSubscribe("test.show.time")]
        //public void ShowTimeHandle(DateTime dateTime, [FromCap] CapHeader header)
        //{
        //    Console.WriteLine("from cap:{0}", dateTime);
        //}
    }
}
