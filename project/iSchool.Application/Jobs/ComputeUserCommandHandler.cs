using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Authorization;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using iSchool.Domain.Enum;
using iSchool.Domain.Modles;

namespace iSchool.Application.Service.Jobs
{
    public class ComputeUserCommandHandler : IRequestHandler<ComputeUserCommand>
    {
        public ComputeUserCommandHandler(IServiceProvider sp, ILog log, AppSettings appSettings,
            IUnitOfWork unitOfWork,
            IRepository<Total_User> resp_tuser)
        {
            this.sp = sp;
            this.log = log;
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.appSettings = appSettings;
            this.resp_tuser = resp_tuser;
        }

        AppSettings appSettings;
        IServiceProvider sp;
        ILog log;
        UnitOfWork unitOfWork;
        IRepository<Total_User> resp_tuser;

        Guid gidEditor => appSettings.GidEditor;
        Guid gidAuditor => appSettings.GidAuditor;
        Guid gidQxEdit => appSettings.GidQxEdit;
        Guid gidQxAudit => appSettings.GidQxAudit;

        public async Task<Unit> Handle(ComputeUserCommand req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            log.Info($"working 获取所有编辑+所有有审核权限的user");
            Get_Users(req);
            log.Info($"work ok 获取所有编辑+所有有审核权限的user");

            return Unit.Value;
        }

        private void Get_Users(ComputeUserCommand cmd)
        {
            var act = new Account();

            var editors = act.GetAdmins(gidEditor, Account.IDType.CharacterID).Select(_ => new Total_User
            {
                Id = _.Id,
                Account = _.Name,
                Username = _.Displayname,
                Date = cmd.Date,
                RoleId = gidEditor,
            }).ToArray();

            var auditors = act.GetAdmins(gidQxAudit, Account.IDType.FunctionID).Select(_ => new Total_User
            {
                Id = _.Id,
                Account = _.Name,
                Username = _.Displayname,
                Date = cmd.Date,
                QxId = gidQxAudit,
            }).ToArray();

            try
            {
                unitOfWork.BeginTransaction();

                unitOfWork.DbConnection.Execute("delete from dbo.Total_User", null, unitOfWork.DbTransaction);

                foreach (var user in editors.Union(auditors))
                {
                    unitOfWork.DbConnection.Execute(@"
insert dbo.Total_User(Id,Account,Username,Date,RoleId,QxId) values(@Id,@Account,@Username,@Date,@RoleId,@QxId) ;
", user, unitOfWork.DbTransaction);
                }

                unitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();
                throw ex;
            }
        }
    }
}
