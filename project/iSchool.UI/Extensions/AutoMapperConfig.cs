using Microsoft.Extensions.DependencyInjection;
using System;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Application.AutoMapper;

namespace iSchool.UI.Extensions
{
    public static class AutoMapperSetup
    {

        /// <summary>
        /// automapper的配置文件
        /// </summary>
        /// <param name="services"></param>
        public static void AddAutoMapperSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("iSchool")));
            AutoMapperConfig.RegisterMappings();
        }
    }
}
