using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Application.Service;
using iSchool.Application.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace iSchool.UI.Controllers
{
    public class RankController : BaseController
    {

        private readonly IMediator _mediator;

        public RankController(IMediator mediator)
        {
            _mediator = mediator;
        }




        /// <summary>
        /// 创建排行榜名字
        /// </summary>
        /// <param name="year"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateName(int year, string name, bool isCollage)
        {
            var id = await _mediator.Send<Guid>(new AddRankNameCommand(year, name, isCollage));
            return RedirectToAction("Data", "Home", new { isCache = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAsync(Guid rank)
        {
            await _mediator.Send(new DelRankNameCommand(rank));
            return RedirectToAction("Data", "Home");
        }

        /// <summary>
        /// 排行榜详情
        /// </summary>
        /// <param name="year"></param>
        /// <param name="type"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult RankList(int year, int type, Guid rank)
        {
            ViewBag.ModalName = type == 1 ? "编辑排行榜" : "排行榜详情";
            ViewBag.Type = type;
            ViewBag.RankId = rank;
            var detail = _mediator.Send(new GetRankDetailQuery(rank, false));
            return PartialView(detail.Result);
        }



        /// <summary>
        /// 修改排名顺序
        /// </summary>
        /// <param name="schoolname"></param>
        /// <param name="rankId"></param>
        /// <param name="sid"></param>
        /// <param name="placing"></param>
        /// <param name="isJux">是否并列</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditRank(Guid rankId, Guid sid, double placing, bool
            isJux)
        {
            //将数据展示到前台
            var detail = _mediator.Send(new GetRankDetailQuery(rankId, false));
            return Json(detail.Result);
        }

        /// <summary>
        /// 排序(添加)排行榜中的学校 
        /// </summary>
        /// <param name="placing"></param>
        /// <param name="rankId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SortRank(double placing, Guid rankId, Guid sid, bool isJux)
        {
            List<RankDetailDto> detail = null;

            var result = _mediator.Send(new SortRankCommand(sid, rankId, placing, isJux));
            if (result.Result != 0)
            {
                //将数据展示到前台
                detail = _mediator.Send(new GetRankDetailQuery(rankId, false)).Result;
            }
            return Json(detail);
        }

        /// <summary>
        /// 删除排行版中的学校
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DelRankSchool(DelRankSchoolCommand command)
        {
            List<RankDetailDto> detail = null;

            var result = _mediator.Send(command).Result;
            if (result != 0)
            {
                //将数据展示到前台
                detail = _mediator.Send(new GetRankDetailQuery(command.RankId, false)).Result;
            }
            return Json(detail);
        }



    }
}