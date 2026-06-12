using FluentValidation;
using TCZPOS.Components.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Validation
{
    public partial class ProductValidator : AbstractValidator<ProductModels>
    {
        public ProductValidator()
        {
            RuleFor(x => x.Barcode)
                .NotEmpty().WithMessage("Barcode is required for scanning.")
                .MaximumLength(50);

            RuleFor(x => x.Pcode)
                .NotEmpty().WithMessage("Internal Product Code (Pcode) is required.")
                .MaximumLength(50);

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product description/name is required.")
                .MaximumLength(150);

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Please select a valid category.");

            RuleFor(x => x.CostPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Cost price cannot be negative.");

            RuleFor(x => x.SellingPrice)
                .GreaterThan(x => x.CostPrice).WithMessage("Selling price must be higher than cost price (Negative Markup).");

            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage("Unit of measure (e.g., pcs, kg) is required.");

            // Validation for perishables
            RuleFor(x => x.ExpiryDate)
                .NotNull().WithMessage("Expiry date is required for perishable items.")
                .When(x => x.HasExpiry);
        }
    }
}
