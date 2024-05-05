using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure.Cache;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Application.Service
{
    public class GetRankListQueryHandler : IRequestHandler<GetRankListQuery, List<RankNameDto>>
    {
        private IRepository<RankName> _ranknameRepository;
        private readonly CacheManager _manager;

        public GetRankListQueryHandler(IRepository<RankName> ranknameRepository, CacheManager manager)
        {
            _ranknameRepository = ranknameRepository;
            _manager = manager;
        }

        public Task<List<RankNameDto>> Handle(GetRankListQuery request, CancellationToken cancellationToken)
        {
            List<RankNameDto> dto = new List<RankNameDto>();
            if (request.IsCache)
            {
                //查询缓存
            }
            else
            {
                //从数据库中查询
                var rankNames = _ranknameRepository.GetAll(p => p.IsValid == true);
                var years = rankNames.Select(p => p.Year).Distinct().OrderByDescending(p => p);
                foreach (var year in years)
                {
                    var item = new RankNameDto();
                    item.Year = year;
                    item.RankNames = rankNames.Where(p => p.Year == year).Select(p =>
                        new RankNameItem
                        {
                            Id = p.Id,
                            Name = p.Name,
                            CreateDate = p.CreateTime
                        })
                    .OrderBy(p => p.CreateDate)
                    .ToList();
                    dto.Add(item);
                }
                //跟新缓存
            }
            return Task.FromResult(dto);
        }


    }
}
