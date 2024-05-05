//using iSchool.Organization.Appliaction.OrgService_bg.RequestModels.Theme;
//using iSchool.Organization.Appliaction.ResponseModels;
//using MediatR;
//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace iSchool.UI.Controllers
//{
//    public class ThemeController : Controller
//    {
//        private readonly IMediator _mediator;

//        public ThemeController(IMediator mediator)
//        {
//            _mediator = mediator;
//        }



//        /// <summary>
//        /// 获取主题列表
//        /// </summary>
//        /// <returns></returns>
//        [HttpGet]
//        public IActionResult List([FromQuery] ThemeListQuery query)
//        {
//            var res = _mediator.Send(query).Result;
//            return Json(ResponseResult.Success(res));
//        }

//        /// <summary>
//        /// 获取主题详情
//        /// </summary>
//        /// <param name="query"></param>
//        /// <returns></returns>
//        [HttpGet]
//        public IActionResult Detail([FromQuery] ThemeDetailQuery query)
//        {
//            return Json("");
//        }

//        /// <summary>
//        /// 添加或者修改主题
//        /// </summary>
//        /// <param name="commend"></param>
//        /// <returns></returns>
//        [HttpPost]
//        public IActionResult AddOrEdit([FromBody] AddOrEditThemeCmd commend)
//        {
//            return Json("");
//        }




//    }
//}
