using FluentValidation;
using iSchool.Application.Service;
using iSchool.Application.Service.Audit;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.UI.Validators
{

    public class AuditCommandValidator : AbstractValidator<AuditCommand>
    {
        public AuditCommandValidator()
        {
            RuleFor(_ => _.Msg).Custom((_, ctx) => 
            {
                var cmd = ctx.InstanceToValidate as AuditCommand;
                if (!cmd.Fail && !string.IsNullOrEmpty(cmd.Msg))
                {
                    ctx.AddFailure(nameof(cmd.Msg), "审核意见有内容时，无法做发布操作 ！");
                }
            });
        }
    }
}
