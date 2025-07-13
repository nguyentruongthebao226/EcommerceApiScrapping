using EcommerceApiScrapingService.DTOs;
using EcommerceApiScrapingService.Models;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;

namespace EcommerceApiScrapingService.Services
{
    public interface IShopeeClient
    {
        Task<JsonNode> GetShopInfoAsync(AccountToken token);
        Task<JsonNode> GetProductListAsync(AccountToken token, int page, int size);
        Task<JsonNode> GetProductDetailAsync(AccountToken token, string productId);
        Task<JsonNode> CreateProductAsync(AccountToken token, JsonObject payload);
    }

    public class ShopeeClient : IShopeeClient
    {
        private readonly HttpClient _http;
        private readonly ShopeeApiOptions _opt;
        public ShopeeClient(HttpClient http, IOptions<ShopeeApiOptions> opt)
        {
            _http = http;
            _opt = opt.Value;
        }

        private HttpRequestMessage NewRequest(HttpMethod method, string path, AccountToken tkn)
        {
            var url = $"{_opt.BaseUrl}{path}"
                    + $"?SPC_CDS={tkn.SPC_CDS}&SPC_CDS_VER={tkn.SPC_CDS_VER}";
            var req = new HttpRequestMessage(method, url);

            // mandatory headers
            req.Headers.TryAddWithoutValidation("User-Agent", tkn.UserAgent);
            req.Headers.TryAddWithoutValidation("Cookie", tkn.Cookie);
            req.Headers.TryAddWithoutValidation("x-csrftoken", tkn.Csrftoken);
            //req.Headers.TryAddWithoutValidation("x-requested-with", "XMLHttpRequest");

            // you can add any other cookie fields you saved:
            if (!string.IsNullOrEmpty(tkn.XSapSec))
                req.Headers.TryAddWithoutValidation("x-sap-sec", tkn.XSapSec);

            // Shopee often also requires these
            req.Headers.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
            req.Headers.TryAddWithoutValidation("Accept-Language", "vi-VN,vi;q=0.9,en;q=0.8");
            req.Headers.TryAddWithoutValidation("Referer", "https://banhang.shopee.vn/");
            req.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Not)A;Brand\"; v = \"8\", \"Chromium\"; v = \"138\", \"Google Chrome\"; v = \"138\"");
            req.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            req.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            req.Headers.TryAddWithoutValidation("sec-fetch-dest", "empty");
            req.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
            req.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
            // …and any other Sec-CH-UA headers you need…

            return req;
        }

        public async Task<JsonNode> GetShopInfoAsync(AccountToken token)
        {
            var req = NewRequest(HttpMethod.Get, _opt.Endpoints.ShopInfo, token);
            using var resp = await _http.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            return JsonNode.Parse(json)!["data"]!;
        }

        public async Task<JsonNode> GetProductListAsync(AccountToken token, int page, int size)
        {
            // append paging to path or to the query string in NewRequest
            var req = NewRequest(HttpMethod.Get, _opt.Endpoints.GetProductList, token);
            var uri = new UriBuilder(req.RequestUri!)
            {
                Query = req.RequestUri!.Query +
                        $"&page_number={page}&page_size={size}&list_type=live_all&need_ads=true"
            };
            req.RequestUri = uri.Uri;

            using var resp = await _http.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            return JsonNode.Parse(await resp.Content.ReadAsStringAsync())!;
        }

        public async Task<JsonNode> GetProductDetailAsync(AccountToken token, string productId)
        {
            var req = NewRequest(HttpMethod.Get, _opt.Endpoints.GetProductDetail, token);
            var uri = new UriBuilder(req.RequestUri!)
            {
                Query = req.RequestUri!.Query + $"&product_id={productId}&is_draft=false"
            };
            req.RequestUri = uri.Uri;

            using var resp = await _http.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            var root = JsonNode.Parse(await resp.Content.ReadAsStringAsync())!;
            return root["data"]!["product_info"]!;
        }

        public async Task<JsonNode> CreateProductAsync(AccountToken token, JsonObject payload)
        {
            var req = NewRequest(HttpMethod.Post, _opt.Endpoints.CreateProduct, token);
            var wrapper = new JsonObject
            {
                ["is_draft"] = false,
                ["product_info"] = payload.DeepClone()
            };
            req.Content = new StringContent(
                wrapper.ToJsonString(),
                Encoding.UTF8,
                "application/json"
            );

            using var resp = await _http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            // Log cho bạn debug
            Console.WriteLine($"[CreateProduct] Status: {(int)resp.StatusCode} {resp.ReasonPhrase}");
            Console.WriteLine($"[CreateProduct] Body: {body}");

            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"CreateProduct failed {(int)resp.StatusCode}: {body}"
                );

            return JsonNode.Parse(body)!;
        }
    }
}
