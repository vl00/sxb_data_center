using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using MediatR;
using iSchool.Organization.Appliaction.RequestModels;

namespace iSchool.UI.Controllers
{
    /// <summary>
    /// 辅助功能
    /// </summary>
    public class HelperController : Controller
    {
        IMediator _mediator;
        public HelperController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region 敏感词

        /// <summary>
        /// 页面
        /// </summary>        
        /// <returns></returns>
        [HttpGet]
        public IActionResult SensitiveWords()
        {
            return View();
        }

        /// <summary>
        /// 查询api
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SearchSensitiveWords(string text)
        {
            
            var result = _mediator.Send(new SensitiveKeywordCmd() { Txt = text }).Result;
            if (!string.IsNullOrEmpty(result.FilteredTxt))
                return Json(result);
            else
            {
                var result2 = _mediator.Send(new SensitiveKeywordCmd() { Txts = new string[1] { text } }).Result;
                return Json(result2);
            }
        }
        #endregion
    }
}
