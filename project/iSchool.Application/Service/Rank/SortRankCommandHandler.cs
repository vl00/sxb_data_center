using System.Threading;
using System.Threading.Tasks;
using iSchool.Domain.Repository.Interfaces;
using MediatR;

namespace iSchool.Application.Service
{
    public class SortRankCommandHandler : IRequestHandler<SortRankCommand, int>
    {

        public IRankReposiory _rankReposiory;

        public SortRankCommandHandler(IRankReposiory rankReposiory)
        {
            _rankReposiory = rankReposiory;
        }

        public Task<int> Handle(SortRankCommand request, CancellationToken cancellationToken)
        {
            var result = _rankReposiory.SortRank(request.Sid, request.RankId, request.Placing, request.IsJux);
            return Task.FromResult(result);
        }
    }
}
