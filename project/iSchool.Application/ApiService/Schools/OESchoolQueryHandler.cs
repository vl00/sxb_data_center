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
    public class OESchoolQueryHandler : IRequestHandler<OESchoolQuery, OESchoolQueryResult[]>
    {
        UnitOfWork unitOfWork;

        public OESchoolQueryHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task<OESchoolQueryResult[]> Handle(OESchoolQuery req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var sql = @"
select e.Id,(s.Name+'-'+(case when isnull(e.name,'')='' then '校本部' else e.name end))as Name,s.name_e as EName,s.Website,s.Intro,s.Logo,s.Status,
(case when e.IsValid=0 or e.IsValid=0 then 0 else 1 end)as IsValid
from OnlineSchoolExtension e
inner join OnlineSchool s on e.sid=s.id
where 1=1 and e.Id in @Ids
";

            var sgs = unitOfWork.DbConnection.Query<OESchoolQueryResult>(sql, new { req.Ids });

            return sgs.ToArray();
        }
    }
}
