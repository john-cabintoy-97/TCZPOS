using FluentValidation;
using TCZPOS.Components.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCZPOS.Components.Validation
{
    public partial class StockInValidator : AbstractValidator<StockInModels>
    {
        public StockInValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Must select a product to stock in.");

            RuleFor(x => x.QuantityAdded)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            RuleFor(x => x.UnitCostAtTimeOfStock)
                .GreaterThanOrEqualTo(0).WithMessage("Unit cost cannot be negative.");

            RuleFor(x => x.ReferenceNumber)
                .NotEmpty().WithMessage("Invoice or Reference number is required for audit trails.");
        }
    }
}
