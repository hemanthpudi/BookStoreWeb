using BookStore.API.DTOs;
using BookStore.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var cart = await _cartService.GetCartAsync(userId);

            return Ok(cart);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddToCart(
            CartItemRequestDto request)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var cart = await _cartService.AddToCartAsync(
                userId,
                request);

            return Ok(cart);
        }
        [HttpPut("items/{bookId}")]
        public async Task<IActionResult> UpdateCartItem(
            int bookId,
            CartItemRequestDto request)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var cart = await _cartService.UpdateCartItemAsync(
                userId,
                bookId,
                request.Quantity);

            return Ok(cart);
        }

        [HttpDelete("items/{bookId}")]
        public async Task<IActionResult> RemoveCartItem(int bookId)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            await _cartService.RemoveCartItemAsync(
                userId,
                bookId);

            return NoContent();
        }
        [HttpDelete]
        public async Task<IActionResult>ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId==null)
            {
                return Unauthorized();
            }
            await _cartService.ClearCartAsync(userId);
            return NoContent();
        }
    }
}