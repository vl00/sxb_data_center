using Dapper;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Application.Service
{
    /// <summary>
    /// 获取年份字段内容
    /// </summary>
    public class GetSchoolYearFieldContentQueryHandler : IRequestHandler<GetSchoolYearFieldContentQuery, Dictionary<string, string>>
    {     
        private UnitOfWork UnitOfWork { get; set; }
        CSRedis.CSRedisClient redis;
        /// <summary>
        /// 构造函数注入
        /// </summary>
        public GetSchoolYearFieldContentQueryHandler(IUnitOfWork unitOfWork, CSRedis.CSRedisClient redis) 
        {
            UnitOfWork = (UnitOfWork)unitOfWork;
            this.redis = redis;
        }

        public Task<Dictionary<string, string>> Handle(GetSchoolYearFieldContentQuery request, CancellationToken cancellationToken)
        {
            
            var strFields = "'" + request.FieldName.Replace("|", "','") + "'";
            string sql = string.Format(@"select * from SchoolYearFieldContent_{0}
                                         where isvalid=1
                                         and eid='{1}'                                         
                                         and field in ({2})
                                         and year={0}", request.Year, request.Eid.ToString(), strFields);
          
            var data= UnitOfWork.DbConnection.Query<SchoolYearFieldContent>(sql,null , UnitOfWork.DbTransaction);           
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (data != null)
            {
                foreach (var item in data)
                {
                    result.Add(item.Field, item.Content);
                }
            }
            return Task.FromResult(result);
        }
    }
}
