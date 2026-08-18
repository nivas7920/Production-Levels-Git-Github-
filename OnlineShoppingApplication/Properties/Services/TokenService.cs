using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineShoppingApplication.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateToken(string userId, string name, string email)
        {
            // Read expiry time from appsettings.json
            var expiry = Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"]);

            // Debug information
            Console.WriteLine("==================================");
            Console.WriteLine("Expiry Minutes = " + expiry);
            Console.WriteLine("Current Time   = " + DateTime.Now);
            Console.WriteLine("Expires At     = " + DateTime.Now.AddMinutes(expiry));
            Console.WriteLine("==================================");

            // Create JWT Claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Name, name),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Secret Key
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            // Signing Credentials
            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            // Create JWT Token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(expiry),
                signingCredentials: credentials);

            // Debug token information
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Issued  : " + DateTime.Now);
            Console.WriteLine("Expires : " + token.ValidTo);
            Console.WriteLine("--------------------------------");

            // Return JWT Token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}