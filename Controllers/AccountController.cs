using EcommerceApiScrapingService.Configurations;
using EcommerceApiScrapingService.DTOs;
using EcommerceApiScrapingService.Models;
using EcommerceApiScrapingService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EcommerceApiScrapingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _service;
        private readonly AccountTokenService _accountTokenService;
        private readonly ShopeeOAuthSettings _oAuthSettings;
        private readonly ShopeeLoginService _loginService;

        public AccountController(AccountService service, IOptions<ShopeeOAuthSettings> oAuthSettings, ShopeeLoginService loginService, AccountTokenService accountTokenService)
        {
            _service = service;
            _oAuthSettings = oAuthSettings.Value;
            _loginService = loginService;
            _accountTokenService = accountTokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> ShopeeLogin()
        //public async Task<IActionResult> ShopeeLogin([FromBody] ShopeeLoginRequest req)
        {
            try
            {
                var req = new ShopeeLoginRequest
                {
                    Username = "0938641861",
                    Password = "Thebao226"
                };
                var headers = await _loginService.LoginAndGetHeaders(req.Username, req.Password);

                var accountToken = new AccountToken
                {
                    Username = req.Username,
                    Cookie = headers["Cookie"],
                    UserAgent = headers["User-Agent"],
                    Csrftoken = headers.ContainsKey("x-csrftoken") ? headers["x-csrftoken"] : "",
                    SPC_CDS = headers.ContainsKey("SPC_CDS") ? headers["SPC_CDS"] : "",
                    SPC_CDS_VER = headers.ContainsKey("SPC_CDS_VER") ? headers["SPC_CDS_VER"] : "",
                    XSapSec = headers.ContainsKey("x-sap-sec") ? headers["x-sap-sec"] : "",
                    RawHeadersJson = JsonSerializer.Serialize(headers),
                    CreatedAt = DateTime.UtcNow
                    // ExpiredAt = ... nếu có
                };
                _accountTokenService.Create(accountToken);
                return Ok(new ApiResponse<Dictionary<string, string>>(200, "Login success", headers));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(500, "Login failed: " + ex.Message, null));
            }
        }

        [HttpGet("shop-info")]
        public async Task<IActionResult> GetShopInfo()
        {
            string username = "0938641861";
            var token = _accountTokenService.GetByUsername(username);
            if (token == null)
                return NotFound(new { message = "Not found token for this account!" });

            // Chuẩn bị param (lấy từ DB)
            var spcCds = token.SPC_CDS;
            var spcCdsVer = token.SPC_CDS_VER;

            var url = $"https://banhang.shopee.vn/api/framework/selleraccount/shop_info/?SPC_CDS={spcCds}&SPC_CDS_VER={spcCdsVer}&_cache_api_sw_v1_=1";

            using var httpClient = new HttpClient();

            // Header gần giống browser, bổ sung Sec-* nếu muốn an toàn hơn
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", token.UserAgent);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", token.Cookie);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-csrftoken", token.Csrftoken);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-requested-with", "XMLHttpRequest");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://banhang.shopee.vn/");

            // Nếu bạn lưu thêm các Sec-* thì bổ sung luôn:
            //if (!string.IsNullOrEmpty(token.ScFeSession))
            //    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sc-Fe-Session", token.ScFeSession);
            //if (!string.IsNullOrEmpty(token.ScFeVer))
            //    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sc-Fe-Ver", token.ScFeVer);
            //if (!string.IsNullOrEmpty(token.SecChUa))
            //    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Ch-Ua", token.SecChUa);
            //if (!string.IsNullOrEmpty(token.SecChUaMobile))
            //    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Ch-Ua-Mobile", token.SecChUaMobile);
            //if (!string.IsNullOrEmpty(token.SecChUaPlatform))
            //    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Ch-Ua-Platform", token.SecChUaPlatform);

            // Call API
            var resp = await httpClient.GetAsync(url);
            var content = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode, new { message = "Shopee API error", detail = content });

            return Content(content, "application/json");
        }


        [HttpGet("product-list")]
        public async Task<IActionResult> GetProductList([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 12)
        {
            string username = "0938641861";

            var token = _accountTokenService.GetByUsername(username);
            if (token == null)
                return NotFound(new { message = "Không tìm thấy token cho tài khoản này!" });

            // Các tham số bắt buộc, lấy từ token hoặc từ FE truyền lên
            //var spcCds = "b1da2198-8632-4945-9a29-7d8e1b4a227e" ?? token.SPC_CDS;
            var spcCds = token.SPC_CDS;
            var spcCdsVer = token.SPC_CDS_VER ?? "2";
            var url = $"https://banhang.shopee.vn/api/v3/mpsku/list/v2/get_product_list?SPC_CDS={spcCds}&SPC_CDS_VER={spcCdsVer}&page_number={pageNumber}&page_size={pageSize}&list_type=live_all&need_ads=true";

            using var httpClient = new HttpClient();

            // Add đủ các header như browser
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "vi-VN,vi;q=0.9,en;q=0.8");
            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", "SPC_F=XM5BIInZC1AN2PgJU2eeZXtwpDoO42Zo; REC_T_ID=c4abe1bb-5982-11f0-b31d-02d42b1e350a; _gcl_au=1.1.2107933309.1751707863; _fbp=fb.1.1751707863594.767455174545417461; _hjSessionUser_868286=eyJpZCI6IjAyNWViYTM4LTkwMjEtNWJmMy1hMzliLWVjYzNhZDk1Y2M3MyIsImNyZWF0ZWQiOjE3NTE3MDc4NjQzNDksImV4aXN0aW5nIjp0cnVlfQ==; SPC_CLIENTID=XM5BIInZC1AN2PgJqctdrqricxlagbsd; SPC_SI=TKdjaAAAAABSN01uNk1FU7xhXAAAAAAAMUUyaDFsczU=; _gid=GA1.2.762681932.1751872211; SPC_SI=TKdjaAAAAABSN01uNk1FU7xhXAAAAAAAMUUyaDFsczU=; SPC_SEC_SI=v1-Y1JjSmF2QmZkNTk5QlIyUxlfk448gPd1WOvhwz+PCan7YOj0ZTkf1/2fOZysMXSzF9Z9IlNzkktiLPHo41HbvWBsn1JgJxbqMSpRjiFwnsM=; SPC_CDS_CHAT=cf8a0357-e75f-49d4-b465-4bd0aaa2bc76; SPC_CDS=b1da2198-8632-4945-9a29-7d8e1b4a227e; SPC_SC_SA_TK=; SPC_SC_SA_UD=; SPC_SC_OFFLINE_TOKEN=; SC_SSO=-; SC_SSO_U=-; SPC_ST=.TjV6aFFBQlljakpGTldCWNR+voq7YY7z7Vw14LJ+5atUVYpkI/wIrKYPmzsBBg5BT2qnwF4nOIdSW/FREMpBE2S0C/vNsVMJx8wiyRLUGZbWFpfnkhNUDNfm10mV6kNuBhE8qdIdj6zUJVgAl6QEc5nzxGbAFnATgleLnmxjCUEKAsPGjIeNVw0Mj7YDXCN5xahzhICz6U3ZVt9rc4x6MOYVN+on+H6rdkW3GgWYPLU=; SPC_SC_SESSION=gem3UI+VzmBxN0Bbeg8DJHKnimc7/fDi2gk5IHtypA4+2sXw+1Z+xOQ8HijZ26b1NW3HHYtraURP7AbqozEhRc5EF9UuBkex3Fk8bTE6E0T2McFxfxOssZStbM2qf96HiQTT6mLD6JBH0opIHYm06KyP47SlhnI9blJ80qewJBa7mGOcqvsjDEOFWIQYPcrEX_1_15103682; SPC_STK=1D3MN59IcuZyu1NEnZJFc5tujtpE7a/1wN4WAmXwXPjl59DQrVWLyjmhtfLR6HG8g2W2KYWbGMD6q2et58tN0/10VKnWXGeZ2V0QYZfyNEN6dKu6KyXXn90fBe4zhJW1+t+reWjyn1MfWfuscS6TePB20T6AIIUN1Gmi01eMuFwiqSBhSBEGlzxUlSzBbcvrAysirgiCpUytqTqhuOqzUerZMV0tN8FAIGFjajhDtppBLqxKi3qWlnguj0LU82g7Uo5trfQ5YRGxE9l8C4nXCaft3e2TNIHXJQ4g6oU6p5kew4ctBRav12jP7DB4qtm9AXgSLSFTjYMB0v/0rMfsJMF+7Wj45QRXgrNeqHyivZIAd6sMijinSH4o2VCEipaK; SC_DFP=ScCNoeLZznZTsuYuBAlvvBDDXhDytnWp; _QPWSDCXHZQA=323c29f5-7964-48a8-d34a-a3f042d391a4; REC7iLP4Q=34dc5bd7-e10b-4ccd-ad43-75a708d33152; _sapid=8abd8139432aea518fe04d3bbc570e581f13c6b3196509f3db7a7539; _ga_4GPP1ZXG63=GS2.1.s1751880894$o4$g1$t1751882154$j60$l0$h0; _gcl_aw=GCL.1751935296.CjwKCAjw4K3DBhBqEiwAYtG_9EwYOFLt18Z1iGm7bfarQkiXqCmkrjjThxDsFaAWcInlkmcKATuYKxoCokkQAvD_BwE; _med=cpc; SPC_U=15103682; SPC_R_T_ID=r76S6J9dD+ATu7swxpphFkr0GBJzYiRDc0twc/Fy+XQnHdyWULnL36LeHDrWpghQ41zdbBkkVAkDv4/VQHKtOFI+X2Uxkx+GHx3v0brTwm/ugyipPSwuVCEH68CB3MsoeFaaa4K+skaR8yBMdxmBntvy4u4A7xUJDwTNxrvsFxo=; SPC_R_T_IV=OEdPbDBUZ0ZJVDFoUUZKSQ==; SPC_T_ID=r76S6J9dD+ATu7swxpphFkr0GBJzYiRDc0twc/Fy+XQnHdyWULnL36LeHDrWpghQ41zdbBkkVAkDv4/VQHKtOFI+X2Uxkx+GHx3v0brTwm/ugyipPSwuVCEH68CB3MsoeFaaa4K+skaR8yBMdxmBntvy4u4A7xUJDwTNxrvsFxo=; SPC_T_IV=OEdPbDBUZ0ZJVDFoUUZKSQ==; _gcl_gs=2.1.k1$i1751935366$u121375384; _ga=GA1.2.366844464.1751707864; SPC_EC=.RTlRMWwzU2pvTWtGc2RSTvR7cHorKu4TFt1fqSOswLOtlQkDIzZGCjNCu/EUFA9PxVy66uhE74p/62MjFxQep42mYgG3K/YrXXBPGxYz9uMWIUbeynPVYKp+HRHiWZ9kTxakCDD7lhyDvH2VMs45Y9r+rbuVnf3lCoAKlW6EcYRT2ZaY1FAizZujZx2iI9rfjUalzodrv8BTcJIz8uESqFKHpOVeCza3XKKEwMO9WH4=; CTOKEN=HON6HlucEfCaMdbC5mql7Q%3D%3D; ds=81a031ce7d1258135179e99b519ae32f; shopee_webUnique_ccd=WxEdn6pRdhCYE8u5vtUalw%3D%3D%7CjQPTJXvzv9rbCN9sNepXu3NJi04BjjkaFizwkq4rDEzudpO%2FZyyAOdlNBx%2BXoPlP6NCAfA1iSzU%3D%7CJAjnyojzPPPuhqZW%7C08%7C3" ?? token.Cookie);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", token.Cookie);
            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Locale", "vi");
            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Priority", "u=1, i");
            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://banhang.shopee.vn/merchant/inventory/product/list");

            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sc-fe-session", token.ScFeSession ?? "");
            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sc-fe-ver", token.ScFeVer ?? "");

            //// Các Sec-* header
            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", token.SecChUa ?? "");
            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", token.SecChUaMobile ?? "?0");
            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", token.SecChUaPlatform ?? "\"Windows\"");


            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sc-fe-session", "082102FB887C9D20");
            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sc-fe-ver", "21.105839");

            // Các Sec-* header
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\"Not)A;Brand\"; v = \"8\", \"Chromium\"; v = \"138\", \"Google Chrome\"; v = \"138\"");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");


            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-dest", "empty");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-mode", "cors");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-site", "same-origin");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", token.UserAgent);

            // Gửi request
            //var resp = await httpClient.GetAsync(url);
            //var content = await resp.Content.ReadAsStringAsync();

            //if (!resp.IsSuccessStatusCode)
            //    return StatusCode((int)resp.StatusCode, new { message = "Shopee API error", detail = content });

            //return Content(content, "application/json");
            // (KHÔNG decode byte thủ công!)
            var response = await httpClient.GetAsync(url);

            // Lấy Content-Type trả về từ Shopee (browser cũng như này)
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";

            // Đọc đúng response dạng string
            var content = await response.Content.ReadAsStringAsync();

            // Trả ra đúng content-type cho Postman/browser đọc
            return Content(content, contentType);
        }


        [HttpGet("product-detail")]
        public async Task<IActionResult> GetProductDetail([FromQuery] string productId)
        {
            // Lấy headers từ DB (tuỳ bạn lưu theo shopId/user...)

            string username = "0938641861";

            var token = _accountTokenService.GetByUsername(username);
            if (token == null)
                return NotFound(new { message = "Không tìm thấy token cho tài khoản này!" });

            // Build URL (SPC_CDS... lấy từ headers/token hoặc truyền qua param)
            var spcCds = token.SPC_CDS;
            var spcCdsVer = token.SPC_CDS_VER ?? "2";
            var url = $"https://banhang.shopee.vn/api/v3/product/get_product_info?SPC_CDS={spcCds}&SPC_CDS_VER={spcCdsVer}&product_id={productId}&is_draft=false";

            using var httpClient = new HttpClient();

            // Add đủ các header như browser
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "vi-VN,vi;q=0.9,fr-FR;q=0.8,fr;q=0.7,en-US;q=0.6,en;q=0.5");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", token.Cookie);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-dest", "empty");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-mode", "cors");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-site", "same-origin");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", token.UserAgent);

            var response = await httpClient.GetAsync(url);

            // Lấy Content-Type trả về từ Shopee (browser cũng như này)
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";

            // Đọc đúng response dạng string
            var content = await response.Content.ReadAsStringAsync();

            // Trả ra đúng content-type cho Postman/browser đọc
            return Content(content, contentType);
        }

    }
}
public class ShopeeLoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}