 namespace OnlineShoppingApplication.Models
{
    /// <summary>
    /// Represents the purchase summary with itemised breakdown, subtotal, discount, and grand total.
    /// Discount is applied when subtotal is >= 5000.
    /// </summary>
    public class PurchaseSummary
    {
        public int CartId { get; set; }

        public List<CartItemSummary> Items { get; set; } = new List<CartItemSummary>();

        public decimal Subtotal { get; set; }

        public decimal DiscountAmount { get; set; }

        public int DiscountPercentage { get; set; }

        public decimal GrandTotal { get; set; }
    }

    /// <summary>
    /// Represents an individual item in the purchase summary.
    /// </summary>
    public class CartItemSummary
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal ItemTotal { get; set; }

        public int DiscountPercentage { get; set; }
    }
}
