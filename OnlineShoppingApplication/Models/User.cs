namespace OnlineShoppingApplication.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        // Store hashed password instead of plain password
        public string PasswordHash { get; set; } = string.Empty;
    }
}
