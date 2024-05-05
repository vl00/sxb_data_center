using CSRedis;
using Dapper;
using iSchool.Domain;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
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
    public class AddTag2CgyCommandHandler : IRequestHandler<AddTag2CgyCommand, FnResult<int>>
    {        
        private UnitOfWork unitOfWork;
        private CSRedisClient redis;

        public AddTag2CgyCommandHandler(IUnitOfWork unitOfWork, CSRedisClient redis)
        {            
            this.unitOfWork = (UnitOfWork)unitOfWork;
            this.redis = redis;
        }

        public async Task<FnResult<int>> Handle(AddTag2CgyCommand cmd, CancellationToken cancellationToken)
        {
            if (cmd.Name.In("全部", "其他", "其它"))
            {
                return FnResult.Fail<int>($"不能添加名称为'{cmd.Name}'的分类");
            }

            var sql = @"select count(1) from [TagType] where parentid=@Tag1Cgy and name=@Name";
            var c = await unitOfWork.DbConnection.ExecuteScalarAsync<int>(sql, cmd);
            if (c > 0)
            {
                return FnResult.Fail<int>("已存在相同名称的二级分类");
            }

            sql = @"
declare @id int ; 
set @id=(select top 1 id from [TagType] where parentid=@Tag1Cgy and IsValid=0);
if @id is not null begin
	update [TagType] set IsValid=1,name=@Name,ModifyDateTime=getdate(),Modifier=@UserId where id=@id ;
end else if (select count(1) from [TagType] where parentid=@Tag1Cgy)<=30 begin
	insert [TagType](name,parentid,sort,CreateTime,Creator,ModifyDateTime,Modifier,IsValid)
	select @Name,@Tag1Cgy,1,getdate(),@UserId,getdate(),@UserId,1 
    ;
    set @id=@@identity ;
end else begin
    set @id=null ;
end
select @id
";
            var id = await unitOfWork.DbConnection.ExecuteScalarAsync<int?>(sql, cmd);
            if (id == null)
            {
                return FnResult.Fail<int>("最多30个二级分类");
            }
            await redis.DelAsync($"luru:tagcgy:{cmd.Tag1Cgy}");

            return FnResult.OK(id.Value);
        }
    }
}
