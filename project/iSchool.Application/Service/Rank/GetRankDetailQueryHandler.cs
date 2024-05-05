using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using iSchool.Infrastructure.Cache;

namespace iSchool.Application.Service.Rank
{
    public class GetRankDetailQueryHandler : IRequestHandler<GetRankDetailQuery, List<RankDetailDto>>
    {
        private IRepository<RankingList> _rankingListRepository;
        private IRepository<RankName> _ranknameRepository;
        private CacheManager _cacheManager;
        public UnitOfWork UnitOfWork { get; set; }


        public GetRankDetailQueryHandler(IRepository<RankingList> rankingListRepository,
            CacheManager cacheManager, IRepository<RankName> ranknameRepository,
            IUnitOfWork IUnitOfWork)
        {
            UnitOfWork = (UnitOfWork)IUnitOfWork;
            _ranknameRepository = ranknameRepository;
            _rankingListRepository = rankingListRepository;
            _cacheManager = cacheManager;
        }

        public async Task<List<RankDetailDto>> Handle(GetRankDetailQuery request, CancellationToken cancellationToken)
        {
            try
            {

                if (!request.IsCache)
                {
                    //判断缓存中是否存在数据
                    IEnumerable<RankDetailDto> details = await GetRankDetail(request);
                    //保存到缓存中
                    return details.AsList();
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {

                throw;
            }


        }

        private async Task<IEnumerable<RankDetailDto>> GetRankDetail(GetRankDetailQuery request)
        {
            var rank = _ranknameRepository.GetIsValid<Guid>(request.Id);
            string sql;
            if (rank.IsCollege)
            {

                sql = @"SELECT RankingList.Id,dbo.RankingList.SchoolId,dbo.College.name AS SchoolName,
                Placing,dbo.RankingList.CreateTime
                 FROM dbo.RankingList LEFT JOIN dbo.College
                ON dbo.RankingList.SchoolId = dbo.College.id
                WHERE RankNameId = @Id AND dbo.RankingList.IsValid=1 ORDER BY RankingList.Placing ,dbo.RankingList.ModifyDateTime";
            }
            else
            {
                sql = @"SELECT RankingList.Id,dbo.RankingList.SchoolId,dbo.School.name AS SchoolName,
                Placing,dbo.RankingList.CreateTime
                 FROM dbo.RankingList LEFT JOIN dbo.School
                ON dbo.RankingList.SchoolId = dbo.School.id
                WHERE RankNameId = @Id AND dbo.RankingList.IsValid=1 ORDER BY RankingList.Placing ,dbo.RankingList.ModifyDateTime";
            }
            var details = await UnitOfWork.DbConnection.QueryAsync<RankDetailDto>(sql,
                new { Id = request.Id },
                UnitOfWork.DbTransaction);
            return details;
        }
    }
}
