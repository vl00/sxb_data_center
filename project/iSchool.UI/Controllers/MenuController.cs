
using iSchool.Authorization;
using iSchool.Authorization.Models;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.UI.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.UI.Controllers
{
    public class MenuController : BaseController
    {
        /// <summary>
        /// 全民营销平台Id
        /// </summary>
        private const int MarketingPlatformId = 8;

        /// <summary>
        /// [前]获取当前登录用户的菜单
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ResponseResult<List<MenuRespDto>> Menus(int? platformId)
        {
            var marketingPlatformId = (byte)(platformId == null ? MarketingPlatformId : platformId ?? 0);

            var adminInfo = (new Account().Info(this.HttpContext));

            if (adminInfo == null) return ResponseResult<List<MenuRespDto>>.Success(new List<MenuRespDto>());

            var functions = adminInfo?.Character?.SelectMany((s, funs) =>
            {
                return s.Function?.Where(f => f.PlatformId == marketingPlatformId);
            });

            var points = MenuPoints(marketingPlatformId);
            var menus = functions.Select(s => new MenuRespDto()
            {
                MenuId = s.Id,
                MenuName = s.Name,
                MenuCode = s.Controller,
                Points = points.Where(point => point.MenuId == s.Id).ToList()
            }).ToList();

            return ResponseResult<List<MenuRespDto>>.Success(menus);
        }

        /// <summary>
        /// [前]获取当前登录用户的菜单功能点
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public List<MenuPointDto> MenuPoints(int? platformId)
        {
            var permission = new Permission();

            var marketingPlatformId = (byte) (platformId == null ? MarketingPlatformId :platformId ?? 0);
            var querys = permission.GetAllQueryByPlatformID(marketingPlatformId, (new Account().Info(this.HttpContext)).Id);

            var points = querys.Where(s => s.Id != Guid.Empty).Select(s => new MenuPointDto
            {
                Id = s.Id,
                MenuId = s.FunctionId,
                PointName = s.Name,
                PointCode = s.Selector
            }).ToList();

            return points;
        }
    }
}
