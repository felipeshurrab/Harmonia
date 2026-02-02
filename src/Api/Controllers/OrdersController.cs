using Application.Dtos.Orders;
using Application.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userId = GetCurrentUserId();

            var orders = userRole == "Administrator"
                ? await _orderService.GetAllOrders()
                : await _orderService.GetOrdersBySellerId(userId);

            return Ok(orders);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound();

            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole != "Administrator")
            {
                var userId = GetCurrentUserId();
                var sellerOrders = await _orderService.GetOrdersBySellerId(userId);
                if (!sellerOrders.Any(o => o.Id == id))
                    return Forbid();
            }

            return Ok(order);
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var userName = User.FindFirstValue(JwtRegisteredClaimNames.Name) ?? "Unknown";

            var order = await _orderService.CreateOrderWithStockValidation(request, userId, userName);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
        }
    }
}
