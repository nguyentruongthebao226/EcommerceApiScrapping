using EcommerceApiScrapingService.Configurations;
using EcommerceApiScrapingService.DTOs;
using EcommerceApiScrapingService.Models;
using EcommerceApiScrapingService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace EcommerceApiScrapingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShopeeController : ControllerBase
    {
        private readonly IProductService _prodSvc;
        private readonly IAuthService _authSvc;

        public ShopeeController(IProductService prodSvc, IAuthService authSvc)
        {
            _prodSvc = prodSvc;
            _authSvc = authSvc;
        }

        [HttpPost("login")]
        public async Task<IActionResult> ShopeeLogin([FromBody] ShopeeLoginRequest req)
        {
            try
            {
                var headers = await _authSvc.LoginAndSaveAsync(req.Username, req.Password, req.IsHost);
                return Ok(new ApiResponse<Dictionary<string, string>>(200, "Login success", headers));
            }
            catch (Exception ex)
            {
                // Có thể phân biệt lỗi do Playwright, do DB, v.v.
                return StatusCode(500, new ApiResponse<object>(500, $"Login failed: {ex.Message}", null));
            }
        }

        [HttpGet("shop-info")]
        public async Task<IActionResult> ShopInfo([FromQuery] string username)
        {
            try
            {
                var info = await _prodSvc.GetShopInfoAsync(username);
                return Ok(info);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("product-list")]
        public async Task<IActionResult> ProductList(
           [FromQuery] string username,
           [FromQuery] int page = 1,
           [FromQuery] int size = 12)
        {
            try
            {
                var list = await _prodSvc.GetProductListAsync(username, page, size);
                return Ok(list);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("product-detail")]
        public async Task<IActionResult> GetProductDetail(
           [FromQuery] string username,
           [FromQuery] string productId)
        {
            try
            {
                var detail = await _prodSvc.GetProductDetailAsync(username, productId);
                return Ok(detail);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("product-create")]
        public async Task<IActionResult> CreateProduct(
           [FromQuery] string username,
           [FromBody] JsonObject payload)
        {
            try
            {
                var result = await _prodSvc.CreateProductAsync(username, payload);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("product-detail-clone")]
        public async Task<IActionResult> CloneProductByDetail(
            [FromQuery] string username,
            [FromQuery] string productId)
        {
            try
            {
                var resp = await _prodSvc.CloneProductAsync(username, productId);
                return Ok(resp);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}


