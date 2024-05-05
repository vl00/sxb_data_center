using CSRedis;
using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Application.Service
{
    public class GetTag2CgysQueryHandler : IRequestHandler<GetTag2CgysQuery, List<Tag2Cgy>>
    {
        IRepository<TagType> tagTypeRepository;
        CSRedisClient redis;

        public GetTag2CgysQueryHandler(IRepository<TagType> tagTypeRepository, CSRedisClient redis)
        {
            this.tagTypeRepository = tagTypeRepository;
            this.redis = redis;
        }

        public async Task<List<Tag2Cgy>> Handle(GetTag2CgysQuery req, CancellationToken cancellationToken)
        {
            var ls = await redis.GetAsync<List<Tag2Cgy>>($"luru:tagcgy:{req.Tag1Cgy}");
            if (ls == null)
            {
                ls = tagTypeRepository.GetAll(_ => _.IsValid && _.Parentid == req.Tag1Cgy)
                    .Select(_ => new Tag2Cgy { Id = _.Id, Name = _.Name })
                    .ToList();

                await redis.SetAsync($"luru:tagcgy:{req.Tag1Cgy}", ls, 60 * 5);
            }
            ls.Insert(0, new Tag2Cgy { Id = 0, Name = "全部" });
            ls.Add(new Tag2Cgy { Name = "其他" });
            return ls;
        }        
    }
}
