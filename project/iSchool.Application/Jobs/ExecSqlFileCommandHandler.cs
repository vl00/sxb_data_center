using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
    public class ExecSqlFileCommandHandler : IRequestHandler<ExecSqlFileCommand>
    {
        public ExecSqlFileCommandHandler(IServiceProvider sp, ILog log, AppSettings appSettings,
            IUnitOfWork unitOfWork)
        {
            this.sp = sp;
            this.log = log;
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.appSettings = appSettings;
        }

        AppSettings appSettings;
        IServiceProvider sp;
        ILog log;
        UnitOfWork unitOfWork;

        public async Task<Unit> Handle(ExecSqlFileCommand req, CancellationToken cancellationToken)
        {
            var sql = await File.ReadAllTextAsync(Path.Combine("App_Data/sql", Regex.Replace(req.Fname, @"(\.sql)$", "") + ".sql"));
            if (req.Args != null) sql = string.Format(sql, req.Args.Select(_ => (object)_).ToArray());
            try
            {
                if (req.UseTran) unitOfWork.BeginTransaction();
                await unitOfWork.DbConnection.ExecuteAsync(sql, null, req.UseTran ? unitOfWork.DbTransaction : null);
                if (req.UseTran) unitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                if (req.UseTran) try { unitOfWork.Rollback(); } catch { }
                log.Error(ex);
                throw ex;
            }
            return Unit.Value;
        }

    }
}
