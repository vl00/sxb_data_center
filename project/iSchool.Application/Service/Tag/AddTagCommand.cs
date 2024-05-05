using iSchool.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class AddTagCommand : IRequest<(Tag Tag, bool IsNew)>
    {
        public AddTagCommand(string name, byte type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; set; }

        public byte Type { get; set; }

        public Guid UserId { get; set; }
    }
}
