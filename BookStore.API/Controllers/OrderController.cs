using BookStore.API.DTOs;
using BookStore.API.Models;
using BookStore.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var order = await _orderService
                .CreateOrderAsync(userId);

            return Ok(order);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var orders = await _orderService
                .GetOrdersAsync(userId);

            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var order = await _orderService
                .GetOrderByIdAsync(userId, id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult>UpdateOrderStatus(
            int id,
            [FromBody] OrderStatusUpdateDto request)
        {
            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
            {
                return BadRequest("Invalid order status value.");
            }

            var order = await _orderService.UpdateOrderStatusAsync(
                id, newStatus);
            return Ok(order);
        }
    }
}