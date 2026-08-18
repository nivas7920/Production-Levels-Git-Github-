using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

using OnlineShoppingApplication.Controllers;
using OnlineShoppingApplication.Data;
using OnlineShoppingApplication.Models;

namespace OnlineShoppingApplication.Tests.Controllers
{
    public class ProductsControllerTests
    {
        // Create InMemory Database
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        // ======================================================
        // Test Case 1
        // Get All Products
        // Expected Result : 200 OK with Product List
        // ======================================================
        [Fact]
        public async Task GetProducts_ReturnsOk_WithProductList()
        {
            // ===========================
            // Arrange
            // ===========================

            var context = GetDbContext();

            // Add Test Products

            context.Products.AddRange(

                new Product
                {
                    Name = "Laptop",
                    Description = "Gaming Laptop",
                    Price = 50000,
                    StockQuantity = 10,
                    Category = "Electronics",
                    Discount = 10
                },

                new Product
                {
                    Name = "Mouse",
                    Description = "Wireless Mouse",
                    Price = 500,
                    StockQuantity = 50,
                    Category = "Electronics",
                    Discount = 5
                });

            await context.SaveChangesAsync();

            var controller = new ProductsController(context);

            // ===========================
            // Act
            // ===========================

            var result = await controller.GetProducts();

            // ===========================
            // Assert
            // ===========================

            // Check Status Code

            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            // Check Returned Data

            var products = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);

            // Verify Product Count

            Assert.Equal(2, products.Count());

            // Verify Product Names

            Assert.Contains(products, p => p.Name == "Laptop");

            Assert.Contains(products, p => p.Name == "Mouse");
        }

        [Fact]
        public async Task CreateProduct_ValidProduct_ReturnsCreated()
        {
            // Arrange
            var context = GetDbContext();

            var controller = new ProductsController(context);

            var product = new Product

            {
                Name = "Laptop",
                Description = "Gaming Laptop",
                Price = 50000,
                StockQuantity = 10,
                Category = "Electronics",
                Discount = 10
            };

            // Act
            var result = await controller.CreateProduct(product);

            // Assert

            // Verify API returns 201 Created
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);

            // Verify returned product
            var createdProduct = Assert.IsType<Product>(createdResult.Value);

            // Verify data
            Assert.Equal("Laptop", createdProduct.Name);
            Assert.Equal(50000, createdProduct.Price);

            // Verify product saved in database
            Assert.Single(context.Products);
        }

        [Fact]
        public async Task GetProduct_ExistingId_ReturnsOk()
        {
            // Arrange
            var context = GetDbContext();

            var product = new Product
            {
                Name = "Laptop",
                Description = "Gaming Laptop",
                Price = 50000,
                StockQuantity = 10,
                Category = "Electronics",
                Discount = 10
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var controller = new ProductsController(context);

            // Act
            var result = await controller.GetProduct(product.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            var returnedProduct = Assert.IsType<Product>(okResult.Value);

            Assert.Equal(product.Id, returnedProduct.Id);
            Assert.Equal(product.Name, returnedProduct.Name);
        }


        [Fact]
        public async Task DeleteProduct_ExistingId_ReturnsOk()
        {
            // Arrange
            var context = GetDbContext();

            var product = new Product
            {
                Name = "Laptop",
                Description = "Gaming Laptop",
                Price = 50000,
                StockQuantity = 10,
                Category = "Electronics",
                Discount = 10
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var controller = new ProductsController(context);

            // Act
            var result = await controller.DeleteProduct(product.Id);

            // Assert

            // Verify API returns 200 OK
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Verify product is deleted from database
            Assert.Empty(context.Products);
        }

    }
}