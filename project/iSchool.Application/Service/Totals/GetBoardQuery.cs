using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Domain;
using iSchool.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;

namespace iSchool.Application.Service.Totals
{
    public class GetBoardQuery : IRequest<TotalDataboard>
    {
    }
}
