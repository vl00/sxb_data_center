using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.OrgService_bg;
using iSchool.Organization.Appliaction.OrgService_bg.Imports;
using iSchool.Organization.Appliaction.OrgService_bg.Orders;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace iSchool.UI.Controllers
{
    /// <summary>
    /// 后台--Order模块
    /// </summary>
    public class OrderController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _config;

        public OrderController(IMediator mediator, IConfiguration config)
        {
            _mediator = mediator;            
            _config = config;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Export()
        {
            return View();
        }

        #region 导出

        /// <summary>
        /// 导出活动审核到excel
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ExportXlsxP(ExportOrdersCommand cmd)
        {
            var id = await _mediator.Send(cmd);
            return Json(string.IsNullOrEmpty(id) ? ResponseResult.Failed("系统繁忙") : ResponseResult.Success(id, null));
        }

        /// <summary>
        /// 导出课程订单列表到excel
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExportOrders(ExportOrdersByMoreConditionCommand cmd)
        {
            var id = await _mediator.Send(cmd);
            return Json(string.IsNullOrEmpty(id) ? ResponseResult.Failed("系统繁忙") : ResponseResult.Success(id, null));
        }

        #endregion

        #region 导入

        /// <summary>
        /// 批量导入更新订单信息
        /// excel导入(约课状态、机构反馈等)
        /// </summary>
        /// <cid>课程Id</cid>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> BatchUpdateOrderInfosFromExcel()
        {
            var excelfile = HttpContext.Request.Form.Files[0];

            if (excelfile.Length > 1024.0 * 1024 * 100) // > 100M
            {
                return Json(new
                {
                    isOk = false,
                    msg = "excel超过100M了, 请分割",
                });
            }
            try
            {
                var uid = HttpContext.GetUserId();

                using (var excel = new MemoryStream((int)excelfile.Length))
                {
                    await excelfile.CopyToAsync(excel);

                    await _mediator.Send(new ExcelImportOrderInfoCommand { Excel = excel, UserId = uid });

                    return Json(new { isOk = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    isOk = false,
                    msg = ex.Message,
                });
            }
        }

        /// <summary>
        /// 批量导入更新物流信息 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> BatchUpdateWLInfosFromExcel()
        {
            var excelfile = HttpContext.Request.Form.Files[0];

            if (excelfile.Length > 1024.0 * 1024 * 100) // > 100M
            {
                return Json(ResponseResult.Failed("excel超过100M了,请分割"));
            }
            try
            {
                var uid = HttpContext.GetUserId();
                var fid = Guid.NewGuid().ToString("n");
                var ext = Path.GetExtension(excelfile.FileName);
                ext = !ext.IsNullOrEmpty() ? ext : ".xlsx";
                var path = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/images/temp/{fid}{ext}");

                using var fs = System.IO.File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                fs.Seek(0, SeekOrigin.Begin);
                await excelfile.CopyToAsync(fs);
                await fs.FlushAsync();
                fs.Seek(0, SeekOrigin.Begin);

                var result = await _mediator.Send(new ExcelImportWLInfoCommand { Excel = fs, UserId = uid });

                return Json(ResponseResult.Success(result));
            }
            catch (Exception ex)
            {
                return Json(ResponseResult.Failed(ex.Message));
            }
        }

        /// <summary>
        /// 批量导入更新订单出库状态信息 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> BatchUpdateOutdepotStatusFromExcel()
        {
            var excelfile = HttpContext.Request.Form.Files[0];

            if (excelfile.Length > 1024.0 * 1024 * 100) // > 100M
            {
                return Json(ResponseResult.Failed("excel超过100M了,请分割"));
            }
            try
            {
                var uid = HttpContext.GetUserId();
                var fid = Guid.NewGuid().ToString("n");
                var ext = Path.GetExtension(excelfile.FileName);
                ext = !ext.IsNullOrEmpty() ? ext : ".xlsx";
                var path = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/images/temp/{fid}{ext}");

                using var fs = System.IO.File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                fs.Seek(0, SeekOrigin.Begin);
                await excelfile.CopyToAsync(fs);
                await fs.FlushAsync();
                fs.Seek(0, SeekOrigin.Begin);

                var result = await _mediator.Send(new ExcelImportOrderOutdepotStatusCommand { Excel = fs, UserId = uid });

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(ResponseResult.Failed(ex.Message));
            }
        }

        #endregion

        #region 下载模板

        /// <summary>
        /// 下载模板
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetTemplate()
        {        
            string templateName = "订单物流信息";
            var fs = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), $"App_Data/{templateName}模板.xlsx"), FileMode.Open, FileAccess.Read);
            return File(fs, "application/ms-excel", $"{templateName}.xlsx");
        }

        #endregion


        /// <summary>
        /// 后台查物流信息
        /// </summary>
        /// <param name="dhCode"></param>
        /// <param name="nu">快递单号</param>
        /// <param name="com">快递公司编码</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> KdDetails(string dhCode, string nu, string com)
        {
            try { var userId = HttpContext.GetUserId(); }
            catch 
            {
                return Json(Res2Result.Fail("没权限", 403));
            }
            var rr = await _mediator.Send(new GetKuaidiDetailQuery { ExpressCode = nu, ExpressType = com });
            return Json(Res2Result.Success(rr));
        }

        /// <summary>
        /// mp1.6+ 批量导入物流信息 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> BatchImportSendGoods()
        {
            var excelfile = HttpContext.Request.Form.Files[0];
            if (excelfile.Length > 1024.0 * 1024 * 100) // > 100M
            {
                return Json(ResponseResult.Failed("excel超过100M了,请分割"));
            }
            var uid = HttpContext.GetUserId();
            var fid = Guid.NewGuid().ToString("n");
            var ext = Path.GetExtension(excelfile.FileName);
            ext = !ext.IsNullOrEmpty() ? ext : ".xlsx";
            var path = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/images/temp/{fid}{ext}");
            try
            {
                using var fs = System.IO.File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                fs.Seek(0, SeekOrigin.Begin);
                await excelfile.CopyToAsync(fs);
                await fs.FlushAsync();
                fs.Seek(0, SeekOrigin.Begin);

                var result = await _mediator.Send(new BatchImportSendGoodsCmd { ExcelStream = fs, UserId = uid });
                return Json(ResponseResult.Success(result));
            }
            catch (Exception ex)
            {
                return Json(ResponseResult.Failed(ex.Message));
            }
        }
    }
}

