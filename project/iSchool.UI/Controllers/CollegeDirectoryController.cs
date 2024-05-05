using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using iSchool.Application.Service.CollegeDirectory;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using iSchool.Infrastructure;

namespace iSchool.UI.Controllers
{
    public class CollegeDirectoryController : Controller
    {
        private readonly IMediator _mediator;

        public CollegeDirectoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>        /// 下载模板        /// </summary>        /// <returns></returns>        [HttpGet]        public IActionResult GetTemplate()
        {
            var fs = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "App_Data/大学名录模板.xlsx"), FileMode.Open, FileAccess.Read);
            return File(fs, "application/ms-excel", "大学名录.xlsx");
        }

        /// <summary>
        /// excel导入大学名录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddSchoolFromExcel()
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

                    await _mediator.Send(new ExcelImportCommand { Excel = excel, UserId = uid });

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
    }
}