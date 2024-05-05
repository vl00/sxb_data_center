using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Application.Service
{
    public class DelRankNameCommandHandler : IRequestHandler<DelRankNameCommand>
    {
        private IRepository<RankName> _ranknameRepository;

        public DelRankNameCommandHandler(IRepository<RankName> ranknameRepository)
        {
            _ranknameRepository = ranknameRepository;
        }

        public async Task<Unit> Handle(DelRankNameCommand request, CancellationToken cancellationToken)
        {
            await _ranknameRepository.DelectAsync(request.RankId);
            return Unit.Value;
        }
    }
}
