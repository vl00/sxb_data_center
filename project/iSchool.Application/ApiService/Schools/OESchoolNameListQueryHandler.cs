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
using static iSchool.Infrastructure.ObjectHelper;

namespace iSchool.Application.ApiService.Schools
{
    public class OESchoolNameListQueryHandler : IRequestHandler<OESchoolNameListQuery, OESchoolQueryResult[]>
    {
        UnitOfWork unitOfWork;

        public OESchoolNameListQueryHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task<OESchoolQueryResult[]> Handle(OESchoolNameListQuery req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var ns = req.Names.Select(n => 
            {
                var _ = n.Split('-');
                return (sn: Tryv(() => _[0], ""), en: Tryv(() => _[1], ""));
            });

            var sql = $@"
select * into #sg from OnlineSchool where [name] in @sn and IsValid=1

select * from (
    select e.Id,(s.Name+'-'+(case when isnull(e.name,'')='' then '校本部' else e.name end))as Name
    from OnlineSchoolExtension e
    inner join #sg s on e.sid=s.id
    where e.IsValid=1 and s.IsValid=1 {"and s.status=@Status".If(req.Status != null)}
) T where name in @ns
";

            var sgs = unitOfWork.DbConnection.Query<OESchoolQueryResult>(sql, new DynamicParameters()
                .Set("sn", ns.Select(_ => _.sn).ToArray())
                .Set("ns", req.Names)
                .Set("Status", req.Status)
            );

            return sgs.ToArray();
        }
    }
}
