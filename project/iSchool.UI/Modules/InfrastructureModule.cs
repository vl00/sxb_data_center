using Autofac;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Cache;
using iSchool.Infrastructure.Repositories;
using iSchool.Infrastructure.Repositories.Organization;
using iSchool.Organization.Appliaction.Queries;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.UI.Modules
{
    /// <summary>
    /// 基础设施模块
    /// 4-Infrastructure层即为基础设施层
    /// </summary>
    public class InfrastructureModule : Module
    {
        private readonly string _databaseConnectionString;
        public InfrastructureModule(string databaseConnectionString)
        {
            this._databaseConnectionString = databaseConnectionString;
        }
        protected override void Load(ContainerBuilder builder)
        {
            //基础仓储的接口与实现类的映射
            builder.RegisterGeneric(typeof(BaseRepository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();

            //优惠券部分仓储基础设施
            builder.RegisterType(typeof(GoodsQueries))
             .As(typeof(IGoodsQueries))
             .InstancePerLifetimeScope();
            builder.RegisterType(typeof(CouponQueries))
             .As(typeof(ICouponQueries))
             .InstancePerLifetimeScope();
            builder.RegisterType(typeof(CouponReceiveRepository))
             .As(typeof(ICouponReceiveRepository))
             .InstancePerLifetimeScope();
            builder.RegisterType(typeof(CouponInfoRepository))
             .As(typeof(ICouponInfoRepository))
             .InstancePerLifetimeScope();


            builder.RegisterType(typeof(OrderQueries))
             .As(typeof(IOrderQueries))
             .InstancePerLifetimeScope();

            //注册要通过反射创建的组件。
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>()
                .WithParameter("connectionString", _databaseConnectionString)
                .InstancePerLifetimeScope();
        }
    }


    /// <summary>
    /// 机构-基础设施模块
    /// 4-Infrastructure层即为基础设施层
    /// </summary>
    public class OrgInfrastructureModule : Autofac.Module
    {
        private readonly string _orgDatabaseConnectionString;
        public OrgInfrastructureModule(string orgDatabaseConnectionString)
        {
            this._orgDatabaseConnectionString = orgDatabaseConnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterGeneric(typeof(OrgBaseRepository<>))
               .As(typeof(IBaseRepository<>))
               .Named("OrgBaseRepository", typeof(IBaseRepository<>))
               .InstancePerLifetimeScope();

            builder.RegisterType<OrgUnitOfWork>()
               .As<iSchool.Organization.Domain.IOrgUnitOfWork>()
               .WithParameter("connectionString", _orgDatabaseConnectionString)
               .InstancePerLifetimeScope();
        }
    }

    #region WX
    public class WXInfrastructureModule : Autofac.Module
    {
        private readonly string _databaseConnectionString;
        public WXInfrastructureModule(string databaseConnectionString)
        {
            this._databaseConnectionString = databaseConnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            //builder.RegisterGeneric(typeof(WXBaseRepository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();

            builder.RegisterType<WXOrgUnitOfWork>()
               .As<Organization.Domain.IWXUnitOfWork>()
               .WithParameter("connectionString", _databaseConnectionString)
               .InstancePerLifetimeScope();
        }
    }
    #endregion

    #region Openid_WX
    public class Openid_WXInfrastructureModule : Autofac.Module
    {
        private readonly string _databaseConnectionString;
        public Openid_WXInfrastructureModule(string databaseConnectionString)
        {
            this._databaseConnectionString = databaseConnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            //builder.RegisterGeneric(typeof(Openid_WXBaseRepository<>))
            //    .As(typeof(IBaseRepository<>))
            //    .Named("Openid_WXBaseRepository", typeof(IBaseRepository<>))
            //    .InstancePerLifetimeScope();

            builder.RegisterType<Openid_WXOrgUnitOfWork>()
               .As<IOpenid_WXUnitOfWork>()
               .WithParameter("connectionString", _databaseConnectionString)
               .InstancePerLifetimeScope();
        }
    }
    #endregion

    #region 虎叔叔用户中心
    public class UserInfrastructureModule : Autofac.Module
    {
        private readonly string _databaseConnectionString;
        public UserInfrastructureModule(string databaseConnectionString)
        {
            this._databaseConnectionString = databaseConnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            //builder.RegisterGeneric(typeof(UserBaseRepository<>)).As(typeof(IBaseRepository<>))
            //    .Named("UserBaseRepository", typeof(IBaseRepository<>))
            //    .InstancePerLifetimeScope();

            builder.RegisterType<UserUnitOfWork>().As(typeof(IUserUnitOfWork))
                .WithParameter("connectionString", _databaseConnectionString)
                .InstancePerLifetimeScope();
        }
    }
    #endregion
}
