using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Application.Service
{
    public class DelExtensionCommand : IRequest<bool>
    {
        public DelExtensionCommand()
        {
        }

        public DelExtensionCommand(Guid[] extensionIds)
        {
            ExtensionIds = extensionIds;
        }

        public Guid[] ExtensionIds { get; set; }

        public Guid Sid { get; set; }
        public Guid UserId { get; set; }
        public bool IsAll { get; set; }
    }
}
