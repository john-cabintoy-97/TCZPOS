using FluentValidation;
using TCZPOS.Components.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Validation
{
    public partial class VatValidator : AbstractValidator<VatModels>
    {
        public VatValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tax classification name is required (e.g., VAT 12%).");

            RuleFor(x => x.Percentage)
                .InclusiveBetween(0, 100).WithMessage("VAT percentage must be between 0 and 100.");
        }
    }
}
