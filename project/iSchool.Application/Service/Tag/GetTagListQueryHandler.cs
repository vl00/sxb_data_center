using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSRedis;
using Microsoft.Extensions.Caching.Memory;
using iSchool.Infrastructure;

namespace iSchool.Application.Service
{
    public class GetTagListQueryHandler : IRequestHandler<GetTagListQuery, List<TagDto>>
    {
        CSRedisClient redis;
        Infrastructure.ILog log;
        IMemoryCache memoryCache;
        IRepository<Tag> _tagRepository;

        public GetTagListQueryHandler(IRepository<Tag> tagRepository, CSRedisClient redis, Infrastructure.ILog log, IMemoryCache memoryCache)
        {
            this._tagRepository = tagRepository;
            this.redis = redis;
            this.log = log;
            this.memoryCache = memoryCache;
        }

        Tag[] GetTags(GetTagListQuery req)
        {
            var k = req.TagType == null ? "luru:taglist" : $"luru:taglist:{req.TagType}";
            Tag[] ress = null;
            if (req.IsCache)
            {
                ress = redis.Get<Tag[]>(k);
                if (ress != null) return ress;
            }
            if (req.TagType == null) ress = _tagRepository.GetAll(p => p.IsValid == true).ToArray();
            else ress = _tagRepository.GetAll(p => p.IsValid == true && p.Type == req.TagType.Value).ToArray();
            _ = redis.SetAsync(k, ress, 60 * 15);
            return ress;
        }

        public Task<List<TagDto>> Handle(GetTagListQuery request, CancellationToken cancellationToken)
        {
            var tags = GetTags(request);

            var types = tags.Select(p => new { p.SpellCode, p.Type })
                .Distinct().OrderBy(p => p.SpellCode);

            List<TagDto> dto = new List<TagDto>();

            foreach (var item in types)
            {
                dto.Add(new TagDto
                {
                    SpellCode = item.SpellCode,
                    Type = item.Type,
                    Tags = tags
                    .Where(p => p.SpellCode == item.SpellCode && p.Type == item.Type)
                    .OrderBy(p => p.CreateTime)
                    .Select(p => new TagItem
                    {
                        SpellCode = item.SpellCode,
                        CreateTime = p.CreateTime,
                        Name = p.Name,
                        Id = p.Id,
                        Subdivision = p.Subdivision,
                    }).ToList()
                });

            }
            return Task.FromResult(dto);
        }        
    }
}
