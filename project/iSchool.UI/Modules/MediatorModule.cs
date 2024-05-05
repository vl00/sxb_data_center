using Autofac;
using FluentValidation;
using iSchool.Application.Service;
using iSchool.Organization.Appliaction.Service.Course;
using MediatR;
using MediatR.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace iSchool.UI.Modules
{
    /// <summary>
    /// 中介模块
    /// </summary>
    public class MediatorModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly).AsImplementedInterfaces();

            //中介操作类型
            var mediatrOpenTypes = new[]
            {
                typeof(IRequestHandler<,>),
                typeof(INotificationHandler<>),//MediatR
                typeof(IValidator<>),//Defines a validator for a particular type.
            };

            foreach (var mediatrOpenType in mediatrOpenTypes)
            {
                builder
                    .RegisterAssemblyTypes(typeof(AddRankNameCommand).GetTypeInfo().Assembly)
                    .AsClosedTypesOf(mediatrOpenType)
                    .AsImplementedInterfaces();

                builder
                      .RegisterAssemblyTypes(typeof(CoursesByInfoQuery).GetTypeInfo().Assembly)
                      .AsClosedTypesOf(mediatrOpenType)
                      .AsImplementedInterfaces();


            }

            //MeidatR支持按需配置请求管道进行消息处理。即支持在请求处理前和请求处理后添加额外行为。仅需实现以下两个接口，并注册到Ioc容器即可
            builder.RegisterGeneric(typeof(RequestPostProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));//请求处理后接口
            builder.RegisterGeneric(typeof(RequestPreProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));//请求处理前接口？？？？

            builder.Register<ServiceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });

            //builder.RegisterGeneric(typeof(CommandValidationBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        }
    }
}