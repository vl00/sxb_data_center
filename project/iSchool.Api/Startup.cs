using System;
using System.IO;
using System.Net.Http;
using Autofac;
using iSchool.Api.Auths;
using iSchool.BgServices;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Cache;
using iSchool.UI.Extensions;
using iSchool.UI.Modules;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace iSchool.UI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            var logRepository = log4net.LogManager.CreateRepository("NETCoreRepository");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(logRepository, new FileInfo("log4net.config"));

            _ = iSchool.Authorization.Lib.UtilConf.Configuration;

            LngLatLocation.Init_With_Dapper();

            JsonExtensions.Converters.Add(new FnResultJsonConverter());
            JsonExtensions.Converters.Add(new SchFType0JsonConverter());            
        }

        /// <summary>
        /// gloab config
        /// </summary>
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            //memory cache
            services.AddSingleton<IMemoryCache>((sp) =>
            {
                return new MemoryCache(Configuration.GetSection("memoryCache").Get<MemoryCacheOptions>() ?? new MemoryCacheOptions { });
            });

            #region http client
            services.AddHttpClient(string.Empty, (httpClient) =>
            {
                httpClient.DefaultRequestHeaders.Clear();
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler() { UseProxy = false });

            services.AddHttpClient("baidu_push", (httpClient) =>
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "curl/7.12.1");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler() { UseProxy = false });
            #endregion

            //memcached配置
            services.AddEnyimMemcached(options => Configuration.GetSection("enyimMemcached").Bind(options));
            services.AddTransient(sp => 
            {
                return new CacheManager(sp.GetService<Enyim.Caching.IMemcachedClient>(), Configuration.GetValue<string>("enyimMemcached:ischooldata-prefixkey"));
            });

            //redis
            services.AddSingleton(sp =>
            {
                return new CSRedis.CSRedisClient(Configuration["redis:default"]);
            });

            //rabbit
            {
                services.AddSingleton(sp =>
                {
                    return Configuration.Bind("rabbit:conns:default", new RabbitMQ.Client.ConnectionFactory(), (config, connfaty) =>
                    {
                        connfaty.Uri = new Uri(config["Url"], UriKind.Absolute);
                    });
                });

                services.AddScoped(sp => new RabbitMQConnectionForPublish(sp.GetService<RabbitMQ.Client.ConnectionFactory>()));
            }

            //appsetting文件
            var appSettingConfig = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingConfig);
            services.AddScoped(sp => sp.GetRequiredService<IOptions<AppSettings>>().Value);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // audit
            services.AddSingleton(sp => new AuditOption());

            services.AddHttpContextAccessor();

            services.AddSingleton<Infrastructure.Logs.BufferLogsWatcher>();
            services.AddScoped<ILog, Logger>();

            //automapper
            services.AddAutoMapperSetup();

            //authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = Configuration["auth:cookie:loginPath"]; //"/home/login";
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = Configuration["auth:cookie:name"];
                options.Cookie.Domain = Configuration["auth:cookie:domain"];
                options.Cookie.Path = Configuration["auth:cookie:path"];
                options.DataProtectionProvider = DataProtectionProvider.Create(new DirectoryInfo(Configuration["auth:cookie:dataProtectionDir"]));
            })
            .AddScheme<JobsAuthenticationSchemeOptions, JobsAuthenticationHandler>("jobs", options => 
            {
                Configuration.GetSection("auth:jobs").Bind(options);
            });
            //authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("jobs", builder => builder.AddAuthenticationSchemes("jobs").AddRequirements(new JobsAuthorizationRequirement()));
            })
            .AddSingleton<IAuthorizationHandler, JobsAuthorizationHandler>();
            services.AddEasyWeChat();
            //mvc
            services.AddControllersWithViews(options =>
            {
                //for logs
                options.Filters.Add<Filters.StatisticalAttribute>(-10000);
                //for 权限
                if (Configuration.GetValue<bool>($"{nameof(AppSettings)}:{nameof(AppSettings.IsQxFilterOpened)}"))
                {
                    options.Filters.Add(new TypeFilterAttribute(typeof(iSchool.Authorization.AuthorizeAttribute)) { Arguments = new object[] { true }, Order = -9999 });
                    //options.Filters.AddService<iSchool.Authorization.AuthorizeAttribute>(-9999);
                    //options.Filters.Add(new ServiceFilterAttribute(typeof(iSchool.Authorization.AuthorizeAttribute)) { Order = -9999 });
                }
                //...
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new FnResultJsonConverter());
                options.SerializerSettings.Converters.Add(new SchFType0JsonConverter());
            })
            .ConfigureApiBehaviorOptions(options => 
            {
                options.InvalidModelStateResponseFactory = Middlewares.StatisticalLogsMiddleware.InvalidModelStateResponse;
            })
            .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "iSchool.API", Version = "v1" });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime)
        {
            applicationLifetime.ApplicationStarted.Register(o =>
            {
                var t = ((Tuple<Startup, IServiceProvider>)o);
                t.Item1.OnApplicationStarted(t.Item2);
            }, Tuple.Create(this, app.ApplicationServices), false);
            applicationLifetime.ApplicationStopping.Register(o =>
            {
                var t = ((Tuple<Startup, IServiceProvider>)o);
                t.Item1.OnApplicationStopping(t.Item2);
            }, Tuple.Create(this, app.ApplicationServices), false);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //使用memcached
            app.UseEnyimMemcached();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMiddleware<Middlewares.StatisticalLogsMiddleware>();

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                //page=>'/swagger/index.html'
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "iSchool.API v1");
            });

            //app.Run(async (context) =>
            //{
            //    context.Response.StatusCode = 404;
            //    await context.Response.WriteAsync("Page not found 404");
            //});
        }

        /// <summary>
        /// autofac 依赖注入
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            //将模块添加到容器中
            builder.RegisterModule(new InfrastructureModule(Configuration.GetConnectionString("SqlServerConnection")));
            builder.RegisterModule(new DomainModule());
            builder.RegisterModule(new MediatorModule());
        }

        /// <summary>
        /// on app start
        /// </summary>
        /// <param name="sp"></param>
        private void OnApplicationStarted(IServiceProvider sp)
        {
            RedisHelper.Initialization(sp.GetRequiredService<CSRedis.CSRedisClient>());
            var serviceScopeFactory = sp.GetService<IServiceScopeFactory>();
            SimpleQueue_Extension.ServiceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// on app stop
        /// </summary>
        /// <param name="sp"></param>
        private void OnApplicationStopping(IServiceProvider sp)
        {
            sp.GetService<CSRedis.CSRedisClient>()?.Dispose();
        }
    }
}
