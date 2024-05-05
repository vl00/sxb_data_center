using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using Dapper;
using System.Linq;

namespace iSchool.Application.Service
{
    public class AddGeneralTagsCommandHandler : IRequestHandler<AddGeneralTagsCommand, List<GeneralTag>>
    {
        UnitOfWork unitOfWork;
        IRepository<GeneralTag> generalTagRepository;

        public AddGeneralTagsCommandHandler(IRepository<GeneralTag> generalTagRepository, IUnitOfWork unitOfWork)
        {
            this.unitOfWork = (UnitOfWork)unitOfWork;
            this.generalTagRepository = generalTagRepository;
        }

        public async Task<List<GeneralTag>> Handle(AddGeneralTagsCommand req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var dy = new DynamicParameters();
            var sb = new StringBuilder(@"
declare @c int; 
set @c=0;
");
            var ids = new Guid[req.Names?.Length ?? 0];
            for (var i = 0; i < ids.Length; i++)
            {
                if (string.IsNullOrEmpty(req.Names[i])) continue;

                sb.Append($@"
if not exists(select 1 from dbo.GeneralTag where name=@Name{i}) begin
    insert dbo.GeneralTag(Id,Name) values(@Id{i},@Name{i});
    set @c=@c+1;
end
");
                ids[i] = Guid.NewGuid();
                dy.Add("Id" + i, ids[i]);
                dy.Add("Name" + i, req.Names[i]);
            }
            sb.Append("select @c;");

            try
            {
                //unitOfWork.BeginTransaction();

                var c = unitOfWork.DbConnection.ExecuteScalar<int>(sb.ToString(), dy, unitOfWork.DbTransaction);
                //unitOfWork.CommitChanges();

                if (c <= 0) return new List<GeneralTag>();
            }
            catch (Exception ex)
            {
                //unitOfWork.Rollback();
                throw ex;
            }

            var sql = @"select * from dbo.GeneralTag where Id in @ids";
            var tags = unitOfWork.DbConnection.Query<GeneralTag>(sql, new { ids });
            return tags.AsList();
        }
    }
}
