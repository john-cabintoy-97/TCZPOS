using FluentValidation;
using TCZPOS.Components.Models;

namespace TCZPOS.Components.Validation
{
    public  partial class CategoryValidator : AbstractValidator<CategoryModels>
    {
        public CategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MinimumLength(2).WithMessage("Category name must be at least 2 characters.")
                .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters.");

            RuleFor(x => x.IsActive)
                .NotNull().WithMessage("Selection for active status is required.");
        }
    }
}
