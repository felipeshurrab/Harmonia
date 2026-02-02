using Application.Dtos.Stock;
using Application.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;

        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entries = await _stockService.GetAllStockEntries();
            return Ok(entries);
        }

        [HttpGet("product/{productId:guid}")]
        public async Task<IActionResult> GetByProduct([FromRoute] Guid productId)
        {
            var entries = await _stockService.GetStockEntriesByProductId(productId);
            return Ok(entries);
        }

        [HttpPost]
        public async Task<IActionResult> AddStock([FromBody] StockEntryRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var entry = await _stockService.AddStockToProduct(request, userId);
            return CreatedAtAction(nameof(GetAll), entry);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
        }
    }
}
