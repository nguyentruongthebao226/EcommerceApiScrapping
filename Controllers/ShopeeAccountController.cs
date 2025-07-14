using EcommerceApiScrapingService.DTOs;
using EcommerceApiScrapingService.Models;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApiScrapingService.Controllers
{
    public partial class ShopeeController
    {
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

        [HttpGet("shopInfos")]
        public async Task<IActionResult> ShopInfos()
        {
            try
            {
                var shopInfos = await _authSvc.ShopInfosAsync();
                return Ok(new ApiResponse<List<Account>>(200, "Get list shop success", shopInfos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(500, $"Get list shop failed: {ex.Message}", null));
            }
        }
    }
}
