namespace TCZPOS.Components.DTOs
{
    public class CustomerCreditViewDTO
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;

        // From CustomerCredits Table
        public decimal CurrentBalance { get; set; }
        public decimal CreditLimit { get; set; }

        // --- ADD THESE NEW PROPERTIES ---
        public decimal TotalPurchased { get; set; } // Lifetime accumulated debt
        public decimal TotalPaid { get; set; }      // Lifetime accumulated payments
        // --------------------------------

        // Computed Property for UI styling
        public decimal RemainingCredit => CreditLimit - CurrentBalance;
        public string RiskStatus => CurrentBalance > (CreditLimit * 0.8m) ? "High" : "Normal";
    }
}