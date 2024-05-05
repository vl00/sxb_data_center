using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure.Cache;
using MediatR;

namespace iSchool.Application.Service
{
    public class GetSchoolExtMenuQueryHandler : IRequestHandler<GetSchoolExtMenuQuery, List<ExtMenuItem>>
    {
        private readonly ISchoolExtReposiory _schoolExtReposiory;
        private readonly CacheManager _cacheManager;
        private readonly IMediator _mediator;
        internal const string _key = @"meunlist_";


        public GetSchoolExtMenuQueryHandler(ISchoolExtReposiory schoolExtReposiory, CacheManager cacheManager, IMediator mediator)
        {
            _schoolExtReposiory = schoolExtReposiory;
            this._cacheManager = cacheManager;
            _mediator = mediator;
        }

        public Task<List<ExtMenuItem>> Handle(GetSchoolExtMenuQuery request, CancellationToken cancellationToken)
        {
            //重置缓存
            if (request.Reset)
            {
                _cacheManager.Remove(_key + request.ExtId);
                _  = _mediator.Send(new UpSchoolExtCompletionCommand { Eid = request.ExtId }).Result;
            }

            var menus = _cacheManager.Get<List<ExtMenuItem>>(_key + request.ExtId);
            if (menus == null)
            {
                menus = _schoolExtReposiory.GetMenuList(request.ExtId);
                _cacheManager.Add(_key + request.ExtId, menus);
            }
            return Task.FromResult(menus);
        }
    }
}
