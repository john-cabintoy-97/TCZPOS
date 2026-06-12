using FluentValidation;
using TCZPOS.Components.Models;

namespace TCZPOS.Components.Validation
{
    public class VendorValidator : AbstractValidator<VendorModels>
    {
        public VendorValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Vendor name is required.")
                .MinimumLength(3).WithMessage("Vendor name must be at least 3 characters.")
                .MaximumLength(100).WithMessage("Vendor name cannot exceed 100 characters.");

            RuleFor(x => x.IsActive)
                .NotNull().WithMessage("Status must be specified.");
        }
    }
}
