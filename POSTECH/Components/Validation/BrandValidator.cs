using FluentValidation;
using TCZPOS.Components.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Validation
{
    public partial class BrandValidator : AbstractValidator<BrandModels>
    {
        public BrandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Brand name is required.")
                .MinimumLength(2).WithMessage("Brand name is too short.")
                .MaximumLength(100).WithMessage("Brand name is too long.");

            RuleFor(x => x.IsActive)
                .NotNull().WithMessage("Status must be defined.");
        }
    }
}
