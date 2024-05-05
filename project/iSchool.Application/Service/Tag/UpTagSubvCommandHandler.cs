using CSRedis;
using Dapper;
using iSchool;
using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Application.Service
{
    public class UpTagSubvCommandHandler : IRequestHandler<UpTagSubvCommand>
    {        
        private UnitOfWork unitOfWork;
        private CSRedisClient redis;

        public UpTagSubvCommandHandler(IUnitOfWork unitOfWork, CSRedisClient redis)
        {            
            this.unitOfWork = (UnitOfWork)unitOfWork;
            this.redis = redis;
        }

        public async Task<Unit> Handle(UpTagSubvCommand cmd, CancellationToken cancellationToken)
        {            
            Exception ex = null;
            try
            {
                unitOfWork.BeginTransaction();
                foreach (var change in cmd.Changes)
                {
                    var sql = "update [Tag] set subdivision=@Subdivision,Modifier=@UserId,ModifyDateTime=getdate() where IsValid=1 " +
                        "and id=@TagId and type=@Type ;";

                    unitOfWork.DbConnection.Execute(sql, new { cmd.UserId, change.TagId, change.Type, change.Subdivision }, unitOfWork.DbTransaction);
                }
                unitOfWork.CommitChanges();
            }
            catch (Exception ex0)
            {
                unitOfWork.Rollback();
                ex = ex0;
            }

            if (ex == null)
            {
                var pipe = redis.StartPipe();
                pipe.Del($"luru:taglist");
                foreach (var t1 in cmd.Changes.Select(_ => _.Type).Distinct())
                {
                    pipe.Del($"luru:taglist:{t1}");
                }
                await pipe.EndPipeAsync();
            }
            else throw ex;

            return default;
        }
    }
}
