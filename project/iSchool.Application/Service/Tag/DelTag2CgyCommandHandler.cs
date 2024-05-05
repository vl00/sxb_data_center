using CSRedis;
using Dapper;
using iSchool;
using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Application.Service
{
    public class DelTag2CgyCommandHandler : IRequestHandler<DelTag2CgyCommand>
    {        
        private UnitOfWork unitOfWork;
        private CSRedisClient redis;

        public DelTag2CgyCommandHandler(IUnitOfWork unitOfWork, CSRedisClient redis)
        {            
            this.unitOfWork = (UnitOfWork)unitOfWork;
            this.redis = redis;
        }

        public async Task<Unit> Handle(DelTag2CgyCommand cmd, CancellationToken cancellationToken)
        {            
            var sql = "update [TagType] set IsValid=0,Modifier=@UserId,ModifyDateTime=getdate() where Id in @Tag2CgyIds ;" +
                "update [Tag] set subdivision=null,Modifier=@UserId,ModifyDateTime=getdate() where subdivision in @Tag2CgyIds ;";

            Exception ex = null;
            try
            {
                unitOfWork.BeginTransaction();
                await unitOfWork.DbConnection.ExecuteAsync(sql, cmd, unitOfWork.DbTransaction);
                unitOfWork.CommitChanges();
            }
            catch (Exception ex0)
            {
                unitOfWork.Rollback();
                ex = ex0;
            }

            if (ex == null)
            {
                await redis.StartPipe()
                    .Del($"luru:tagcgy:{cmd.Tag1Cgy}")
                    .Del($"luru:taglist")
                    .Del($"luru:taglist:{cmd.Tag1Cgy}")
                    .EndPipeAsync();
            }
            else throw ex;

            return default;
        }
    }
}
