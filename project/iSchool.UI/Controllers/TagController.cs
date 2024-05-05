using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool;
using iSchool.Application.Service;
using iSchool.Application.ViewModels;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iSchool.UI.Controllers
{
    public class TagController : Controller
    {

        private readonly IMediator _mediator;
        public TagController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 添加二级分类
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddTag2Cgy([FromBody] AddTag2CgyCommand cmd)
        {
            cmd.UserId = this.HttpContext.GetUserId();
            var fr = await _mediator.Send(cmd);
            return Json(fr);
        }

        /// <summary>
        /// 删除二级分类
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DelTag2Cgy([FromBody] DelTag2CgyCommand cmd)
        {
            cmd.UserId = this.HttpContext.GetUserId();
            await _mediator.Send(cmd);
            return Json(FnResult.OK());
        }

        /// <summary>
        /// 通用标签自动补全
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Autocomplete(string term, int top = 10)
        {
            var result = await _mediator.Send(new SearchGeneralTagsQuery(term, top));
            return Json(result);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Add(byte type, string name)
        {
            var r = _mediator.Send(new AddTagCommand(name, type)).Result;
            return Json(FnResult.OK(new { r.IsNew }));
        }

        /// <summary>
        /// 删除标签
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody]DeleteTagCommand ids)
        {
            await _mediator.Send(ids);
            return Json(new { IsOk = true });
        }

        /// <summary>
        /// 更新分类和标签绑定
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpTagSubv([FromBody] UpTagSubvCommand cmd)
        {
            cmd.UserId = HttpContext.GetUserId();
            await _mediator.Send(cmd);
            return Json(FnResult.OK());
        }

        /// <summary>
        /// 标签面板
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="selectIds">选中</param>
        /// <param name="input"></param>
        /// <param name="field_year">多年份字段页面改版专用，其他情况忽略该参数</param>
        /// <param name="isSync">同步字段才需要传true</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult TagPanel(int tagId, string input, string field_year = null, bool isSync = false)
        {
            var selects = Request.Query["selectIds"].ToString();
            KeyValueDto<string>[] selectIds = null;
            //同步字段点选标签需要
            if (isSync)
            {
                selectIds = JsonSerializationHelper.JSONToObjectOfExtFieldsSync<KeyValueDto<string>[]>(selects);
            }
            else
            {
                selectIds = JsonSerializationHelper.JSONToObject<KeyValueDto<string>[]>(selects);
            }


            string ids = "[]";
            if (selectIds != null)
            {
                ids = JsonSerializationHelper.Serialize(selectIds.Select(p => p.Value));
            }
            if (selectIds == null)
                selectIds = new KeyValueDto<string>[0];

            //多年份字段页面改版专用
            ViewBag.Field_Year = field_year;
            ViewBag.SelectIds = selectIds;
            ViewBag.SelectIdsJson = ids;
            ViewBag.TagId = tagId;
            ViewBag.Input = input;
            var tagList = _mediator.Send(new GetTagListQuery(false)).Result.Where(p => p.Type == tagId);
            ViewBag.TagsJson = JsonSerializationHelper.Serialize(tagList);
            return PartialView();
        }



        /// <summary>
        /// ajax添加tag
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddTagsAjax(byte type, string name)
        {
            var amd = new AddTagCommand(name, type);
            amd.UserId = HttpContext.GetUserId();

            var (tag, isNew) = _mediator.Send(amd).Result;

            var tagList = _mediator.Send(new GetTagListQuery(false)).Result.Where(p => p.Type == type);
            var json = JsonSerializationHelper.Serialize(
                new HttpResponse<IEnumerable<Application.ViewModels.TagDto>>
                {
                    State = 200,
                    Result = tagList
                });
            return Json(json);
        }

        /// <summary>
        /// 根据id获取标签的名称
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public IActionResult GetTagNames([FromBody] GetTagNamesQuery req)
        {
            var res = _mediator.Send(req).Result;
            return Json(new
            {
                isOk = true,
                data = res,
            });
        }
    }
}