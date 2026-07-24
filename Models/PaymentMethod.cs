namespace C__Project.Models
{
    public enum PaymentType
    {
        Checking,
        Credit,
        Cash,
        DigitalWallet
    }

    public class PaymentMethod
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public PaymentType Type { get; set; }
        
        // Only applicable for Credit/DigitalWallet types where a limit might exist
        public decimal? CreditLimit { get; set; }
        
        // E.g. "🏦", "💳", "💵", "📱"
        public string Icon { get; set; } = "💳";
        
        // Hex color for UI styling
        public string Color { get; set; } = "#94a3b8";

        // Navigation property
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
