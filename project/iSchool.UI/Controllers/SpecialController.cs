using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.Service;
using iSchool.Organization.Appliaction.Service.EvaluationCrawler;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace iSchool.UI.Controllers
{
    /// <summary>
    /// 专题管理
    /// </summary>
    public class SpecialController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration config;
        
        public SpecialController(IMediator mediator, IWebHostEnvironment hostingEnvironment, IConfiguration config)
        {
            _mediator = mediator;
            _hostingEnvironment = hostingEnvironment;
            this.config = config;
           
        }

        #region 专题管理

        /// <summary>
        /// 专题管理-视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(SearchSpecialsQuery query)
        {
            ViewBag.SpecialList = _mediator.Send(new SelectItemsQuery() { Type = 2 }).Result;//专题下拉框 
            ViewBag.UploadUrl= config[Consts.OrgBaseUrl_evltcrawler_UploadUrl];//上传图片
            query.SpecialBaseUrl = config[Consts.SpecialBaseUrl];
            if (query.SearchType == 1)//视图
            {
                query.PageIndex = 1;query.PageSize = 10;
                var data = _mediator.Send(query).Result;
                ViewBag.SpecialStatusList = EnumUtil.GetSelectItems<SpecialStatusEnum>();//专题状态枚举  
                return View(data);
            }
            else//分页json
            {
                var data = _mediator.Send(query).Result;
                return Json(new { data = data, isOk = true });
            }
           
        }

        /// <summary>
        /// 下拉框专题列表
        /// </summary>
        /// <param name="specialType">专题类型</param>
        /// <returns></returns>
        [HttpGet]        
        public IActionResult Specials(int specialType)
        {
            var result = _mediator.Send(new SelectItemsQuery() { Type = specialType == SpecialTypeEnum.SmallSpecial.ToInt() ? 2 : 6 }).Result;//专题下拉框 
            return Json(result);

        }



        /// <summary>
        /// 专题上/下架
        /// </summary>
        /// <param name="id">专题Id</param>
        /// <param name="status">1:上架;2:下架;</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult OffOrOnTheShelf(Guid id, int status)
        {
            Guid userId = HttpContext.GetUserId();
            var result = _mediator.Send(new UpdateSpecialStatusCommand() {  Id=id, Status=status, UserId=userId  }).Result;
            return Json(result);
        }

        /// <summary>
        /// 删除专题
        /// </summary>
        /// <param name="delSpecId">待删除专题Id</param>
        /// <param name="newSpecId">待删除专题的评测归为其他专题的Id</param>
        /// <param name="specialType">专题类型(1:小专题；2：大专题)</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DelSpecial(Guid delSpecId,Guid newSpecId,int specialType)
        {
            Guid userId = HttpContext.GetUserId();
            var result = _mediator.Send(new DelSpecialChangeEvltSpecCommand() {  DelId=delSpecId, NewId=newSpecId,UserId=userId, SpecialType=specialType}).Result;
            ViewBag.SpecialList = _mediator.Send(new SelectItemsQuery() { Type = 2 }).Result;//专题下拉框 
            return Json(result);
        }



        #endregion

        #region 新增-编辑专题
        /// <summary>
        /// 保存（新增/编辑）专题信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveSpec(AddOrUpdateSpecialCommand request)
        {
            Guid userId = HttpContext.GetUserId();
            if (!string.IsNullOrEmpty(request.StrSmallSpecialIds))
            {
                var ids = request.StrSmallSpecialIds?.Split(',');
                List<Guid> sIds = new List<Guid>();
                foreach (var item in ids)
                {
                    sIds.Add(new Guid(item));
                }
                request.SmallSpecialIds = sIds;
            }            
            request.UserId = userId;
            var result = _mediator.Send(request).Result;
            ViewBag.NewId = Guid.NewGuid();
            if (request.IsAdd)
                ViewBag.SpecialList = _mediator.Send(new SelectItemsQuery() { Type = 2 }).Result;//专题下拉框 
            
            return Json(result);
        }
              

        #endregion

        #region 小专题&评测
        /// <summary>
        /// 获取所有上架的评测
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetEvlts(QueryEvlts query)
        {

            ViewBag.specialId = query.SpecialId;
            Dictionary<Guid, bool> dic = new Dictionary<Guid, bool>();
            query.PageIndex = 1; query.PageSize = 10;
            var data = _mediator.Send(query).Result;
            foreach (var item in data.CurrentPageItems)
            {
                if (!dic.ContainsKey(item.Id))//不包含
                {
                    dic.Add(item.Id, item.IsCheck);
                }
            }
            ViewBag.oldChecked = JsonSerializationHelper.Serialize(dic);
            return View(data);
            #region 旧
            //if (query.SearchType == 1)//返回视图
            //{
            //    query.PageIndex = 1;query.PageSize = 10;
            //    var data = _mediator.Send(query).Result;
            //    foreach (var item in data.CurrentPageItems)
            //    {
            //        if (!dic.ContainsKey(item.Id))//不包含
            //        {
            //            dic.Add(item.Id, item.IsCheck);
            //        }
            //    }
            //    ViewBag.oldChecked = JsonSerializationHelper.Serialize(dic);
            //    return View(data);
            //}
            //else//返回Json
            //{ //修改当前页的选中状态
            //    var data = _mediator.Send(query).Result;
            //    dic = JsonSerializationHelper.JSONToObject<Dictionary<Guid, bool>>(query.oldCheckedList);
            //    var d = data.CurrentPageItems.ToList();
            //    for (int i = 0; i < d.Count; i++)
            //    {
            //        if (!dic.ContainsKey(d[i].Id))//不包含
            //        {
            //            dic.Add(d[i].Id, d[i].IsCheck);
            //        }
            //        else
            //        {
            //            d[i].IsCheck = dic[d[i].Id];
            //        }
            //    }
            //    data.CurrentPageItems = d;
            //    ViewBag.oldChecked = JsonSerializationHelper.Serialize(dic);
            //    return Json(new { data=data,isOk=true, oldChecked = ViewBag.oldChecked });
            //}             
            #endregion
        }


        /// <summary>
        /// 获取所有上架的评测
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetEvltsPost(QueryEvlts query)
        {

            ViewBag.specialId = query.SpecialId;
            Dictionary<Guid, bool> dic = new Dictionary<Guid, bool>();
            var data = _mediator.Send(query).Result;
            dic = JsonSerializationHelper.JSONToObject<Dictionary<Guid, bool>>(query.oldCheckedList);
            var d = data.CurrentPageItems.ToList();
            for (int i = 0; i < d.Count; i++)
            {
                if (!dic.ContainsKey(d[i].Id))//不包含
                {
                    dic.Add(d[i].Id, d[i].IsCheck);
                }
                else
                {
                    d[i].IsCheck = dic[d[i].Id];
                }
            }
            data.CurrentPageItems = d;
            ViewBag.oldChecked = JsonSerializationHelper.Serialize(dic);
            return Json(new { data = data, isOk = true, oldChecked = ViewBag.oldChecked });
        }


        /// <summary>
        /// 保存-专题关联评测
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveSpeEvlts(SaveSpeEvltsCommand request)
        {
            var data = _mediator.Send(request).Result;
            return Json(data);
        }

        #endregion

        #region 获取大专题下的小专题-下拉项集合
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bigId"></param>
        /// <param name="isAdd"></param>
        /// <returns></returns>
        public IActionResult GetSpecialListItemsByBigId(Guid? bigId)
        {
            if (bigId == null)//新增界面
            {
                var allSmallSpecials = _mediator.Send(new SelectItemsQuery() { Type = 7 }).Result;//所有未绑定大专题的小专题 ;
                return Json(allSmallSpecials);
            }
            else//编辑
            {
                var data = _mediator.Send(new SelectItemsQuery() { Type = 4, BigSpecialId = (Guid)bigId }).Result;//某个大专题下的小专题 
                var allSmallSpecials = _mediator.Send(new SelectItemsQuery() { Type = 7, BigSpecialId= (Guid)bigId }).Result;//所有未绑定大专题的小专题+当前大专题下的小专题 ;
                for (int i = 0; i < allSmallSpecials.Count; i++)
                {
                    if (data.FirstOrDefault(_ => _.Value == allSmallSpecials[i].Value) != null)
                        allSmallSpecials[i].Selected = true;
                }
                return Json(allSmallSpecials);
            }
            
        }
        #endregion
    }
}
