using EcommerceApiScrapingService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace EcommerceApiScrapingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public partial class ShopeeController : ControllerBase
    {
        private readonly IProductService _prodSvc;
        private readonly IAuthService _authSvc;

        public ShopeeController(IProductService prodSvc, IAuthService authSvc)
        {
            _prodSvc = prodSvc;
            _authSvc = authSvc;
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


        // API get list product chưa clone (filter theo pagination, theo danh mục, theo tất cả)
        // API Clone theo danh mục, tất cả, picked (kiểm tra record đã cloned)
        // API UpdateAndClone khi vào trang detail và chỉnh sửa


    }
}


