using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace iSchool.UI
{
    #region netcore2.2
    //public class Program
    //{
    //    public static void Main(string[] args)
    //    {
    //        //#if !DEBUG
    //        //            // 防止scd发布到iis时当前工作目录为'%windir%/...'
    //        //            try { Directory.SetCurrentDirectory(Path.GetDirectoryName(typeof(Program).GetTypeInfo().Assembly.ManifestModule.FullyQualifiedName)); }
    //        //            catch { }
    //        //#endif

    //        CreateWebHostBuilder(args)
    //            .Build()
    //            .Run();
    //    }

    //    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    //        WebHost.CreateDefaultBuilder(args)
    //            //.UseKestrel()
    //            .UseIIS()
    //            .ConfigureAppConfiguration((hostingContext, config) =>
    //            {
    //                config.SetBasePath(Directory.GetCurrentDirectory());
    //            })
    //            .UseStartup<Startup>();
    //} 
    #endregion

    #region netcore3.1
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                //.UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging(logging => 
                {
                    logging.AddConsole();

                    //log4net.Util.SystemInfo.NullText = null;
                    logging.AddLog4Net(new Log4NetProviderOptions("log4net.config", true)
                    {
                        LoggerRepository = "NETCoreRepository",
                    });
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder //.UseIIS()
                        .UseStartup<Startup>();
                });           
    }
    #endregion
}
