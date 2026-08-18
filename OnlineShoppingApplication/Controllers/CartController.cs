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

        /// <summary>
        /// Create a new cart.
        /// </summary>
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

                return CreatedAtAction(
                    nameof(GetCart),
                    new { id = cart.Id },
                    cart
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating cart: {ex}");

                return StatusCode(500, new
                {
                    message = "An error occurred while creating the cart."
                });
            }
        }

        /// <summary>
        /// Get a specific cart with all its items.
        /// </summary>
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
                    return NotFound(new
                    {
                        message = $"Cart with ID {id} not found"
                    });
                }

                return Ok(cart);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving cart: {ex}");

                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving the cart."
                });
            }
        }

        /// <summary>
        /// Add a product to the cart.
        /// Quantity is automatically adjusted based on stock.
        /// </summary>
        [HttpPost("{id}/add-item")]
        public async Task<ActionResult<Cart>> AddToCart(
            int id,
            [FromQuery] int productId,
            [FromQuery] int quantity = 1)
        {
            try
            {
                // 1. Validate quantity
                if (quantity <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Quantity must be greater than 0"
                    });
                }

                // 2. Find cart
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cart == null)
                {
                    return NotFound(new
                    {
                        message = $"Cart with ID {id} not found"
                    });
                }

                // 3. Find product
                var product = await _context.Products
                    .FindAsync(productId);

                if (product == null)
                {
                    return NotFound(new
                    {
                        message = $"Product with ID {productId} not found"
                    });
                }

                // 4. Validate stock
                if (product.StockQuantity < quantity)
                {
                    return BadRequest(new
                    {
                        message =
                            $"Insufficient stock. Available: {product.StockQuantity}, Requested: {quantity}"
                    });
                }

                // 5. Check whether product already exists in cart
                var existingItem = cart.CartItems
                    .FirstOrDefault(ci => ci.ProductId == productId);

                if (existingItem != null)
                {
                    // 6. Validate total quantity against stock
                    if (existingItem.Quantity + quantity > product.StockQuantity)
                    {
                        return BadRequest(new
                        {
                            message =
                                $"Cannot add {quantity} more items. " +
                                $"Max available: {product.StockQuantity - existingItem.Quantity}"
                        });
                    }

                    // 7. Update existing quantity
                    existingItem.Quantity += quantity;
                }
                else
                {
                    // 8. Add new cart item
                    var cartItem = new CartItem
                    {
                        CartId = id,
                        ProductId = productId,
                        Quantity = quantity,
                        Price = product.Price
                    };

                    _context.CartItems.Add(cartItem);
                }

                // 9. Save changes
                await _context.SaveChangesAsync();

                // 10. Reload cart with updated items
                var updatedCart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.Id == id);

                // 11. Safety check after reload
                if (updatedCart == null)
                {
                    return NotFound(new
                    {
                        message =
                            $"Cart with ID {id} not found after adding item"
                    });
                }

                // 12. Return updated cart
                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                // Log complete exception on server
                Console.WriteLine(
                    $"Error adding item to cart: {ex}"
                );

                // Do not expose internal exception details
                return StatusCode(500, new
                {
                    message =
                        "An error occurred while adding the item to the cart."
                });
            }
        }

        /// <summary>
        /// Remove a product from the cart.
        /// </summary>
        [HttpDelete("{id}/remove-item")]
        public async Task<ActionResult<Cart>> RemoveFromCart(
            int id,
            [FromQuery] int productId)
        {
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cart == null)
                {
                    return NotFound(new
                    {
                        message = $"Cart with ID {id} not found"
                    });
                }

                var cartItem = cart.CartItems
                    .FirstOrDefault(ci => ci.ProductId == productId);

                if (cartItem == null)
                {
                    return NotFound(new
                    {
                        message =
                            $"Product with ID {productId} not found in cart"
                    });
                }

                _context.CartItems.Remove(cartItem);

                await _context.SaveChangesAsync();

                var updatedCart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (updatedCart == null)
                {
                    return NotFound(new
                    {
                        message =
                            $"Cart with ID {id} not found after removing item"
                    });
                }

                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error removing item from cart: {ex}"
                );

                return StatusCode(500, new
                {
                    message =
                        "An error occurred while removing the item from the cart."
                });
            }
        }

        /// <summary>
        /// Update quantity of a product in the cart.
        /// </summary>
        [HttpPut("{id}/update-item")]
        public async Task<ActionResult<Cart>> UpdateCartItem(
            int id,
            [FromQuery] int productId,
            [FromQuery] int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Quantity must be greater than 0"
                    });
                }

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cart == null)
                {
                    return NotFound(new
                    {
                        message = $"Cart with ID {id} not found"
                    });
                }

                var cartItem = cart.CartItems
                    .FirstOrDefault(ci => ci.ProductId == productId);

                if (cartItem == null)
                {
                    return NotFound(new
                    {
                        message =
                            $"Product with ID {productId} not found in cart"
                    });
                }

                var product = await _context.Products
                    .FindAsync(productId);

                if (product == null)
                {
                    return NotFound(new
                    {
                        message =
                            $"Product with ID {productId} not found"
                    });
                }

                if (quantity > product.StockQuantity)
                {
                    return BadRequest(new
                    {
                        message =
                            $"Insufficient stock. Available: {product.StockQuantity}"
                    });
                }

                cartItem.Quantity = quantity;

                await _context.SaveChangesAsync();

                var updatedCart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (updatedCart == null)
                {
                    return NotFound(new
                    {
                        message =
                            $"Cart with ID {id} not found after updating item"
                    });
                }

                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error updating cart item: {ex}"
                );

                return StatusCode(500, new
                {
                    message =
                        "An error occurred while updating the cart item."
                });
            }
        }

        /// <summary>
        /// Get the purchase summary for a cart.
        /// Discount is applied when subtotal >= 5000.
        /// </summary>
        [HttpGet("{id}/summary")]
        public async Task<ActionResult<PurchaseSummary>> GetPurchaseSummary(
            int id)
        {
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cart == null)
                {
                    return NotFound(new
                    {
                        message = $"Cart with ID {id} not found"
                    });
                }

                var summary = new PurchaseSummary
                {
                    CartId = id,
                    Items = new List<CartItemSummary>()
                };

                decimal subtotal = 0;

                foreach (var item in cart.CartItems)
                {
                    if (item.Product == null)
                        continue;

                    decimal itemTotal =
                        item.Quantity * item.Price;

                    subtotal += itemTotal;

                    summary.Items.Add(new CartItemSummary
                    {
                        ProductId = item.ProductId,
                        ProductName = item.Product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price,
                        ItemTotal = itemTotal,
                        DiscountPercentage =
                            item.Product.Discount
                    });
                }

                summary.Subtotal = subtotal;

                if (subtotal >= DISCOUNT_THRESHOLD &&
                    cart.CartItems.Any())
                {
                    summary.DiscountPercentage =
                        cart.CartItems
                            .Where(ci => ci.Product != null)
                            .Max(ci => ci.Product!.Discount);

                    summary.DiscountAmount =
                        Math.Round(
                            subtotal *
                            summary.DiscountPercentage / 100,
                            2);
                }

                summary.GrandTotal =
                    subtotal - summary.DiscountAmount;

                return Ok(summary);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error calculating purchase summary: {ex}"
                );

                return StatusCode(500, new
                {
                    message =
                        "An error occurred while calculating the purchase summary."
                });
            }
        }

        /// <summary>
        /// Clear all items from the cart.
        /// </summary>
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
                    return NotFound(new
                    {
                        message = $"Cart with ID {id} not found"
                    });
                }

                _context.CartItems.RemoveRange(cart.CartItems);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cart cleared successfully"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error clearing cart: {ex}"
                );

                return StatusCode(500, new
                {
                    message =
                        "An error occurred while clearing the cart."
                });
            }
        }
    }
}