using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iSchool.Sites.Console.Content.ueditor.net.Utilities
{
    public class WaterMarkHandler// : Handler
    {
        private string Sources;
        private Crawler Crawler;
        //public WaterMarkHandler(HttpContext context) : base(context) { }

        //public override ActionResult Process
        //{
        //    get
        //    {
        //        //Sources = Request.Form["upfile"];
        //        //if (!Guid.TryParse(Request.QueryString["articleID"], out Guid articleID) || Sources == null || Sources.Length == 0)
        //        //{
        //        //    return Json(new
        //        //    {
        //        //        state = "参数错误：没有指定抓取源"
        //        //    }, JsonRequestBehavior.AllowGet);
        //        //}
        //        //bool.TryParse(Request.QueryString["watermark"], out bool watermark);
        //        //Crawler = new Crawler(Sources, Server).Fetch(watermark, articleID, Request.Form["DataUrl"]);
        //        //return Json(new
        //        //{
        //        //    state = Crawler.State,
        //        //    source = Crawler.SourceUrl,
        //        //    url = Crawler.ServerUrl
        //        //}, JsonRequestBehavior.AllowGet);
        //    }
        //}
    }
}