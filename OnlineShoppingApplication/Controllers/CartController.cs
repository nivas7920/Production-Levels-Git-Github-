using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShoppingApplication.Data;
using OnlineShoppingApplication.Models;
using Microsoft.AspNetCore.Authorization;
namespace OnlineShoppingApplication.Controllers
{
    /// <summary>
    /// Controller for shopping cart operations.
    /// Discount is applied automatically when subtotal >= 5000.
    /// </summary>
    [ApiController]

    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private const decimal DISCOUNT_THRESHOLD = 5000;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// Create a new cart.
      
        /// <returns>New cart details</returns>
        [HttpPost("create")]
        public async Task<ActionResult<Cart>> CreateCart()
        {
            try
            {
                var cart = new Cart
                {
                    SessionId = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCart), new { id = cart.Id }, cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating cart", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific cart with all its items.
        /// </summary>
        /// <param name="id">Cart ID</param>
        /// <returns>Cart details with items</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Cart>> GetCart(int id)
        {
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cart == null)
                {
                    return NotFound(new { message = $"Cart with ID {id} not found" });
                }

                return Ok(cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving cart", error = ex.Message });
            }
        }

        /// <summary>
        /// Add a product to the cart. Quantity is automatically adjusted based on stock.
        /// </summary>
        /// <param name="id">Cart ID</param>
        /// <param name="productId">Product ID to add</param>
        /// <param name="quantity">Quantity to add (default: 1)</param>
        /// <returns>Updated cart with the new item</returns>
        [HttpPost("{id}/add-item")]
        public async Task<ActionResult<Cart>> AddToCart(int id, [FromQuery] int productId, [FromQuery] int quantity = 1)
        {
            try
            {
                if (quantity <= 0)
                {
                    return BadRequest(new { message = "Quantity must be greater than 0" });
                }

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cart == null)
                {
                    return NotFound(new { message = $"Cart with ID {id} not found" });
                }

                var product = await _context.Products.FindAsync(productId);

                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {productId} not found" });
                }

                if (product.StockQuantity < quantity)
                {
                    return BadRequest(new { message = $"Insufficient stock. Available: {product.StockQuantity}, Requested: {quantity}" });
                }

                // Check if product already in cart
                var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

                if (existingItem != null)
                {
                    // Update quantity
                    if (existingItem.Quantity + quantity > product.StockQuantity)
                    {
                        return BadRequest(new { message = $"Cannot add {quantity} more items. Max available: {product.StockQuantity - existingItem.Quantity}" });
                    }
                    existingItem.Quantity += quantity;
                }
                else
                {
                    // Add new item to cart
                    var cartItem = new CartItem
                    {
                        CartId = id,
                        ProductId = productId,
                        Quantity = quantity,
                        Price = product.Price
                    };

                    _context.CartItems.Add(cartItem);
                }

                await _context.SaveChangesAsync();

                // Reload cart with updated items
                var updatedCart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.Id == id);

                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error adding item to cart", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove a product from the cart.
        /// </summary>
        /// <param name="id">Cart ID</param>
        /// <param name="productId">Product ID to remove</param>
        /// <returns>Updated cart</returns>
        [HttpDelete("{id}/remove-item")]
        public async Task<ActionResult<Cart>> RemoveFromCart(int id, [FromQuery] int productId)
        {
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cart == null)
                {
                    return NotFound(new { message = $"Cart with ID {id} not found" });
                }

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

                if (cartItem == null)
                {
                    return NotFound(new { message = $"Product with ID {productId} not found in cart" });
                }

                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                // Reload cart with updated items
                var updatedCart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.Id == id);

                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error removing item from cart", error = ex.Message });
            }
        }

        /// <summary>
        /// Update quantity of a product in the cart.
        /// </summary>
        /// <param name="id">Cart ID</param>
        /// <param name="productId">Product ID</param>
        /// <param name="quantity">New quantity</param>
        /// <returns>Updated cart</returns>
        [HttpPut("{id}/update-item")]
        public async Task<ActionResult<Cart>> UpdateCartItem(int id, [FromQuery] int productId, [FromQuery] int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    return BadRequest(new { message = "Quantity must be greater than 0" });
                }

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cart == null)
                {
                    return NotFound(new { message = $"Cart with ID {id} not found" });
                }

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

                if (cartItem == null)
                {
                    return NotFound(new { message = $"Product with ID {productId} not found in cart" });
                }

                var product = await _context.Products.FindAsync(productId);

                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {productId} not found" });
                }

                if (quantity > product.StockQuantity)
                {
                    return BadRequest(new { message = $"Insufficient stock. Available: {product.StockQuantity}" });
                }

                cartItem.Quantity = quantity;
                await _context.SaveChangesAsync();

                // Reload cart with updated items
                var updatedCart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.Id == id);

                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating cart item", error = ex.Message });
            }
        }

        /// <summary>
        /// Get the purchase summary for a cart with itemised breakdown, subtotal, discount, and grand total.
        /// Discount is applied when subtotal >= 5000.
        /// </summary>
        /// <param name="id">Cart ID</param>
        /// <returns>Purchase summary</returns>
        [HttpGet("{id}/summary")]
        public async Task<ActionResult<PurchaseSummary>> GetPurchaseSummary(int id)
        {
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cart == null)
                {
                    return NotFound(new { message = $"Cart with ID {id} not found" });
                }

                var summary = new PurchaseSummary
                {
                    CartId = id,
                    Items = new List<CartItemSummary>()
                };

                decimal subtotal = 0;

                foreach (var item in cart.CartItems)
                {
                    if (item.Product == null) continue;

                    decimal itemTotal = item.Quantity * item.Price;
                    subtotal += itemTotal;

                    summary.Items.Add(new CartItemSummary
                    {
                        ProductId = item.ProductId,
                        ProductName = item.Product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price,
                        ItemTotal = itemTotal,
                        DiscountPercentage = item.Product.Discount
                    });
                }

                summary.Subtotal = subtotal;

                // Apply discount if subtotal >= 5000
                // Use the highest discount from cart items
                if (subtotal >= DISCOUNT_THRESHOLD && cart.CartItems.Any())
                {
                    summary.DiscountPercentage = cart.CartItems
                        .Where(ci => ci.Product != null)
                        .Max(ci => ci.Product!.Discount);

                    summary.DiscountAmount = Math.Round(subtotal * summary.DiscountPercentage / 100, 2);
                }

                summary.GrandTotal = subtotal - summary.DiscountAmount;

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error calculating purchase summary", error = ex.Message });
            }
        }

        /// <summary>
        /// Clear all items from the cart.
        /// </summary>
        /// <param name="id">Cart ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}/clear")]
        public async Task<ActionResult<object>> ClearCart(int id)
        {
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cart == null)
                {
                    return NotFound(new { message = $"Cart with ID {id} not found" });
                }

                _context.CartItems.RemoveRange(cart.CartItems);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cart cleared successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error clearing cart", error = ex.Message });
            }
        }
    }
}
