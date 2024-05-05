using Dapper;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Application.Service
{
    public class DeleteTagCommandHandler : IRequestHandler<DeleteTagCommand>
    {
        IMediator mediator;

        public UnitOfWork UnitOfWork { get; set; }

        public DeleteTagCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
        {
            UnitOfWork = (UnitOfWork)unitOfWork;
            this.mediator = mediator;
        }

        public async Task<Unit> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
        {
            var sql = @"Update Tag set IsValid=0 where id in @ids";
            await UnitOfWork.DbConnection.ExecuteAsync(sql, new { ids = request.Ids }, UnitOfWork.DbTransaction);

            _ = Task.Factory.StartNew(() => mediator.Send(new GetTagListQuery(false)));

            return Unit.Value;
        }
    }
}
