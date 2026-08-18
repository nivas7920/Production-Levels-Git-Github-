namespace OnlineShoppingApplication.Models
{
    /// <summary>
    /// Represents a shopping cart containing cart items.
    /// </summary>
    public class Cart
    {
        public int Id { get; set; }

        public string SessionId { get; set; } = Guid.NewGuid().ToString();

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
