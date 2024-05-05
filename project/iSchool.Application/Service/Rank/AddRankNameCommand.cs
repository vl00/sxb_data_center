using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class AddRankNameCommand : IRequest<Guid>
    {
        public AddRankNameCommand(int year, string name, bool isCollege)
        {
            Year = year;
            Name = name;
            IsCollege = isCollege;
        }

        public int Year { get; set; }
        public string Name { get; set; }

        public bool IsCollege { get; set; }

    }
}
