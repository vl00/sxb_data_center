using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;


/// <summary>
/// Handler 的摘要说明
/// </summary>
public abstract class Handler:Controller
{
	public Handler(HttpContext context)
	{
        this.Request = context.Request;
        this.Response = context.Response;
        this.Context = context;
        this.Server = context.Server;
	}

    public abstract ActionResult Process { get; }

    protected ActionResult WriteJson(object response)
    {
        string jsonpCallback = Request["callback"];
        if (string.IsNullOrWhiteSpace(jsonpCallback))
        {
            return Json(response, "text/plain", JsonRequestBehavior.AllowGet);
        }
        else 
        {
            string json = JsonConvert.SerializeObject(response);
            Response.AddHeader("Content-Type", "application/javascript");
            return Content(string.Format("{0}({1});", jsonpCallback, json));
        }
    }

    public HttpRequest Request { get; private set; }
    public HttpResponse Response { get; private set; }
    public HttpContext Context { get; private set; }
    public HttpServerUtility Server { get; private set; }
}