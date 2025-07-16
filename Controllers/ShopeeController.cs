using EcommerceApiScrapingService.DTOs;
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
           [FromQuery] int size = 12,
           [FromQuery] bool checkCloned = false)
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

        [HttpGet("product-list-filter")]
        public async Task<IActionResult> ProductListFilter(
          [FromQuery] string username,
          [FromQuery] int page = 1,
          [FromQuery] int size = 12,
          [FromQuery] int category_id = 0)
        {
            try
            {
                var list = await _prodSvc.GetProductListWithFilterAndCheckClonedAsync(username, page, size, category_id, true);
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
        public async Task<IActionResult> CloneProductsAsync(
            [FromQuery] string usernameOriginal,
            [FromQuery] string usernameDestination,
            [FromBody] CloneProductsRequest request)
        {
            try
            {
                var result = await _prodSvc.CloneProductsAsync(usernameDestination, usernameOriginal, request.ProductIds);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("product-detail-clone")]
        public async Task<IActionResult> CloneProductByDetail(
          [FromQuery] string usernameDestination,
          [FromBody] JsonObject payload)
        {
            try
            {
                var resp = await _prodSvc.CloneProductInDetail(usernameDestination, payload);
                return Ok(resp);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("product-detail-clone-test")]
        public async Task<IActionResult> CloneProductByDetail(
            [FromQuery] string usernameOriginal,
            [FromQuery] string usernameDestination,
            [FromQuery] string productId)
        {
            try
            {
                var resp = await _prodSvc.CloneProductAsync(usernameDestination, usernameOriginal, productId);
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


