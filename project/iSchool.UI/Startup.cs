using Autofac;
using iSchool.Application.SettingModel;
using iSchool.BgServices;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Cache;
using iSchool.Organization.Appliaction.IntegrationEvents.Abstractions;
using iSchool.Organization.Appliaction.Options;
using iSchool.Organization.Domain.Security.Settings;
using iSchool.UI.Extensions;
using iSchool.UI.Middlewares;
using iSchool.UI.Modules;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders;
using Microsoft.OpenApi.Models;
using ProductManagement.Framework.Cache.Redis;
using ProductManagement.Framework.Cache.Redis.Configuration;
using ProductManagement.Framework.Cache.RedisProfiler;
using Sxb.GenerateNo;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace iSchool.UI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;

            //_ = iSchool.Authorization.Lib.UtilConf.Configuration;

            LngLatLocation.Init_With_Dapper();

            JsonExtensions.Converters.Add(new FnResultJsonConverter());
            JsonExtensions.Converters.Add(new SchFType0JsonConverter());
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; set; }

        /// <summary>
        /// 【ConfigureServices】
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddHttpContextAccessor();

            // for DataProtection
            //services.AddDataProtection()
            //    .PersistKeysToFileSystem(new DirectoryInfo(@"123"));

            //AllowedYearRange
            var allowedYearRange = Configuration.GetSection("AllowedYearRange");
            services.Configure<AllowedYearRange>(allowedYearRange);

            //memory cache
            services.AddSingleton<IMemoryCache>((sp) =>
            {
                return new MemoryCache(Configuration.GetSection("memoryCache").Get<MemoryCacheOptions>() ?? new MemoryCacheOptions { });
            });

            services.Configure<WechatMessageTplSetting>(Configuration.GetSection("WechatMessageTplSetting"));
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
            services.AddScoped<Infras.Locks.ILock1Factory, Infras.Locks.CSRedisCoreLock1Factory>();
            //Redis
            var redisConfig = Configuration.GetSection("RedisConfig").Get<RedisConfig>();            
            services.AddSingleton<MyRedisProfiler>();
            services.AddSingletonCacheRedis(config =>
            {
                config.Database = redisConfig.Database;
                config.RedisConnect = redisConfig.RedisConnect;
                config.CloseRedis = redisConfig.CloseRedis;
                config.HaveLog = redisConfig.HaveLog;
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
            services.Configure<Organization.Appliaction.EvltCoverCreateOption>(option =>
            {
                Configuration.Bind("AppSettings:EvltCoverCreate", option, (config, o) =>
                {
                    o.FontColor = Color.FromName(config["fontColor"]);
                });
            });
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //售后管理服务配置。
            services.Configure<AftersalesOption>(Configuration.GetSection(AftersalesOption.AftersalesConfig));

            // audit
            services.AddSingleton(sp => new AuditOption());

            services.AddSingleton<Infrastructure.Logs.BufferLogsWatcher>();
            services.AddScoped<ILog, Logger>();

            // 机构后台调用机构api逻辑
            services.AddScoped<Organization.Domain.Security.IUserInfo, Organization.Domain.Security.UserInfo>();
            services.AddScoped<Organization.Appliaction.RequestModels.SmLogUserOperation>();

            // 用于生成订单号
            services.AddSingleton<ISxbGenerateNo, SxbGenerateNo>();


            //automapper
            services.AddAutoMapperSetup();

            //使用请求压缩 br gzip
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                new[] { "image/svg+xml" });

            });

            services.AddEasyWeChat();

            //authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                //throw new Exception($"test:{Directory.GetCurrentDirectory()}");
                options.LoginPath = Configuration["auth:cookie:loginPath"]; //"/home/login";
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = Configuration["auth:cookie:name"];
                options.Cookie.Domain = Configuration["auth:cookie:domain"];
                options.Cookie.Path = Configuration["auth:cookie:path"];
                options.DataProtectionProvider = DataProtectionProvider.Create(new DirectoryInfo(Configuration["auth:cookie:dataProtectionDir"]));
            });

            #region mvc

            //解决MVC视图中的中文被html编码的问题
            services.Configure<WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });

            //在现有项目中启用运行时编译
            services.AddRazorPages().AddRazorRuntimeCompilation();

            //mvc
            //services.AddTransient(_ => new iSchool.Authorization.AuthorizeAttribute(true));

            services.AddControllersWithViews(options =>
            {
                options.EnableEndpointRouting = false;
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
                options.SerializerSettings.Converters.Add(new SchFType0JsonConverter());
            })
            .SetCompatibilityVersion(CompatibilityVersion.Latest);

            #endregion

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyMethod();
                    builder.AllowAnyHeader();
                    builder.SetIsOriginAllowed(_ => true);
                    builder.AllowCredentials();
                });
            });

            if (Env.IsDevelopment())
            {
                //Swagger
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "iSchool.UI",
                        Version = "v1",
                        Description = "虎叔叔~~"
                    });
                    //添加中文注释                
                    var basePath = Path.GetDirectoryName(typeof(Startup).Assembly.ManifestModule.FullyQualifiedName);
                    var files = Directory.EnumerateFiles(basePath, "iSchool.*.xml");
                    foreach (var file in files)
                    {
                        c.IncludeXmlComments(file);
                    }                    
                    c.DocInclusionPredicate((docName, description) => true);
                });
                services.AddSwaggerGenNewtonsoftSupport();
            }

            #region cap
            services.AddCap(x =>
            {
                // 注册 Dashboard
                //x.UseDashboard();

                // 注册节点到 Consul
                //x.UseDiscovery(d =>
                //{
                //    d.DiscoveryServerHostName = "localhost";
                //    d.DiscoveryServerPort = 8500;
                //    d.CurrentNodeHostName = "james.sxkid.com";
                //    d.CurrentNodePort = 5001;
                //    d.NodeId = "Sxb.Settlement.API_1";
                //    d.NodeName = "CAP Sxb.Settlement.API No.1";
                //});
                //如果你使用的ADO.NET，根据数据库选择进行配置：
                x.UseSqlServer(Configuration.GetConnectionString("OrgSqlServerConnection"));

                //CAP支持 RabbitMQ、Kafka、AzureServiceBus、AmazonSQS 等作为MQ，根据使用选择配置：
                x.UseRabbitMQ(options =>
                {
                    Configuration.GetSection("RabbitMQ").Bind(options);
                });
            });
            services.AddIntegrationEvent();
            #endregion cap
        }

        /// <summary>
        /// 【Configure】
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="applicationLifetime"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime)
        {
            //ApplicationStarted当应用程序主机完全启动并准备等待时触发
            applicationLifetime.ApplicationStarted.Register(o =>
            {
                var t = ((Tuple<Startup, IServiceProvider>)o);
                t.Item1.OnApplicationStarted(t.Item2);
            }, Tuple.Create(this, app.ApplicationServices), false);
            //ApplicationStopping当应用程序主机执行适当的关闭时触发。请求可能仍在处理中。关闭将被阻塞，直到该事件完成。
            applicationLifetime.ApplicationStopping.Register(o =>
            {
                var t = ((Tuple<Startup, IServiceProvider>)o);
                t.Item1.OnApplicationStopping(t.Item2);
            }, Tuple.Create(this, app.ApplicationServices), false);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                    // /swagger
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "iSchool.UI v1");
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //使用请求压缩 br gzip
            app.UseResponseCompression();

            //使用memcached
            app.UseEnyimMemcached();

            //添加用于将HTTP请求重定向到HTTPS的中间件。
            app.UseHttpsRedirection();

            //为当前请求路径启用静态文件服务
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseCors();

            app.UseMiddleware<Middlewares.StatisticalLogsMiddleware>();
            app.UseMiddleware<iSchool.SmLogUserOperationMiddleware>();

            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Home}/{action=Index}/{id?}");
            //});

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
            builder.RegisterModule(new OrgInfrastructureModule(Configuration.GetConnectionString("OrgSqlServerConnection")));
            #region 测试环境随机评测用户使用
            builder.RegisterModule(new WXInfrastructureModule(Configuration.GetConnectionString("WXSqlServerConnection")));
            builder.RegisterModule(new Openid_WXInfrastructureModule(Configuration.GetConnectionString("Openid_WXSqlServerConnection")));
            #endregion
            builder.RegisterModule(new DomainModule());
            builder.RegisterModule(new MediatorModule());
            builder.RegisterModule(new UserInfrastructureModule(Configuration.GetConnectionString("UserSqlServerConnection")));

            //mongodb
            builder.RegisterModule(new MongodbModule(Configuration["mongodb:mongodbconnection"], Configuration["mongodb:mongodbdatabase"]));



        }

        /// <summary>
        /// on app start
        /// </summary>
        /// <param name="sp"></param>
        private void OnApplicationStarted(IServiceProvider sp)
        {
            //RedisHelper.Initialization(sp.GetRequiredService<CSRedis.CSRedisClient>());       
            var serviceScopeFactory = sp.GetService<IServiceScopeFactory>();
            AsyncUtils.SetServiceScopeFactory(serviceScopeFactory);
            SimpleQueue_Extension.ServiceScopeFactory = serviceScopeFactory;

            // for export xlsx
            if (Path.Combine(AppContext.BaseDirectory, Configuration["AppSettings:XlsxDir"]) is string xlsx_dir && !Directory.Exists(xlsx_dir))
                Directory.CreateDirectory(xlsx_dir);
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
