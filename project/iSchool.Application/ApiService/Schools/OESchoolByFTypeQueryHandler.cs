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
    public class OESchoolByFTypeQueryHandler : IRequestHandler<OESchoolByFTypeQuery, OESchoolByFTypeQueryResult[]>
    {
        UnitOfWork unitOfWork;

        public OESchoolByFTypeQueryHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task<OESchoolByFTypeQueryResult[]> Handle(OESchoolByFTypeQuery req, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var sql = $@"
select top {req.Top} e.Id,(s.Name+'-'+(case when isnull(e.name,'')='' then '校本部' else e.name end))as Name
from OnlineSchoolExtension e
inner join OnlineSchoolExtContent ec on e.id=ec.eid and ec.IsValid=1
inner join OnlineSchool s on e.sid=s.id and s.IsValid=1
where e.IsValid=1 {{0}}
";
            var sb = new StringBuilder();

            if (!req.SchoolName.IsNullOrEmpty())
            {
                sb.Append(" and s.name like @SchoolName");
            }
            if ((req.SchoolType?.Length ?? 0) > 0)
            {
                sb.Append(" and (1=0");
                foreach (var e in req.SchoolType)
                {
                    sb.AppendLine($" or (e.grade={e.Grade} and e.type={e.RunType} and isnull(e.discount,0)={(e.Discount == true ? 1 : 0)} and isnull(e.Diglossia,0)={(e.Diglossia == true ? 1 : 0)} and isnull(e.chinese,0)={(e.Chinese == true ? 1 : 0)})");
                }
                sb.Append(" )");
            }
            if ((req.CityCodes?.Length ?? 0) > 0)
            {
                sb.Append(" and (1=0");
                foreach (var c in req.CityCodes)
                {
                    sb.AppendLine($" or ec.province={c} or ec.city={c} or ec.area={c}");
                }
                sb.Append(" )");
            }

            sql = string.Format(sql, sb);

            var sgs = unitOfWork.DbConnection.Query<OESchoolByFTypeQueryResult>(sql, new DynamicParameters()
                .Set("SchoolName", "%" + req.SchoolName + "%")
            );

            return sgs.ToArray();
        }
    }
}
