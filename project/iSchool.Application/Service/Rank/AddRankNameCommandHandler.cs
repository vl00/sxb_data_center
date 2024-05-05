using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Application.Service
{
    /// <summary>
    ///添加排行榜名字
    /// </summary>
    public class AddRankNameCommandHandler : IRequestHandler<AddRankNameCommand, Guid>
    {
        private IRepository<RankName> _ranknameRepository;

        public AddRankNameCommandHandler(IRepository<RankName> ranknameRepository)
        {
            _ranknameRepository = ranknameRepository;
        }

        public Task<Guid> Handle(AddRankNameCommand request, CancellationToken cancellationToken)
        {
            var rankName = new RankName();
            rankName.Creator = Guid.Empty;
            rankName.Modifier = Guid.Empty;
            rankName.Year = request.Year;
            rankName.Name = request.Name;
            rankName.IsCollege = request.IsCollege;
            _ranknameRepository.Insert(rankName);
            return Task.FromResult(rankName.Id);
        }
    }
}
