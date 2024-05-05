using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain.Repository.Interfaces;

namespace iSchool.Application.Service
{

    public class SearchGeneralTagsQueryHandler : IRequestHandler<SearchGeneralTagsQuery, List<string>>
    {
        private readonly ITagRepository _tagRepository;

        public SearchGeneralTagsQueryHandler(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public Task<List<string>> Handle(SearchGeneralTagsQuery request, CancellationToken cancellationToken)
        {
            var result= _tagRepository.SearchGeneralTag(request.Name, request.Top);
            return Task.FromResult(result);
        }
    }
}
