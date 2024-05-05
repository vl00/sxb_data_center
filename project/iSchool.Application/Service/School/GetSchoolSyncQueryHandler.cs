using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using MediatR;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Infrastructure;
using Dapper;
using System;

namespace iSchool.Application.Service
{

    /// <summary>
    /// 获取同步学校的数据
    /// </summary>
    public class GetSchoolSyncQueryHandler : IRequestHandler<GetSchoolSyncQuery, IDictionary<string, object>>
    {
        private readonly IRepository<SchoolExtension> _schoolExtensionRepository;
        private readonly IRepository<SchoolVideo> _schoolVideoRepository;
        private readonly IRepository<School> _schoolRepository;
        private readonly IRepository<SchoolExtQuality> _schoolExtQualityRepository;
        private readonly IRepository<SchoolExtContent> _schoolExtContentRepository;

        UnitOfWork unitOfWork;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schoolExtensionRepository"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="schoolVideoRepository"></param>
        /// <param name="schoolRepository"></param>
        /// <param name="schoolExtQualityRepository">师资力量及教学质量 videos字段存着校长风采视频url</param>
        public GetSchoolSyncQueryHandler(IRepository<SchoolExtension> schoolExtensionRepository,
            IUnitOfWork unitOfWork, IRepository<SchoolVideo> schoolVideoRepository, IRepository<School> schoolRepository
            ,IRepository<SchoolExtQuality> schoolExtQualityRepository
            ,IRepository<SchoolExtContent> schoolExtContentRepository)
        {
            this.unitOfWork = (UnitOfWork)unitOfWork;
            _schoolExtensionRepository = schoolExtensionRepository;
            _schoolVideoRepository = schoolVideoRepository;
            _schoolRepository = schoolRepository;
            _schoolExtQualityRepository = schoolExtQualityRepository;
            _schoolExtContentRepository = schoolExtContentRepository;
        }

        public Task<IDictionary<string, object>> Handle(GetSchoolSyncQuery request, CancellationToken cancellationToken)
        {
            IDictionary<string, object> result = null;

            //校验学校是否存在
            var ext = _schoolExtensionRepository.GetIsValid(p => p.Id == request.Eid);
            if (ext == null)
            {
                return Task.FromResult(result);
            }
            else
            {
                //根据json文件生成json查询学校
                result = new Dictionary<string, object>();
                //查询学校信息
                var school = _schoolRepository.GetIsValid(p => p.IsValid == true && p.Id == request.Sid);
                result.Add("schoolname", school.Name);
                result.Add("extname", ext.Name);
                result.Add("extnickname", ext.NickName);
                result.Add("schftype", ext.SchFtype);
                

                //查询学校视频
                var extVideoField = request.schoolExtFieldSyncConfigDto.Fields.Where(p => p.VideoType != null);
                foreach (var item in extVideoField)
                {
                    var urls = _schoolVideoRepository.GetAll( p => p.Eid == request.Eid && p.Type == item.VideoType.Value && p.IsValid == true ).Select(p =>  p.VideoUrl).ToList();
                    result.Add(item.Name, urls);

                }
                //查询校长风采视频

                
                var extDataField = request.schoolExtFieldSyncConfigDto.Fields.Where(p => p.VideoType == null).GroupBy(p => p.DBtable);


                foreach (var item in extDataField)
                {
                    //表名
                    var table = item.Key;
                    //字段名字
                    var fields = string.Join(',', item.SelectMany(p => p.DBfield).Select(p => p.ToLower()).Distinct());

                    var sql = $"select {fields} from {table} where eid=@eid and IsValid=1";
                    //查询学校内容
                    var queryResult = unitOfWork.DbConnection.QueryFirstOrDefault(sql, new { eid = request.Eid });

                    if (queryResult != null)
                    {
                        bool isLodgingExtern = false;
                        bool? lodging = null, _extern = null;

                        foreach (var query in (IDictionary<string, object>)queryResult)
                        {
                            if (query.Key.ToLower().Equals("lodging"))//是否寄宿
                            {
                                isLodgingExtern = true;
                                lodging =query.Value==null?null: BusinessHelper.StringToBollen(query.Value.ToString().ToLower());
                            }
                            if (query.Key.ToLower().Equals("sdextern"))//是否走读
                            {
                                isLodgingExtern = true;
                                _extern = query.Value == null ? null : BusinessHelper.StringToBollen(query.Value.ToString().ToLower());
                            }
                            if (query.Key.ToLower().Equals("videos"))
                            {
                                var queryValue = new List<string>();
                                try { queryValue = JsonSerializationHelper.JSONToObject<List<string>>(query.Value.ToString()); } catch { }

                                 result.Add(query.Key.ToLower(), queryValue);
                            }
                            else
                            {
                                result.Add(query.Key.ToLower(), query.Value);
                            }
                        }
                        if(isLodgingExtern)
                        {
                            result["lodging"] = BusinessHelper.SetLodgingSdExternSelectValue(lodging,_extern);                            
                        }
                    }
                    else
                    {
                        foreach (var field in item.SelectMany(p => p.DBfield))
                        {
                            result.Add(field.ToLower(), null);
                        }
                    }
                }
                return Task.FromResult(result);

            }

        }
    }
}
