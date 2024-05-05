using AutoMapper;
using Dapper;
using iSchool.Application.SettingModel;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Common;
using MediatR;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Application.Service
{
    /// <summary>
    /// 获取多年份字段最新记录
    /// </summary>
    public class LatestYearFieldDataQueryHandler : IRequestHandler<LatestYearFieldDataQuery, List<SchoolYearFieldContentDto>>
    {

        private readonly IMapper _mapper;
        private UnitOfWork UnitOfWork { get; set; }

        public LatestYearFieldDataQueryHandler(IMapper mapper, IUnitOfWork unitOfWork) 
        {
            this._mapper = mapper;
            this.UnitOfWork = (UnitOfWork)unitOfWork;
            
        }
        public Task<List<SchoolYearFieldContentDto>> Handle(LatestYearFieldDataQuery request, CancellationToken cancellationToken)
        {
            var response = new List<SchoolYearFieldContentDto>();
            var eid = request.EId.ToString();
            string yearExtFieldSQL = string.Format(@"select field,max(years) years,latest from YearExtField where eid='{0}'
                                                        group by field,latest", eid);
            var listYearExtField = UnitOfWork.DbConnection.Query<YearExtField>(yearExtFieldSQL).ToList();

            if (listYearExtField != null && listYearExtField.Count > 0)
            {

                #region  组装sql      
                Dictionary<int, string> yearFields = new Dictionary<int, string>();
                StringBuilder lastSql = new StringBuilder();
                foreach (var item in listYearExtField)
                {
                    var latest = (int)item.Latest;
                    if (yearFields.ContainsKey(latest))
                    {
                        yearFields[latest] += ",'" + item.Field + "'";
                    }
                    else
                    {
                        yearFields.Add(latest, "'" + item.Field + "'");
                    }
                }
                foreach (var item in yearFields)
                {
                    string sql = string.Format(@"select year,field,content content 
                                                     from SchoolYearFieldContent_{0} 
                                                     where isvalid=1 
                                                     and eid='{1}' 
                                                     and field in ({2})  ", item.Key, eid, item.Value);
                    if (lastSql.Length > 0) lastSql.AppendLine(" union ");

                    lastSql.AppendLine(sql);
                }
                #endregion

                var listYearData = UnitOfWork.DbConnection.Query<SchoolYearFieldContent>(lastSql.ToString(), new { Eid = eid });
                var listYearDto = new List<SchoolYearFieldContentDto>();

                //获取 允许年份范围集合        
                var listAllowYears = TimeHelp.GetNewYearist();
                foreach (var yearData in listYearData)
                {
                    var ydto = _mapper.Map<SchoolYearFieldContentDto>(yearData);
                    ydto.Years = listYearExtField.First(p => p.Field == yearData.Field).Years.Split(",").ToList();
                     ;
                    ydto.NewOtherYears = listAllowYears.Except(ydto.Years).ToList();
                    listYearDto.Add(ydto);
                }
                response = listYearDto;
            }

            return Task.FromResult(response);
        }
    }
}
