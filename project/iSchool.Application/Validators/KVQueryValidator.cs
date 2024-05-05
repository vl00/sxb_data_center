using FluentValidation;
using iSchool.Application.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.UI.Validators
{

    public class KVQueryValidator : AbstractValidator<KVQuery>
    {
        public KVQueryValidator()
        {
            RuleFor(_ => _.ParentId)
                .GreaterThanOrEqualTo(0);

            RuleFor(_ => _.Type)
                .GreaterThan(0);
        }
    }
}
