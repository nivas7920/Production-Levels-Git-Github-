namespace OnlineShoppingApplication.Services
{
    public interface ITokenService
    {
        string CreateToken(string userId, string name, string email);
    }
}
