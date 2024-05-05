using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using iSchool.UI;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace iSchool.UI { }

namespace iSchool.Api
{
    //#region netcore 2.2 API
    //public class Program
    //{
    //    public static void Main(string[] args)
    //    {
    //        CreateWebHostBuilder(args).Build().Run();
    //    }

    //    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    //        WebHost.CreateDefaultBuilder(args)
    //            .UseKestrel()
    //            .UseIIS()
    //            .ConfigureAppConfiguration((hostingContext, config) =>
    //            {
    //                config.SetBasePath(Directory.GetCurrentDirectory());
    //            })
    //            .UseStartup<Startup>();
    //}
    //#endregion

    #region netcore 3.1 API
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                //.UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder //.UseIIS()
                        .UseStartup<Startup>();
                });
    }
    #endregion
}
