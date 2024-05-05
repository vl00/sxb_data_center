using iSchool.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class UpTagSubvCommand : IRequest
    {
        public Guid UserId { get; set; }
        public UpTagSubvChange[] Changes { get; set; }
    }

    public class UpTagSubvChange
    {
        public Guid TagId { get; set; }
        public int Type { get; set; }
        public int? Subdivision { get; set; }
    }
}
