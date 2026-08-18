namespace OnlineShoppingApplication.Models
{
    /// <summary>
    /// Represents a product in the online store.
    /// </summary>
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Discount { get; set; } // Discount percentage

        public string Category { get; set; } = string.Empty;

        public int StockQuantity { get; set; }
    }
}
