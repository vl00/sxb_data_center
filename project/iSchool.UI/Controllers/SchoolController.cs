using iSchool.Application.CommonHelper;
using iSchool.Application.ReponseModels;
using iSchool.Application.Service;
using iSchool.Application.Service.Audit;
using iSchool.Application.ViewModels;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Cache;
using iSchool.Infrastructure.Upload;
using iSchool.UI.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProductManagement.Framework.Cache.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace iSchool.UI.Controllers
{
    /// <summary>
    /// 数据录入模块
    /// </summary>
    public partial class SchoolController : Controller
    {
        private readonly IMediator _mediator;
        private readonly AppSettings _appSettings;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly CacheManager _cacheManager;
        private readonly string _syncKey = "ext_sync";
        readonly IEasyRedisClient _easyRedisClient;

        public SchoolController(IMediator mediator, IOptions<AppSettings> options,
            IWebHostEnvironment hostingEnvironment, CacheManager cacheManager, IEasyRedisClient easyRedisClient)
        {
            _easyRedisClient = easyRedisClient;
            _mediator = mediator;
            _appSettings = options.Value;
            _hostingEnvironment = hostingEnvironment;
            _cacheManager = cacheManager;
        }

        /// <summary>
        /// 学校列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(SearchSchoolListQuery query, int page = 1, string search = null)
        {
            page = page < 1 ? 1 : page;
            //年级
            ViewBag.GradeSelect = EnumUtil.GetSelectItems<SchoolGrade>();
            //城市数据
            ViewBag.ProvinceSelect = _mediator.Send(new KVQuery { ParentId = 0, Type = 1 })
                .Result
                .Select(p => new SelectListItem { Text = p.Name, Value = p.Id.ToString() });
            IEnumerable<SelectListItem> citySelect = null;
            if (query.Province != 0)
            {
                citySelect = _mediator.Send(new KVQuery { ParentId = query.Province, Type = 1 })
                .Result
                .Select(p => new SelectListItem { Text = p.Name, Value = p.Id.ToString() });
            }
            ViewBag.CitySelect = citySelect;
            IEnumerable<SelectListItem> areaSelect = null;
            if (query.City != 0)
            {
                areaSelect = _mediator.Send(new KVQuery { ParentId = query.City, Type = 1 })
                .Result
                .Select(p => new SelectListItem { Text = p.Name, Value = p.Id.ToString() });
            }
            ViewBag.AreaSelect = areaSelect;
            //学校类型
            var schoolType = EnumUtil.GetSelectItems<SchoolType>();
            ViewBag.SchoolTypeSelect = schoolType;
            //审核状态
            ViewBag.AuditStatus = EnumUtil.GetSelectItems<SchoolAuditStatus>();
            //查询条件
            string queryJson = query == null ? null : JsonSerializationHelper.Serialize(query);
            ViewBag.queryJson = queryJson;
            ViewBag.query = query;
            ViewBag.search = search;
            //查询文本
            ViewBag.SearchText = search;
            var userId = HttpContext.GetUserId();
            ViewBag.UserId = userId;
            SearchSchoolListDto dto = new SearchSchoolListDto { PageIndex = page, PageSize = 0 };

            //是否有全部
            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            ViewBag.IsAll = isAll;

            //编辑员
            var editors = new Authorization.Account().GetAdmins(_appSettings.GidEditor, Authorization.Account.IDType.CharacterID).Select(_ => new Domain.Total_User
            {
                Id = _.Id,
                Account = _.Name,
                Username = _.Displayname,
                RoleId = _appSettings.GidEditor,
            })
            .OrderBy(_ => _.Username)
            .ToArray();

            if (!isAll) ViewBag.Editors = editors.Where(_ => _.Id == HttpContext.GetUserId()).ToArray();
            else ViewBag.Editors = editors;
            //...

            query.Search = search;
            query.Index = page;
            query.IsAll = isAll;
            query.PageSize = 10;
            query.UserId = userId;
            dto = _mediator.Send(query).Result;

            var userIds = dto.list.Where(p => p.AuditUserId != null && p.AuditUserId != Guid.Empty).Select(p => p.AuditUserId.Value)
                .Union(dto.list.Where(p => p.Creator != null && p.Creator != Guid.Empty).Select(p => p.Creator));
            var userName = AdminInfoUtil.GetNames(userIds);
            ViewBag.UserNames = userName;

            var usersAsIPagedList = new StaticPagedList<SearchSchoolItem>(dto.list, page, dto.PageSize, dto.PageCount);

            #region GenerateUploadDataToken
            if (Request.Query.ContainsKey("generateToken"))
            {
                var uploadToken = _easyRedisClient.GetOrAddAsync("UploadDataToken", () =>
                {
                    return Guid.NewGuid().ToString();
                }, TimeSpan.FromDays(1)).Result;
                ViewBag.UploadDataToken = uploadToken;
            }
            #endregion

            return View(usersAsIPagedList);
        }

        /// <summary>
        /// 删除学校
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteSchool(Guid sid)
        {
            var userId = HttpContext.GetUserId();
            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            var result = _mediator.Send(new DelSchoolCommad { IsAll = isAll, SId = sid, UserId = userId }).Result;
            return Json(result);
        }


        /// <summary>
        /// 学校页面
        /// </summary>
        /// <param name="sid">学校id</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Main(Guid? sid = null)
        {
            var status = sid == null || sid == Guid.Empty ? 1 : 2;
            SchoolDto dto = new SchoolDto();
            //如果是修改
            if (status == 2)
            {
                var userId = HttpContext.GetUserId();
                var isAll = HttpContext.HasCurrQyx(".qx-schAll");
                dto = _mediator.Send(new GetSchoolDetailsQuery(sid.Value, isAll, userId)).Result;
                if (dto == null)
                {
                    return RedirectToAction("Index");
                }
            }
            ViewBag.Status = status;
            ViewBag.TempId = sid == null ? Guid.NewGuid() : sid.Value;
            ViewBag.UserName = (sid == null && dto.Creator != null && dto.Creator != Guid.Empty) ? ""
                : AdminInfoUtil.GetNames(new List<Guid> { dto.Creator }).Values.FirstOrDefault();
            return View(dto);
        }
        /// <summary>
        /// 认领学校
        /// </summary>
        /// <returns></returns>
        public IActionResult Claim(Guid sid)
        {
            var userId = HttpContext.GetUserId();
            var result = _mediator.Send(new ClaimSchoolCommand { Sid = sid, UserId = userId }).Result;
            return Json(result);
        }


        /// <summary>
        /// 添加学校
        /// </summary>
        /// <param name="_tnow"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddSchool(long? _tnow, AddSchoolCommand command)
        {
            _ = _tnow;
            if (string.IsNullOrEmpty(command.Name))
            {
                return Json(new HttpResponse<string> { State = 500, Message = "学校名称不能空！" });
            }

            var check = await _mediator.Send(new CheckSchoolNameQuery { Sid = command.Sid, Name = command.Name });
            if (check.State != 200)
            {
                return Json(check);
            }
            var userId = HttpContext.GetUserId();
            command.UserId = userId;
            try
            {
                var result = await _mediator.Send(command);
                if (result == null)
                {
                    return Json(new HttpResponse<string> { State = 500, Message = "操作失败！" });
                }
                else
                {
                    //新增成功后添加跳转到主页
                    return Json(new HttpResponse<string> { State = 200, Result = result.Value.ToString() });
                }
            }
            catch (CustomResponseException ex)
            {
                return Json(new HttpResponse<string> { State = ex.ErrorCode == 0 ? 500 : ex.ErrorCode, Message = ex.Message });
            }
            catch
            {
                return Json(new HttpResponse<string> { State = 500, Message = "操作失败！" });
            }
        }

        /// <summary>
        /// 删除学部
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DelExtension(DelExtensionCommand command)
        {
            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            var userId = HttpContext.GetUserId();
            command.UserId = userId;
            command.IsAll = isAll;

            var result = _mediator.Send(command).Result;

            return result ? Json(new HttpResponse<string>
            { State = 200 })
            : Json(new HttpResponse<string>
            {
                State = 400,
                Message = "删除学部失败"
            });
        }
        /// <summary>
        /// 提交审核
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Examination(Guid sid)
        {
            if (sid == Guid.Empty)
                return Json(new HttpResponse<string> { State = 500, Message = "不存在该学校" });
            var userId = HttpContext.GetUserId();
            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            var result = _mediator.Send(new ExaminationSchoolCommand
            {
                IsAll = isAll,
                Sid = sid,
                UserId = userId
            }).Result;

            return Json(result);

        }



        /// <summary>
        /// 第一步（学校概况）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Step1(Guid sid, Guid? extId)
        {
            if (sid == Guid.Empty)
            {
                return RedirectToAction("Main");
            }
            SchoolExtensionDto dto = new SchoolExtensionDto();
            //判断
            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            var userId = HttpContext.GetUserId();

            if (extId != null && extId != Guid.Empty)
            {
                //查询学校分部
                var result = _mediator.Send(new GetSchoolExtensionQuery(sid, extId.Value, SchoolExtensionType.Extension, isAll, userId)).Result;
                if (result != null)
                {
                    dto = (SchoolExtensionDto)result;
                }
                else
                    return RedirectToAction("Main", new { sid = sid });
            }
            ViewBag.GradeSelect = EnumUtil.GetSelectItems<SchoolGrade>();
            var schoolType = EnumUtil.GetSelectItems<SchoolType>();
            ViewBag.SchoolTypeSelect = schoolType;
            ViewBag.SchoolTypeJson = JsonSerializationHelper.Serialize(schoolType);
            if (!string.IsNullOrEmpty(dto.SourceAttachments))
            {
                dto.ListSourceAttchments = JsonSerializationHelper.JSONToObject<List<SoureAttach>>(dto.SourceAttachments);
                dto.ListSourceAttchments.ForEach(x => x.icon = EnumUtil.GetAttachIcon(x.uri.Split(".").Last().ToLower()));
            }
            if (!string.IsNullOrEmpty(dto.NickName))
            {
                dto.NickName = string.Join(",", JsonConvert.DeserializeObject<string[]>(dto.NickName));
            }
            return View(dto);
        }

        [HttpGet]
        public IActionResult Step1win(Guid sid, Guid? extId)
        {
            if (sid == Guid.Empty)
            {
                return RedirectToAction("Main");
            }
            SchoolExtensionDto dto = new SchoolExtensionDto();
            //判断
            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            var userId = HttpContext.GetUserId();

            if (extId != null && extId != Guid.Empty)
            {
                //查询学校分部
                var result = _mediator.Send(new GetSchoolExtensionQuery(sid, extId.Value, SchoolExtensionType.Extension, isAll, userId)).Result;
                if (result != null)
                {
                    dto = (SchoolExtensionDto)result;
                }
                else
                    return RedirectToAction("Main", new { sid = sid });
            }
            ViewBag.GradeSelect = EnumUtil.GetSelectItems<SchoolGrade>();
            var schoolType = EnumUtil.GetSelectItems<SchoolType>();
            ViewBag.SchoolTypeSelect = schoolType;
            ViewBag.SchoolTypeJson = JsonSerializationHelper.Serialize(schoolType);
            return View(dto);
        }

        /// <summary>
        /// 学校简介保存
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Step1(SchoolExtensionDto dto, string operation)
        {
            if (dto.Sid == Guid.Empty)
            {
                return Json(new HttpResponse<string> { State = 400, Message = "学校Id不能为空。" });
            }
            if (dto.Grade == 0)
            {
                return Json(new HttpResponse<string> { State = 400, Message = "年级不能为空。" });
            }
            if (dto.Type == 0)
            {
                return Json(new HttpResponse<string> { State = 400, Message = "办学类型不能为空。" });
            }
            var dt = ((SchoolGrade)dto.Grade, (SchoolType)dto.Type, dto.Discount, dto.Diglossia, dto.Chinese);
            if (fix4select(ref dt) is HttpResponse<string> hr) return Json(hr);
            HttpResponse<string> fix4select(ref (SchoolGrade Grade, SchoolType Type, bool Discount, bool Diglossia, bool Chinese) d)
            {
                //fix for ui selected
                switch (d.Grade)
                {
                    case SchoolGrade.Kindergarten when d.Type == SchoolType.International:
                    case SchoolGrade.Kindergarten when d.Type == SchoolType.Other:
                    case SchoolGrade.Kindergarten when d.Type == SchoolType.Public:
                        d.Discount = d.Diglossia = d.Chinese = false; //null
                        return null;
                    case SchoolGrade.Kindergarten when d.Type == SchoolType.Private:
                        d.Diglossia = d.Chinese = false; //null
                        return null;

                    case SchoolGrade.PrimarySchool when d.Type == SchoolType.Public:
                    case SchoolGrade.PrimarySchool when d.Type == SchoolType.ForeignNationality:
                        d.Discount = d.Diglossia = d.Chinese = false; //null
                        return null;
                    case SchoolGrade.PrimarySchool when d.Type == SchoolType.Private:
                        d.Discount = d.Chinese = false; //null
                        return null;

                    case SchoolGrade.JuniorMiddleSchool when d.Type == SchoolType.Public:
                    case SchoolGrade.JuniorMiddleSchool when d.Type == SchoolType.ForeignNationality:
                        d.Discount = d.Diglossia = d.Chinese = false; //null
                        return null;
                    case SchoolGrade.JuniorMiddleSchool when d.Type == SchoolType.Private:
                        d.Discount = d.Chinese = false; //null
                        return null;

                    case SchoolGrade.SeniorMiddleSchool when d.Type == SchoolType.Public:
                        d.Discount = d.Diglossia = d.Chinese = false; //null
                        return null;
                    case SchoolGrade.SeniorMiddleSchool when d.Type == SchoolType.Private:
                        d.Discount = d.Chinese = false; //null
                        return null;
                    case SchoolGrade.SeniorMiddleSchool when d.Type == SchoolType.International:
                    case SchoolGrade.SeniorMiddleSchool when d.Type == SchoolType.ForeignNationality:
                        d.Discount = d.Diglossia = false; //null
                        return null;

                    default:
                        if (d.Type != SchoolType.SAR) return new HttpResponse<string> { State = 400, Message = "未知的学校类型" };
                        d.Discount = d.Diglossia = d.Chinese = false; //null
                        return null;
                }
            }
            dto.Discount = dt.Discount;
            dto.Diglossia = dt.Diglossia;
            dto.Chinese = dt.Chinese;
            if (!string.IsNullOrEmpty(dto.NickName))
            {
                dto.NickName = JsonConvert.SerializeObject(dto.NickName.Split(","));
            }
            //check schftype
            var attrs = SchFTypeUtil.CodeAttrs(dto.Grade, dto.Type, dto.Discount, dto.Diglossia, dto.Chinese);
            var x = SchFTypeUtil.Dict.SingleOrDefault(_ => _.attrs == attrs);
            if (string.IsNullOrEmpty(x.code)) return Json(new HttpResponse<string> { State = 400, Message = "未知的学校类型" });
            if (dto.ExtId == null) dto.SchFtype = x.code;
            else if (dto.SchFtype != x.code) return Json(new HttpResponse<string> { State = 400, Message = "未知学校总类型" });
            //去重非空
            dto.Source = dto.Source.Distinct().Where(p => !string.IsNullOrEmpty(p)).ToArray();
            var extId = dto.ExtId;
            var userId = HttpContext.GetUserId();
            dto.UserId = userId;
            //判断
            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            var result = _mediator.Send(new AddSchoolExtCommand(dto, extId == null ? DataOperation.Insert : DataOperation.Update, isAll)).Result;
            if (result.State != 200)
            {
                return Json(result);
            }
            else
            {
                return Json(new HttpResponse<string>
                {
                    State = 200,
                    Result = operation == "last"
                    ? Url.Action("Step2", new { sid = dto.Sid, extId = result.Result }) :
                    Url.Action("Main", new { sid = dto.Sid })
                });
            }
        }


        /// <summary>
        /// 同步学部信息
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="extId"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExtFieldsSync(Guid sid, Guid extId)
        {
            if (sid == Guid.Empty || extId == Guid.Empty)
            {
                return RedirectToAction("Main", new { sid = sid });
            }

            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            var userId = HttpContext.GetUserId();


            //获取配置文件的内容并添加缓存中
            //_cacheManager.Remove(_syncKey);
            //var obj = _cacheManager.Get<SchoolExtFieldSyncConfigDto>(_syncKey);
            //if (obj == null)
            //{
            var jsonFieldsStr = System.IO.File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SchFieldSyncConfig.json"));
            var obj = jsonFieldsStr.ToObject<SchoolExtFieldSyncConfigDto>();
            //    _cacheManager.Add(_syncKey, obj);
            //}
            //待优化TODO
            var data = _mediator.Send(new GetSchoolSyncQuery(sid, extId, obj)).Result;
            if (data == null)
            {
                //找不到学部
                return RedirectToAction("Main", new { sid = sid });
            }


            //城市数据
            if (obj.Fields.Count(p => p.TextFormat.Equals(EnumUtil.GetDesc(TextFormat.ReginClickOn))) > 0)
            {
                ViewBag.ProvinceSelect = _mediator.Send(new KVQuery { ParentId = 0, Type = 1 })
                    .Result
                    .Select(p => new SelectListItem { Text = p.Name, Value = p.Id.ToString() });

                IEnumerable<SelectListItem> CitySelect = new List<SelectListItem>();
                if (data.Keys.Contains("province") && data["province"] != null && (int)(data["province"]) != 0)
                {
                    CitySelect = _mediator.Send(new KVQuery { ParentId = (int)(data["province"]), Type = 1 })
                    .Result
                    .Select(p => new SelectListItem { Text = p.Name, Value = p.Id.ToString() });
                }
                ViewBag.CitySelect = CitySelect;
                IEnumerable<SelectListItem> AreaSelect = new List<SelectListItem>();
                if (data.Keys.Contains("city") && data["city"] != null && (int)data["city"] != 0)
                {
                    AreaSelect = _mediator.Send(new KVQuery { ParentId = (int)data["city"], Type = 1 })
                    .Result
                    .Select(p => new SelectListItem { Text = p.Name, Value = p.Id.ToString() });
                }
                ViewBag.AreaSelect = AreaSelect;
            }


            ViewBag.SchoolExtFieldSyncConfigDto = obj;
            ViewBag.Sid = sid;
            ViewBag.Eid = extId;
            ViewBag.IsTrue = obj.Fields.Count(p => p.TextFormat == "reginclickon") > 0 ? 1 : 0;
            return PartialView(data);

        }

        /// <summary>
        /// 同步操作
        /// 同步字段 v1.1版本，调整为可同步修改，也就是说当前所有的字段都可以同步修改
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="eid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExtFieldsSync(Guid sid, Guid eid, string name)
        {
            var userId = HttpContext.GetUserId();
            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            List<KeyValueDto<string, object, int>> data = new List<KeyValueDto<string, object, int>>();
            //获取json配置文件
            var jsonFieldsStr = System.IO.File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SchFieldSyncConfig.json"));
            var obj = jsonFieldsStr.ToObject<SchoolExtFieldSyncConfigDto>();
            var field = obj.Fields.FirstOrDefault(p => p.Name.Equals(name));

            //分部信息
            var extdata = _mediator.Send(new GetSchoolSyncQuery(sid, eid, obj)).Result;

            #region TextFormat=tagcheckon|video
            //如果是（视频控件，标签选择器）只是同步临时判断 改为可编辑"tagcheckon",
            if (new string[] { "video" }.Contains(field.TextFormat))
            {
                foreach (var item in field.DBfield)
                {
                    data.Add(new KeyValueDto<string, object, int> { Key = item, Value = Request.Form[item], Message = (int)FielDataType.String });
                }
                var result = _mediator.Send(new SyncSchoolExtFieldCommand(sid, eid, data, field.DBtable, field.VideoType != null, field, userId, extdata));
                return Json(result);
            }
            #endregion

            #region TextFormat=editorvideo
            //   富文本加视频上传   第一个可编辑  第二个只能同步 
            //第二版  改为都可以同步修改
            else if (field.TextFormat.Equals("editorvideo"))
            {
                //第一个字段的名称
                var first = field.DBfield[0];
                var second = field.DBfield[1];
                var key = Request.Form.Keys.FirstOrDefault(p => p.ToLower().Equals(first));
                data.Add(new KeyValueDto<string, object, int> { Key = first, Value = Request.Form[key], Message = (int)FielDataType.String });
                data.Add(new KeyValueDto<string, object, int> { Key = field.DBfield[1], Value = Request.Form[second], Message = (int)FielDataType.String });
                var result = _mediator.Send(new SyncSchoolExtFieldCommand(sid, eid, data, field.DBtable, field.VideoType != null, field, userId, extdata));
                return Json(result);
            }
            #endregion

            #region TextFormat=characteristiceditor
            //学校特色课程或项目组合控件  第一个只能同步  第二个可编辑
            //第二版变更为同步可编辑
            else if (field.TextFormat.Equals("characteristiceditor"))
            {
                var first = field.DBfield[0];
                var second = field.DBfield[1];

                var value = "[]";
                var strValue = Request.Form[first].ToString().Replace("\\", "").Replace("\"[{", "[{").Replace("}]\"", "}]");
                if (strValue.Contains("Key"))
                {
                    value = strValue;
                }
                data.Add(new KeyValueDto<string, object, int> { Key = first, Value = value, Message = (int)FielDataType.String });
                //data.Add(new KeyValueDto<string, object, int> { Key = first, Value = Request.Form[first], Message = (int)FielDataType.String });
                data.Add(new KeyValueDto<string, object, int> { Key = second, Value = Request.Form[second], Message = (int)FielDataType.String });
                var result = _mediator.Send(new SyncSchoolExtFieldCommand(sid, eid, data, field.DBtable, field.VideoType != null, field, userId, extdata));
                return Json(result);
            }
            #endregion

            #region TextFormat=tagcheckon 
            //点选标签
            else if (field.TextFormat.Equals("tagcheckon"))
            {
                var dbfield = field.DBfield[0];
                var value = "[]";
                if (!string.IsNullOrEmpty(dbfield))
                {
                    var strValue = Request.Form[dbfield].ToString().Replace("\\", "").Replace("\"[{", "[{").Replace("}]\"", "}]");
                    if (strValue.Contains("Key"))
                    {
                        value = strValue;
                    }
                    data.Add(new KeyValueDto<string, object, int> { Key = dbfield, Value = value, Message = (int)FielDataType.String });
                }
                var result = _mediator.Send(new SyncSchoolExtFieldCommand(sid, eid, data, field.DBtable, field.VideoType != null, field, userId, extdata));
                return Json(result);

            }
            #endregion

            #region TextFormat=textdatetime 
            //开放日及行事历 (txt控件，时间控件)组合控件  openhours,calendar
            else if (field.TextFormat.Equals("textdatetime"))
            {
                var dbfield = field.DBfield[0];
                List<KeyValueDto<string>> listNameTime = null;
                if (!string.IsNullOrEmpty(dbfield))
                {
                    listNameTime = new List<KeyValueDto<string>>();
                    var arrName = Request.Form[dbfield + "1"].ToString().Split(',');
                    var arrTime = Request.Form[dbfield + "2"].ToString().Split(',');
                    if (arrName != null && arrName.Count() > 0 && arrTime != null && arrTime.Count() > 0)
                    {
                        var valueCount = arrName.Count() > arrTime.Count() ? arrTime.Count() : arrName.Count();
                        for (int i = 0; i < valueCount; i++)
                        {
                            if (!string.IsNullOrEmpty(arrName[i]) && !string.IsNullOrEmpty(arrTime[i]))
                            {
                                listNameTime.Add(new KeyValueDto<string> { Key = arrName[i], Value = arrTime[i] });
                            }
                        }
                    }
                }
                data.Add(new KeyValueDto<string, object, int> { Key = dbfield, Value = JsonSerializationHelper.Serialize(listNameTime) });
                var result = _mediator.Send(new SyncSchoolExtFieldCommand(sid, eid, data, field.DBtable, field.VideoType != null, field, userId, extdata));
                return Json(result);

            }
            #endregion

            #region TextFormat=clickon && 走读寄宿
            //走读寄宿
            else if (field.TextFormat.Equals("clickon") && field.DBfield[0] == "lodging")
            {
                var first = field.DBfield[0];
                var second = field.DBfield[1];
                var dic = BusinessHelper.SaveLodgingSdExtern(Request.Form[first].ToString());
                data.Add(new KeyValueDto<string, object, int> { Key = first, Value = dic[first] });
                data.Add(new KeyValueDto<string, object, int> { Key = second, Value = dic[second] });

                var result = _mediator.Send(new SyncSchoolExtFieldCommand(sid, eid, data, field.DBtable, field.VideoType != null, field, userId, extdata));
                return Json(result);

            }
            #endregion

            #region 其他
            else
            {
                //遍历表单中所有的key
                foreach (var key in Request.Form.Keys)
                {
                    var dbfield = field.DBfield.FirstOrDefault(p => p.Contains(key.ToLower()));
                    if (!string.IsNullOrEmpty(dbfield))
                    {
                        data.Add(new KeyValueDto<string, object, int> { Key = dbfield, Value = Request.Form[key] });
                    }
                }
                var result = _mediator.Send(new SyncSchoolExtFieldCommand(sid, eid, data, field.DBtable, field.VideoType != null, field, userId, extdata));
                return Json(result);
            }
            #endregion
        }

        /// <summary>
        /// 学校概况
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Step2(Guid sid, Guid extId)
        {

            if (sid == Guid.Empty || extId == Guid.Empty)
            {
                return RedirectToAction("Main", new { sid = sid });
            }
            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            var userId = HttpContext.GetUserId();
            var data = _mediator.Send(new GetSchoolExtContentQuery(sid, extId, isAll, userId)).Result;
            if (data == null)
            {
                //找不到学部
                return RedirectToAction("Main", new { sid = sid });
            }

            ViewBag.ExtId = extId;
            ViewBag.Sid = sid;
            ViewBag.Menus = _mediator.Send(new GetSchoolExtMenuQuery(extId)).Result;
            //城市数据
            ViewBag.ProvinceSelect = _mediator.Send(new KVQuery { ParentId = 0, Type = 1 })
                .Result
                .Select(p => new SelectListItem { Text = p.Name, Value = p.Id.ToString() });

            IEnumerable<SelectListItem> CitySelect = new List<SelectListItem>();
            if (data != null && data.Province != null && data.Province != 0)
            {
                CitySelect = _mediator.Send(new KVQuery { ParentId = data.Province.Value, Type = 1 })
                .Result
                .Select(p => new SelectListItem { Text = p.Name, Value = p.Id.ToString() });
            }
            ViewBag.CitySelect = CitySelect;
            IEnumerable<SelectListItem> AreaSelect = new List<SelectListItem>();
            if (data != null && data.City != null && data.City != 0)
            {
                AreaSelect = _mediator.Send(new KVQuery { ParentId = data.City.Value, Type = 1 })
                .Result
                .Select(p => new SelectListItem { Text = p.Name, Value = p.Id.ToString() });
            }
            ViewBag.AreaSelect = AreaSelect;

            ViewBag.LodgingSdExternSelect = EnumUtil.GetDescs<LodgingSdExternOptions>();
            return View(data);
        }

        /// <summary>
        /// 获取表单数据
        /// </summary>
        /// <param name="year"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetAchievementData(int year, Guid id)
        {
            if (id == Guid.Empty)
            {
                return Json(new HttpResponse<string> { State = 404 });
            }
            var result = _mediator.Send(new GetAchievementDataQuery(year, id));
            return Json(result.Result);
        }

        /// <summary>
        /// 升学成绩弹出框
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult AchievementPanel(int year, Guid extId, byte grade, byte type)
        {
            ViewBag.ExtId = extId;
            ViewBag.Year = year;
            ViewBag.Grade = grade;
            ViewBag.Type = type;
            var result = _mediator.Send(new GetAchievementListQuery(extId, year, grade)).Result;
            return PartialView(result);
        }

        /// <summary>
        /// 删除升学成绩
        /// </summary>
        /// <param name="year"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DelAchievementData(int year, Guid id)
        {
            if (id == Guid.Empty)
                return Json(new HttpResponse<string> { State = 404 });
            var userId = HttpContext.GetUserId();
            var result = _mediator.Send(new DelSchoolExtAchDataCommand(year, id, userId));
            return Json(result.Result);
        }

        /// <summary>
        /// 保存升学成绩
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveAchievement(byte grade, string json)
        {
            var userId = HttpContext.GetUserId();
            if (grade == (byte)SchoolGrade.SeniorMiddleSchool)
            {
                var command = JsonSerializationHelper.JSONToObject<AddHighSchoolAchCommand>(json);
                command.UserId = userId;
                var result = _mediator.Send(command).Result;
                return Json(result);
            }
            else if (grade == (byte)SchoolGrade.JuniorMiddleSchool)
            {
                var command = JsonSerializationHelper.JSONToObject<AddMiddleSchoolAchCommand>(json);
                command.UserId = userId;
                var result = _mediator.Send(command).Result;
                return Json(result);
            }
            else if (grade == (byte)SchoolGrade.PrimarySchool)
            {
                var command = JsonSerializationHelper.JSONToObject<AddPrimarySchoolAchCommand>(json);
                command.UserId = userId;
                var result = _mediator.Send(command).Result;
                return Json(result);
            }
            else if (grade == (byte)SchoolGrade.Kindergarten)
            {
                var command = JsonSerializationHelper.JSONToObject<AddKindergartenSchoolAchCommand>(json);
                command.UserId = userId;
                var result = _mediator.Send(command).Result;
                return Json(result);
            }
            return Json(new HttpResponse<string> { State = 404, Message = "没有找到当前分部！" });
        }

        /// <summary>
        /// 保存学校成绩的列表
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Count"></param>
        /// <param name="ExtId"></param>
        /// <param name="year"></param>
        /// <param name="Completion"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveAchievementList(Guid[] Id, int[] Count, Guid ExtId, int year, float Completion)
        {

            var length = Id.Length > Count.Length ? Count.Length : Id.Length;
            SchoolAchItem[] data = new SchoolAchItem[length];
            if (length > 0)
            {
                for (int i = 0; i < length; i++)
                {
                    data[i] = new SchoolAchItem { Id = Id[i], Count = Count[i] };
                }
            }
            var userId = HttpContext.GetUserId();

            var result = _mediator
                .Send(new AddSchoolAchListCommand
                {
                    Data = data,
                    ExtId = ExtId,
                    Year = year,
                    Completion = Completion,
                    UserId = userId
                }).Result;
            return Json(result);
        }




        /// <summary>
        /// 学校概况保存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Step2(AddSchoolExtContentCommand dto, string operation)
        {
            if (string.IsNullOrEmpty(dto.Address))
                return Json(new HttpResponse<string> { State = 500, Message = "地址不能未空！" });
            if (dto.Eid == Guid.Empty)
                return Json(new HttpResponse<string> { State = 404, Message = "没有找到这个学部！" });
            var userId = HttpContext.GetUserId();
            dto.UserId = userId;
            dto.LodgingExtern = BusinessHelper.SaveLodgingSdExtern(dto.Lodging);
            dto.CurrentVideoTypes = $" and type in ({(int)VideoType.Experience}, {(int)VideoType.Interview}, {(int)VideoType.Profile})  ";
            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            var result = _mediator.Send(dto).Result;
            if (result.State != 200)
            {
                return Json(result);
            }
            else
            {
                return Json(new HttpResponse<string>
                {
                    State = 200,
                    Result = operation.Trim() == "last" ?
                    Url.Action("Step3", new { sid = dto.Sid, extId = dto.Eid }) :
                    Url.Action("Step1", new { sid = dto.Sid, extId = dto.Eid })
                });
            }


        }




        /// <summary>
        /// 第三步（招生简章）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Step3(Guid sid, Guid extId)
        {
            if (extId == Guid.Empty) return RedirectToAction("Main", new { sid = sid });
            //判断
            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            var userId = HttpContext.GetUserId();

            var dto = _mediator.Send(new GetSchoolExtRecruitQuery(extId, sid, isAll, userId)).Result;
            if (dto == null)
            {
                return RedirectToAction("Main", new { Sid = sid });
            }
            ViewBag.ExtId = extId;
            ViewBag.Sid = sid;
            ViewBag.Menus = _mediator.Send(new GetSchoolExtMenuQuery(extId)).Result;
            //var data = _mediator.Send(new GetSchoolExtContentQuery(sid, extId, isAll, userId)).Result;
            var message = _mediator.Send(new GetAuditMessageQuery(sid)).Result;
            dto.CurrAuditMessage = message;


            return View(dto);
        }

        /// <summary>
        /// 保存Step3的数据
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Step3([FromBody] AddSchoolExtRecruitCommand cmd)
        {
            var userId = HttpContext.GetUserId();
            cmd.UserId = userId;

            var r = await _mediator.Send(cmd);
            if (r.State != 200)
            {
                return Json(r);
            }
            else
            {
                return Json(new HttpResponse<string>
                {
                    State = 200,
                    Result = cmd.Operation.Trim() == "last"
                        ? Url.Action("Step4", new { sid = cmd.Sid, extId = cmd.Eid })
                        : Url.Action("Step2", new { sid = cmd.Sid, extId = cmd.Eid })
                });
            }
        }

        /// <summary>
        /// 第四步（课程体系）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Step4(Guid sid, Guid extId)
        {
            if (extId == Guid.Empty) return RedirectToAction("Main", new { sid = sid });
            //判断
            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            var userId = HttpContext.GetUserId();
            var dto = _mediator.Send(new GetSchoolExtCourseQuery(sid, extId, isAll, userId)).Result;
            if (dto == null)
            {
                return RedirectToAction("Main", new { Sid = sid });
            }
            ViewBag.ExtId = extId;
            ViewBag.Sid = sid;
            ViewBag.Menus = _mediator.Send(new GetSchoolExtMenuQuery(extId)).Result;
            if (dto == null)
            {
                return RedirectToAction("Main", new { Sid = sid });
            }
            //var data = _mediator.Send(new GetSchoolExtContentQuery(sid, extId, isAll, userId)).Result;
            var message = _mediator.Send(new GetAuditMessageQuery(sid)).Result;
            ViewBag.CurrAuditMessage = message;
            return View(dto);
        }

        /// <summary>
        /// 第四步保存（课程体系）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Step4(AddSchoolExtCourseCommand command, string operation)
        {
            var userId = HttpContext.GetUserId();
            command.UserId = userId;
            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            var result = _mediator.Send(command).Result;

            if (result.State != 200)
            {
                return Json(result);
            }
            else
            {
                return Json(new HttpResponse<string>
                {
                    State = 200,
                    Result = operation.Trim() == "last" ?
                    Url.Action("Step5", new { sid = command.Sid, extId = command.Eid }) :
                    Url.Action("Step3", new { sid = command.Sid, extId = command.Eid })
                });
            }
        }

        /// <summary>
        /// 第五步（收费标准）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Step5(Guid sid, Guid extId = default)
        {
            if (sid == Guid.Empty || extId == Guid.Empty)
            {
                return RedirectToAction(nameof(Main));
            }
            //判断
            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            var userId = HttpContext.GetUserId();

            var res = _mediator.Send(new GetSchoolExtChargeQuery(sid, extId, isAll, userId)).Result;
            if (res?.Dto == null)
            {
                //找不到学部
                return RedirectToAction(nameof(Main), new { sid });
            }

            ViewBag.Menus = _mediator.Send(new GetSchoolExtMenuQuery(extId)).Result;
            ViewBag.sid = sid;
            ViewBag.eid = extId;
            ViewBag.Ext = res.Ext;
            var message = _mediator.Send(new GetAuditMessageQuery(sid)).Result;
            ViewBag.CurrAuditMessage = message;

            if (res.Dto == null)
            {
                return RedirectToAction("Main", new { Sid = sid });
            }
            return View(res.Dto);
        }
        /// <summary>
        /// 第五步保存（收费标准）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Step5(AddSchoolExtChargeCommand cmd, string op, float ac = 3.0f)
        {
            if (cmd.Eid == Guid.Empty)
            {
                return Json(new HttpResponse<string> { State = 400, Message = "学部Id不能为空。" });
            }

            //发现post form时, 空数组会变成null ！？            
            if (cmd.Yearslist == null) cmd.Yearslist = new YearChange[0];

            //计算完成度
            var i = 0;
            i += cmd.Yearslist.Any(_ => _.Field == SchoolExtFiledYearTag.Tuition.GetName() && _.Act != YearChange.Act_remove) ? 1 : 0;
            i += cmd.Yearslist.Any(_ => _.Field == SchoolExtFiledYearTag.Applicationfee.GetName() && _.Act != YearChange.Act_remove) ? 1 : 0;
            i += cmd.Yearslist.Any(_ => _.Field == SchoolExtFiledYearTag.Otherfee.GetName() && _.Act != YearChange.Act_remove) ? 1 : 0;
            cmd.Completion = ac == i ? 1 : ac == 0 ? 0 : i / ac;
            cmd.Completion = cmd.Completion > 1 ? 1 : cmd.Completion;

            cmd.CurrentUserId = HttpContext.GetUserId();

            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            var res = _mediator.Send(cmd).Result;

            res.Result = op.Trim() == "last"
                ? Url.Action("Step6", new { sid = cmd.Sid, extId = cmd.Eid })
                : Url.Action("Step4", new { sid = cmd.Sid, extId = cmd.Eid });

            return Json(res);
        }

        /// <summary>
        /// 第六步（师资力量及教学质量）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Step6(Guid sid, Guid extId)
        {
            if (sid == Guid.Empty || extId == Guid.Empty)
            {
                return RedirectToAction(nameof(Main));
            }
            //判断
            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            var userId = HttpContext.GetUserId();

            var res = _mediator.Send(new GetSchoolExtQualityQuery(sid, extId, isAll, userId)).Result;
            if (res == null)
            {
                //找不到学部
                return RedirectToAction(nameof(Main), new { sid });
            }

            ViewBag.Menus = _mediator.Send(new GetSchoolExtMenuQuery(extId)).Result;
            ViewBag.sid = sid;
            ViewBag.eid = extId;
            ViewBag.Ext = res.Ext;

            var message = _mediator.Send(new GetAuditMessageQuery(sid)).Result;
            ViewBag.CurrAuditMessage = message;

            if (res.Dto == null)
            {
                return RedirectToAction("Main", new { Sid = sid });
            }

            #region 视频信息
            var vInfo = Enumerable.Range(0, res?.Dto?.Videos?.Length ?? 0).Select(i =>
            {
                return new VideosInfo()
                {
                    Cover = res.Dto.Covers[i],
                    Type = (byte)VideoType.Principal,
                    VideoUrl = res.Dto.Videos[i],
                    VideoDesc = res.Dto.VideoDescs[i]
                };
            });
            ViewBag.VideosInfo = vInfo;
            #endregion

            return View(res.Dto);
        }
        /// <summary>
        /// 第六步保存（师资力量及教学质量）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Step6(SchoolExtQualityDto dto, string op, float ac = 2.0f)
        {
            if (dto.Eid == Guid.Empty)
            {
                return Json(new HttpResponse<string> { State = 400, Message = "学部Id不能为空。" });
            }
            if (dto.Videos != null)
            {
                dto.Videos = dto.Videos.Distinct().ToArray();
            }
            dto.CurrentVideoTypes = $" and type in ({(int)VideoType.Principal})  ";
            var cmd = new AddSchoolExtQualityCommand();
            cmd.Dto = dto;
            cmd.CurrentUserId = HttpContext.GetUserId();
            //var isAll = HttpContext.HasCurrQyx(".qx-schAll");

            var res = _mediator.Send(cmd).Result;

            res.Result = op.Trim() == "last"
                ? Url.Action("Step7", new { sid = dto.Sid, extId = dto.Eid })
                : Url.Action("Step5", new { sid = dto.Sid, extId = dto.Eid });

            return Json(res);
        }


        /// <summary>
        /// 第七步(硬件设施及学生生活)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Step7(Guid sid, Guid extId)
        {
            if (sid == Guid.Empty || extId == Guid.Empty)
            {
                return RedirectToAction(nameof(Main));
            }
            //判断
            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            var userId = HttpContext.GetUserId();
            var res = _mediator.Send(new GetSchoolExtLifeQuery(sid, extId, isAll, userId)).Result;
            if (res == null)
            {
                //找不到学部
                return RedirectToAction(nameof(Main), new { sid });
            }

            ViewBag.Menus = _mediator.Send(new GetSchoolExtMenuQuery(extId)).Result;
            ViewBag.sid = sid;
            ViewBag.eid = extId;
            ViewBag.Ext = res.Ext;
            var message = _mediator.Send(new GetAuditMessageQuery(sid)).Result;
            ViewBag.CurrAuditMessage = message;
            if (res.Dto == null)
            {
                return RedirectToAction("Main", new { Sid = sid });
            }
            return View(res.Dto);
        }
        /// <summary>
        /// 第七步保存（硬件设施及学生生活）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Step7(SchoolExtLifeDto dto)
        {
            if (dto.Eid == Guid.Empty)
            {
                return Json(new HttpResponse<string> { State = 400, Message = "学部Id不能为空。" });
            }

            var cmd = new AddSchoolExtLifeCommand();
            cmd.Dto = dto;
            cmd.CurrentUserId = HttpContext.GetUserId();
            //var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            var res = _mediator.Send(cmd).Result;

            return Json(res);
        }


        /// <summary>
        /// 完成
        /// </summary>
        /// <returns></returns>
        public IActionResult Complete(Guid sid, Guid extId)
        {
            if (sid == Guid.Empty || extId == Guid.Empty)
            {
                return RedirectToAction(nameof(Main));
            }
            //判断
            var isAll = HttpContext.HasCurrQyx(".qx-schAll");
            var userId = HttpContext.GetUserId();
            var res = _mediator.Send(new GetSchoolExtLifeQuery(sid, extId, isAll, userId)).Result;
            if (res == null)
            {
                //找不到学部
                return RedirectToAction(nameof(Main), new { sid });
            }

            ViewBag.Menus = _mediator.Send(new GetSchoolExtMenuQuery(extId)).Result;
            ViewBag.sid = sid;
            ViewBag.eid = extId;

            return View();
        }

        /// <summary>
        /// 预览
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Preview(Guid sid, Guid? extId)
        {
            var req = new PreviewSchoolQuery { Sid = sid, Eid = extId };
            var res = _mediator.Send(req).Result;

            ViewBag.eid = req.Eid;
            return View(res);
        }





        /// <summary>
        /// 学校内容
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult SchoolContent(string extId)
        {
            return PartialView();
        }

        /// <summary>
        /// 学校排行榜
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult SchoolAchievement()
        {
            ViewBag.ModalName = "录入成绩";
            return PartialView();
        }


        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="type"></param>
        /// /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [DisableRequestSizeLimit]
        public IActionResult Upload(int type, Guid id)
        {
            if (!Directory.Exists(_hostingEnvironment.WebRootPath + @"\images\temp"))
            {
                try { Directory.CreateDirectory(_hostingEnvironment.WebRootPath + @"\images\temp"); }
                catch { }
            }

            var files = Request.Form.Files;
            //如果上传文件未null
            if (files == null || files.Count() == 0)
            {
                return Json(new HttpResponse<string> { State = 200, Message = "上传文件不能为空" });
            }
            //logo
            if (type == (int)UploadType.Logo)
            {
                var file = files[0];
                var extension = string.Empty;

                ////获取裁剪参数
                //var json = Request.Form["avatar_data"].ToString();
                //JObject jo = JObject.Parse(json);
                //var result = UploadHelper.TransportImage(file, "logo", id.ToString(),
                //   Convert.ToInt32((float)jo["width"]), Convert.ToInt32((float)jo["height"]), Convert.ToInt32((float)(jo["x"])), Convert.ToInt32((float)(jo["y"])),
                //    out extension, _appSettings.UploadUrl, _hostingEnvironment.WebRootPath + "\\images\\temp\\");

                var result = UploadHelper.TransportImage(file, "logo", id.ToString(),
                    out extension,
                    _appSettings.UploadUrl, _hostingEnvironment.WebRootPath + "\\images\\temp\\");

                if (string.IsNullOrEmpty(result.url))
                    return Json(new HttpResponse<string> { State = 500 });
                else
                    return Json(new HttpResponse<string> { State = 200, Result = result.compressUrl + "?t=" + DateTime.Now.Ticks.ToString() });
            }
            else if (type == (int)UploadType.Video)
            {
                //video
                var uploads = UploadHelper.PostVideoToHulyega(id.ToString(), _appSettings.UploadUrl, files.Select(p => p).ToArray());
                var result = uploads.Select(p => new KeyValueDto<string, string, string, string>
                {
                    Key = p.fileName,
                    Value = p.videoUrl.ToString(),
                    Message = p.compressCoverUrl,
                    Desc = ""
                });
                return Json(new HttpResponse<IEnumerable<KeyValueDto<string, string, string, string>>> { State = 200, Result = result });
            }
            else if (type == (int)UploadType.Others)
            {
                var uploads = UploadHelper.TransportFiles(id.ToString(), _appSettings.UploadUrl, files.Select(p => p).ToArray());
                var result = uploads.Select(p => new KeyValueDto<string>
                {
                    Key = p.Key.ToString(),
                    Value = p.Value,
                });
                return Json(new HttpResponse<IEnumerable<KeyValueDto<string>>> { State = 200, Result = result });
            }
            else if (type == (int)UploadType.Gallery)
            {//画廊
                var file = files[0];
                var extension = string.Empty;
                var json = Request.Form["avatar_data"].ToString();
                JObject jo = JObject.Parse(json);
                var width = Convert.ToInt32((float)jo["width"]);
                var height = Convert.ToInt32((float)jo["height"]);
                var x = Convert.ToInt32((float)(jo["x"]));
                var y = Convert.ToInt32((float)(jo["y"]));

                if (width == 0 || height == 0)
                    return Json(new HttpResponse<string> { State = 201, Message = "缩略图的长度或宽度不能为零" });

                var fileName = Guid.NewGuid();
                var result = UploadHelper.TransportImage(file, fileName.ToString(), id.ToString(),
                    width, height, x, y, out extension, _appSettings.UploadUrl);

                return Json(new HttpResponse<string> { State = 200, Result = result.url, Message = result.compressUrl });
            }
            return Json("");
        }



        /// <summary>
        /// 校验学校名字
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CheckSchoolName(CheckSchoolNameQuery query)
        {
            if (string.IsNullOrEmpty(query.Name.Trim()))
            {
                return Json(new HttpResponse<string> { State = 200 });
            }
            var data = _mediator.Send(query).Result;
            return Json(data);
        }


        /// <summary>
        /// 学校搜索
        /// </summary>
        /// <param name="top"></param>
        /// <param name="grade"></param>
        /// <param name="isCollage"></param>
        /// <param name="ContainExt"></param>
        /// <param name="IsOnline"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult SearchSchool(int top, byte grade, bool isCollage = false,
            bool ContainExt = true, bool IsOnline = true)
        {
            var text = Request.Query["data[q]"].ToString();
            if (string.IsNullOrEmpty(text))
            {
                return Json(new { Q = text, Results = new { } });
            }
            var result = _mediator.Send(new SearchSchoolQuery
            {
                Name = text,
                IsCollage = isCollage,
                Top = 10,
                Grade = grade,
                ContainExt = ContainExt,
                IsOnline = IsOnline
            }).Result;
            var json = result.Select(p => new { Id = p.Value, Text = p.Key });
            return Json(new { Q = text, Results = json });
        }

        /// <summary>
        /// 用于毕业去向的搜索
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchSachSchool(int top, byte grade, bool isCollage = false, bool IsOnline = true)
        {
            var text = Request.Query["data[q]"].ToString();
            if (string.IsNullOrEmpty(text))
            {
                return Json(new { Q = text, Results = new { } });
            }
            var result = await _mediator.Send(new SearchSachSchoolQuery
            {
                Name = text,
                IsCollage = isCollage,
                Top = 10,
                Grade = grade,
                IsOnline = IsOnline
            });
            var json = result.Select(p => new { Id = p.Value, Text = p.Key });
            return Json(new { Q = text, Results = json });
        }

        /// <summary>
        /// 城市联动数据
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ChangeCityData(int parentId)
        {
            var cityData = _mediator.Send(new KVQuery { ParentId = parentId, Type = 1 }).Result;
            var result = cityData.Select(p => new SelectListItem { Text = p.Name, Value = p.Id.ToString() });
            return Json(result);
        }
        /// <summary>
        /// 腾讯地图地址逆向解释
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetMapLatLnt(string address)
        {
            if (string.IsNullOrEmpty(address) || address.Length < 3)
                return Json(new { status = 500, message = "地址不能为空！" });
            var _mapapikey = _appSettings.QqMapKey;
            var url = $"https://apis.map.qq.com/ws/geocoder/v1/?address={address}&key={_mapapikey}";
            var returnModel = HttpHelper.Get<QqMapReverseAddress>(url);
            if (null == returnModel) return Json(new { status = 500, message = "地图API请求异常！" });
            return Json(returnModel);
        }

        #region 根据条件查询年份字段内容
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName">字段名称</param>
        /// <param name="eid">学部Id</param>
        /// <param name="year">年份</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetSchoolYearFieldContentByCondition(string fieldName, Guid eid, int year)
        {
            var result = await _mediator.Send(new GetSchoolYearFieldContentQuery(fieldName, eid, year));
            return Json(new { result });
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> FindByName(string name = null)
        {
            ViewBag.Sch_name = name;
            ViewBag.Schs = await _mediator.Send(new SchoolFindByNameQuery { Name = name });
            return View();
        }


    }
}