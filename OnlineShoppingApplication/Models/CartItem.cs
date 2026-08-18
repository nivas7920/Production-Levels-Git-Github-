using System.Text.Json.Serialization;

namespace OnlineShoppingApplication.Models
{
    /// <summary>
    /// Represents a single item in the shopping cart.
    /// </summary>
    public class CartItem
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product? Product { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public int CartId { get; set; }

        [JsonIgnore]
        public Cart? Cart { get; set; }
    }
}
