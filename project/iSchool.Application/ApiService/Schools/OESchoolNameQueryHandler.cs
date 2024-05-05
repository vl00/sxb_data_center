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

namespace iSchool.Application.ApiService.Schools
{
    public class OESchoolNameQueryHandler : IRequestHandler<OESchoolNameQuery, OESchoolQueryResult[]>
    {
        UnitOfWork unitOfWork;

        public OESchoolNameQueryHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task<OESchoolQueryResult[]> Handle(OESchoolNameQuery req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var sql = $@"
select top {req.Count} e.Id,(s.Name+'-'+(case when isnull(e.name,'')='' then '校本部' else e.name end))as Name
from OnlineSchoolExtension e
inner join OnlineSchool s on e.sid=s.id
where e.IsValid=1 and s.IsValid=1 and s.Name like @Name {"and s.status=@Status".If(req.Status != null)}
";

            var sgs = unitOfWork.DbConnection.Query<OESchoolQueryResult>(sql, new
            {
                Name = $"%{(req.Name?.Trim() ?? "")}%",
                req.Status
            });

            return sgs.ToArray();
        }
    }
}
