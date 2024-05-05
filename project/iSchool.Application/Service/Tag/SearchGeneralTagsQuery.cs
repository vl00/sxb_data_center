using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class SearchGeneralTagsQuery : IRequest<List<string>>
    {


        public SearchGeneralTagsQuery()
        {
        }

        public SearchGeneralTagsQuery(string name, int top)
        {
            Name = name;
            Top = top;
        }

        public string Name { get; set; }

        public int Top { get; set; }
    }
}
