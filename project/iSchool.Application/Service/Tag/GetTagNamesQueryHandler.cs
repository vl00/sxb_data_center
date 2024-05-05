using iSchool.Application.ViewModels;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Application.Service
{
    public class GetTagNamesQueryHandler : IRequestHandler<GetTagNamesQuery, TagItem[]>
    {
        IRepository<Tag> _tagRepository;

        public GetTagNamesQueryHandler(IRepository<Tag> tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public Task<TagItem[]> Handle(GetTagNamesQuery request, CancellationToken cancellationToken)
        {
            var tags = _tagRepository.GetAll(_ => _.IsValid == true)
                .Where(_ => request.Ids.Contains(_.Id))
                .ToArray()
                .Select(_ => new TagItem
                {
                    Id = _.Id,
                    Name = _.Name,
                    SpellCode = _.SpellCode,
                    CreateTime = _.CreateTime,
                })
                .ToArray();

            return Task.FromResult(tags);
        }
    }
}
