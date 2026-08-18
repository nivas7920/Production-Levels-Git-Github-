using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

using OnlineShoppingApplication.Controllers;
using OnlineShoppingApplication.Data;
using OnlineShoppingApplication.Models;
using OnlineShoppingApplication.Services;

namespace OnlineShoppingApplication.Tests.Controllers
{
    public class AuthControllerTests
    {
        // Create a new InMemory database for each test
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        // =========================================================
        // Test Case 1
        // Register with Valid User
        // Expected Result : OK (200)
        // =========================================================
        [Fact]
        public async Task Register_ValidUser_ReturnsOk()
        {
            // Arrange

            var context = GetDbContext();

            var tokenServiceMock = new Mock<ITokenService>();

            var controller = new AuthController(
                context,
                tokenServiceMock.Object);

            var request = new RegisterRequest
            {
                Name = "Nivas",
                Email = "nivas@test.com",
                Password = "Nivas@123"
            };

            // Act

            var result = await controller.Register(request);

            // Assert

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Single(context.Users);

            var savedUser = context.Users.First();

            Assert.Equal(request.Name, savedUser.Name);
            Assert.Equal(request.Email, savedUser.Email);

            // Password should be hashed
            Assert.NotNull(savedUser.PasswordHash);
            Assert.NotEqual(request.Password, savedUser.PasswordHash);
        }

        // =========================================================
        // Test Case 2
        // Register with Duplicate Email
        // Expected Result : BadRequest (400)
        // =========================================================
        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            // Arrange

            var context = GetDbContext();

            var tokenServiceMock = new Mock<ITokenService>();

            var controller = new AuthController(
                context,
                tokenServiceMock.Object);

            var passwordHasher = new PasswordHasher<User>();

            // Add Existing User
            var existingUser = new User
            {
                Name = "Nivas",
                Email = "nivas@test.com"
            };

            existingUser.PasswordHash =
                passwordHasher.HashPassword(existingUser, "Nivas@123");

            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            // Duplicate Register Request
            var request = new RegisterRequest
            {
                Name = "Nivas",
                Email = "nivas@test.com",
                Password = "AnotherPassword@123"
            };

            // Act

            var result = await controller.Register(request);

            // Assert

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // Only one user should exist
            Assert.Single(context.Users);

            // Verify the returned result is not null
            Assert.NotNull(badRequestResult);
        }


        // =========================================================
        // Test Case 3
        // Login with Valid Credentials
        // Expected Result : OK (200)
        // =========================================================
        [Fact]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            // Arrange

            var context = GetDbContext();

            var tokenServiceMock = new Mock<ITokenService>();

            // Mock JWT Token
            tokenServiceMock
                .Setup(x => x.CreateToken(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns("FakeJwtToken");

            var controller = new AuthController(
                context,
                tokenServiceMock.Object);

            var passwordHasher = new PasswordHasher<User>();

            // Create Existing User
            var user = new User
            {
                Name = "Nivas",
                Email = "nivas@gmail.com"
            };

            user.PasswordHash = passwordHasher.HashPassword(user, "Nivas@123");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Login Request
            var request = new LoginRequest
            {
                Email = "nivas@gmail.com",
                Password = "Nivas@123"
            };

            // Act

            var result = await controller.Login(request);

            // Assert

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult);

            // Verify JWT Token Method Called Once
            tokenServiceMock.Verify(x =>
                x.CreateToken(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }

        // =========================================================
        // Test Case 4
        // Login with Invalid Password
        // Expected Result : Unauthorized (401)
        // =========================================================
        [Fact]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            // Arrange

            var context = GetDbContext();

            var tokenServiceMock = new Mock<ITokenService>();

            var controller = new AuthController(
                context,
                tokenServiceMock.Object);

            var passwordHasher = new PasswordHasher<User>();

            // Create Existing User
            var user = new User
            {
                Name = "Nivas",
                Email = "nivas@test.com"
            };

            // Store the correct password
            user.PasswordHash = passwordHasher.HashPassword(user, "Nivas@123");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Login Request with WRONG Password
            var request = new LoginRequest
            {
                Email = "nivas@test.com",
                Password = "WrongPassword"
            };

            // Act

            var result = await controller.Login(request);

            // Assert

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);

            Assert.NotNull(unauthorizedResult);

            // Verify JWT Token was NEVER generated
            tokenServiceMock.Verify(
                x => x.CreateToken(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }


    }
}