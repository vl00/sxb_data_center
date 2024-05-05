using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.OrgService_bg.Services.Supplier;
using iSchool.Organization.Appliaction.OrgService_bg.Supplier;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.EvaluationCrawler;
using iSchool.Organization.Appliaction.Services;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.UI.Controllers
{
    /// <summary>
    /// 供应商管理
    /// </summary>
    public class SupplierController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration config;

        public SupplierController(IMediator mediator, IWebHostEnvironment hostingEnvironment, IConfiguration config)
        {
            _mediator = mediator;
            _hostingEnvironment = hostingEnvironment;
            this.config = config;
        }


        #region 供应商管理
        /// <summary>
        /// 供应商列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="query">查询条件</param>
        /// <returns></returns>
        [HttpGet]
        public ResponseResult Index(SearchSupplierListQuery query)
        {
            var data = _mediator.Send(query).Result;
            return ResponseResult.Success(data);
        }

        /// <summary>
        /// 供应商列表下拉选项
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ResponseResult ListOption()
        {
            //匹配下拉框  
            var orglist = _mediator.Send(new SelectItemsQuery() { Type = 1 }).Result;
            orglist.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = "所有", Value = "-1" });
            return ResponseResult.Success(orglist);
        }
        #endregion
        #region 新增-编辑机构

        /// <summary>
        /// 获取供应商详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ResponseResult GetSupplierInfo(Guid id)
        {
            var result = _mediator.Send(new SupplierInfoByIdQuery { Id = id}).Result;
            return ResponseResult.Success(result);
        }

        /// <summary>
        /// 保存（新增/编辑）供应商信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ResponseResult SaveSupplier([FromBody]AddOrEditSupplierCommand request)
        {

            Guid userId = HttpContext.GetUserId();
            request.UserId = userId;
            var result = _mediator.Send(request).Result;
            return result;
        }
        #endregion

        #region 导出机构
        public IActionResult ExportSupplier(ExportSupplierListQuery query)
        {
            var result = _mediator.Send(query).Result;

            IEnumerable<(string r1c, Func<int, int, dynamic, object> wr)> Wcell()
            {
                yield return ("序号", (row, col, data) => data.RowNum?.ToString() ?? "");
                yield return ("供应商名称", (row, col, data) => data.Name);
                yield return ("供应商对公账号", (row, col, data) => data.BankCardNo);
                yield return ("开户行", (row, col, data) => data.BankAddress);
                yield return ("供应商对公账户名称（公司名称）", (row, col, data) => data.CompanyName);
                yield return ("是否私人", (row, col, data) => data.IsPrivate);
                yield return ("供应商退货地址", (row, col, data) => data.ReturnAddress);
                yield return ("相关品牌", (row, col, data) => data.OrgName);
                yield return ("结算方式", (row, col, data) => data.BillingType);
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
                foreach (var item in result)
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
                    $"供应商统计表{DateTime.Now:yyyyMMdd}.xlsx", false);
        }

        #endregion
    }
}
