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


        //[HttpGet("product-detail")]
        //public async Task<IActionResult> GetProductDetail([FromQuery] string productId)
        //{
        //    // Lấy headers từ DB (tuỳ bạn lưu theo shopId/user...)

        //    string username = "0938641861";

        //    var token = _accountTokenService.GetByUsername(username);
        //    if (token == null)
        //        return NotFound(new { message = "Không tìm thấy token cho tài khoản này!" });

        //    // Build URL (SPC_CDS... lấy từ headers/token hoặc truyền qua param)
        //    var spcCds = token.SPC_CDS;
        //    var spcCdsVer = token.SPC_CDS_VER ?? "2";
        //    var url = $"https://banhang.shopee.vn/api/v3/product/get_product_info?SPC_CDS={spcCds}&SPC_CDS_VER={spcCdsVer}&product_id={productId}&is_draft=false";

        //    using var httpClient = new HttpClient();

        //    // Add đủ các header như browser
        //    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
        //    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "vi-VN,vi;q=0.9,fr-FR;q=0.8,fr;q=0.7,en-US;q=0.6,en;q=0.5");
        //    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", token.Cookie);
        //    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-dest", "empty");
        //    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-mode", "cors");
        //    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-site", "same-origin");
        //    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", token.UserAgent);

        //    var response = await httpClient.GetAsync(url);

        //    // Lấy Content-Type trả về từ Shopee (browser cũng như này)
        //    var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";

        //    // Đọc đúng response dạng string
        //    var content = await response.Content.ReadAsStringAsync();

        //    using var doc = JsonDocument.Parse(content);

        //    // Lấy node product_info
        //    var prodEl = doc.RootElement
        //        .GetProperty("data");

        //    var result = Content(prodEl.GetRawText(), "application/json");


        //    return result;


        //}



        //[HttpPost("product-detail")]
        //public async Task<IActionResult> GetProductDetail([FromQuery] string productId)
        //{
        //    // 1) Lấy token & build URL GET
        //    string username = "0938641861";
        //    var token = _accountTokenService.GetByUsername(username);
        //    if (token == null)
        //        return NotFound(new { message = "Không tìm thấy token!" });

        //    var spcCds = token.SPC_CDS;
        //    var spcCdsVer = token.SPC_CDS_VER ?? "2";
        //    var getUrl =
        //      $"https://banhang.shopee.vn/api/v3/product/get_product_info" +
        //      $"?SPC_CDS={spcCds}&SPC_CDS_VER={spcCdsVer}" +
        //      $"&product_id={productId}&is_draft=false";

        //    using var httpClient = new HttpClient();
        //    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
        //    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", token.UserAgent);
        //    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", token.Cookie);
        //    // … thêm các header khác nếu cần

        //    var getResp = await httpClient.GetAsync(getUrl);
        //    if (!getResp.IsSuccessStatusCode)
        //        return StatusCode((int)getResp.StatusCode,
        //                          await getResp.Content.ReadAsStringAsync());

        //    var rawJson = await getResp.Content.ReadAsStringAsync();

        //    // 2) Parse & clone JsonNode
        //    var rootNode = JsonNode.Parse(rawJson)!;
        //    var prodInfoNode = rootNode["data"]!["product_info"]!;
        //    var prodInfoClone = JsonNode.Parse(prodInfoNode.ToJsonString())!;

        //    // 3) Mutate fields
        //    prodInfoClone["id"] = null;  // để hệ thống cấp lại hoặc bỏ id cũ
        //    prodInfoClone["create_time"] = JsonValue.Create(
        //        DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        //    );

        //    // 4) Remove complaint_policy hoàn toàn
        //    prodInfoClone.AsObject().Remove("complaint_policy");

        //    // 5) Build payload wrapper và POST
        //    var createPayload = new JsonObject
        //    {
        //        ["is_draft"] = false,
        //        ["product_info"] = prodInfoClone
        //    };

        //    var postJson = createPayload.ToJsonString();
        //    using var content = new StringContent(
        //        postJson,
        //        Encoding.UTF8,
        //        "application/json"
        //    );

        //    var createUrl =
        //      $"https://banhang.shopee.vn/api/v3/product/create_product_info" +
        //      $"?SPC_CDS={spcCds}&SPC_CDS_VER={spcCdsVer}";
        //    var postResp = await httpClient.PostAsync(createUrl, content);

        //    var respStr = await postResp.Content.ReadAsStringAsync();
        //    if (!postResp.IsSuccessStatusCode)
        //        return StatusCode((int)postResp.StatusCode, respStr);

        //    // 6) Trả về kết quả của POST
        //    return Content(respStr, "application/json");
        //}



        [HttpGet("product-detail")]
        public async Task<IActionResult> GetProductDetail([FromQuery] string productId)
        {
            string username = "0938641861";
            var token = _accountTokenService.GetByUsername(username);
            if (token == null)
                return NotFound(new { message = "Không tìm thấy token!" });

            // 1) Lấy JSON detail
            var spcCds = token.SPC_CDS;
            var spcCdsVer = token.SPC_CDS_VER ?? "2";
            var getUrl = $"https://banhang.shopee.vn/api/v3/product/get_product_info" +
                         $"?SPC_CDS={spcCds}&SPC_CDS_VER={spcCdsVer}" +
                         $"&product_id={productId}&is_draft=false";

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", token.UserAgent);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", token.Cookie);
            var getResp = await httpClient.GetAsync(getUrl);
            if (!getResp.IsSuccessStatusCode)
                return StatusCode((int)getResp.StatusCode, await getResp.Content.ReadAsStringAsync());

            var rawJson = await getResp.Content.ReadAsStringAsync();
            var root = JsonNode.Parse(rawJson)!;
            var detail = root["data"]!["product_info"]!;

            // 2) Map sang payload cho create
            JsonObject payload = MapDetailToCreatePayload(detail);

            // 3) Gửi request create
            var createUrl = $"https://banhang.shopee.vn/api/v3/product/create_product_info" +
                            $"?SPC_CDS={spcCds}&SPC_CDS_VER={spcCdsVer}";
            using var content = new StringContent(
                new JsonObject
                {
                    ["product_info"] = payload,
                    ["is_draft"] = false
                }.ToJsonString(),
                Encoding.UTF8,
                "application/json"
            );
            var postResp = await httpClient.PostAsync(createUrl, content);
            var respStr = await postResp.Content.ReadAsStringAsync();
            if (!postResp.IsSuccessStatusCode)
                return StatusCode((int)postResp.StatusCode, respStr);

            return Content(respStr, "application/json");
        }




        private JsonObject MapDetailToCreatePayload(JsonNode detail)
        {
            var dest = new JsonObject();

            // 1) Các trường cơ bản
            if (detail["name"] is JsonNode name)
                dest["name"] = name.DeepClone();
            if (detail["enable_model_level_dts"] is JsonNode dts)
                dest["enable_model_level_dts"] = dts.DeepClone();
            if (detail["category_path"] is JsonArray cp)
                dest["category_path"] = cp.DeepClone();
            if (detail["weight"] is JsonObject wgt)
                dest["weight"] = wgt.DeepClone();
            if (detail["condition"] is JsonNode cond)
                dest["condition"] = cond.DeepClone();
            if (detail["parent_sku"] is JsonNode ps)
                dest["parent_sku"] = ps.DeepClone();

            // brand_id nằm trong brand_info
            if (detail["brand_info"] is JsonObject bi && bi["brand_id"] is JsonNode bid)
                dest["brand_id"] = bid.DeepClone();

            // 2) Attributes, images, long_images
            if (detail["attributes"] is JsonArray attrs)
                dest["attributes"] = attrs.DeepClone();
            if (detail["images"] is JsonArray imgs)
                dest["images"] = imgs.DeepClone();
            if (detail["long_images"] is JsonArray limgs)
                dest["long_images"] = limgs.DeepClone();

            // 3) Tier variations
            if (detail["std_tier_variation_list"] is JsonArray stdTier)
                dest["std_tier_variation_list"] = stdTier.DeepClone();

            // 4) Các trường thông tin khác
            if (detail["size_chart_info"] is JsonObject sci)
                dest["size_chart_info"] = sci.DeepClone();
            if (detail["video_list"] is JsonArray vl)
                dest["video_list"] = vl.DeepClone();
            if (detail["description_info"] is JsonObject di)
                dest["description_info"] = di.DeepClone();
            if (detail["dimension"] is JsonObject dim)
                dest["dimension"] = dim.DeepClone();
            if (detail["pre_order_info"] is JsonObject poi)
                dest["pre_order_info"] = poi.DeepClone();
            if (detail["wholesale_list"] is JsonArray wl)
                dest["wholesale_list"] = wl.DeepClone();

            // unlisted (hoặc is_unlisted tuỳ field)
            if (detail["is_unlisted"] is JsonNode un)
                dest["unlisted"] = un.DeepClone();

            // 5) Logistics channels: phải có ít nhất một channel.enabled = true
            JsonArray logisticsClone;
            if (detail["logistics_channels"] is JsonArray chans && chans.Count > 0)
            {
                logisticsClone = chans.DeepClone()!.AsArray();
                // bật channel đầu tiên nếu chưa có channel nào enabled
                if (!logisticsClone.OfType<JsonObject>().Any(c => c["enabled"]?.GetValue<bool>() == true))
                    logisticsClone.OfType<JsonObject>().First()["enabled"] = true;
            }
            else
            {
                // fallback: thêm một channel mặc định (ví dụ 5002)
                logisticsClone = new JsonArray {
            new JsonObject {
                ["size"] = 0,
                ["price"] = "0",
                ["cover_shipping_fee"] = false,
                ["enabled"] = true,
                ["channelid"] = 5002,
                ["sizeid"] = 0
            }
        };
            }
            dest["logistics_channels"] = logisticsClone;

            // 6) Build model_list theo đúng format payload mẫu
            var newModels = new JsonArray();
            if (detail["model_list"] is JsonArray modelList)
            {
                foreach (var item in modelList.OfType<JsonObject>())
                {
                    var nm = new JsonObject
                    {
                        ["id"] = 0
                    };

                    if (item["tier_index"] is JsonArray ti)
                        nm["tier_index"] = ti.DeepClone();
                    if (item["is_default"] is JsonNode df)
                        nm["is_default"] = df.DeepClone();
                    if (item["sku"] is JsonNode sku)
                        nm["sku"] = sku.DeepClone();

                    // price: lấy input_normal_price
                    if (item["price_info"] is JsonObject pi && pi["input_normal_price"] is JsonNode inp)
                        nm["price"] = inp.DeepClone();

                    // stock_setting_list: chỉ lấy sellable_stock
                    if (item["stock_detail"] is JsonObject sd &&
                        sd["seller_stock_info"] is JsonArray ssi &&
                        ssi.FirstOrDefault() is JsonObject firstSeller &&
                        firstSeller["sellable_stock"] is JsonNode ss)
                    {
                        nm["stock_setting_list"] = new JsonArray {
                    new JsonObject { ["sellable_stock"] = ss.DeepClone() }
                };
                    }

                    newModels.Add(nm);
                }
            }
            dest["model_list"] = newModels;

            return dest;
        }


    }
}
public class ShopeeLoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class ShopeeResponse<T>
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("msg")]
    public string Msg { get; set; }

    [JsonPropertyName("user_message")]
    public string UserMessage { get; set; }

    [JsonPropertyName("data")]
    public T Data { get; set; }
}

public class DataContainer
{
    [JsonPropertyName("product_info")]
    public ProductInfo ProductInfo { get; set; }

    [JsonPropertyName("long_images")]
    public List<string> LongImages { get; set; }
}

public class ProductInfo
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("images")]
    public List<string> Images { get; set; }
}
