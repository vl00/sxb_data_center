using iSchool.Domain.Repository.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Application.Service
{
    public class DelRankSchoolCommandHandler : IRequestHandler<DelRankSchoolCommand, int>
    {
        public IRankReposiory _rankReposiory;

        public DelRankSchoolCommandHandler(IRankReposiory rankReposiory)
        {
            _rankReposiory = rankReposiory;
        }

        public Task<int> Handle(DelRankSchoolCommand request, CancellationToken cancellationToken)
        {
            var result = _rankReposiory.DelRank(request.Sid, request.RankId);
            return Task.FromResult(result);
        }
    }
}
