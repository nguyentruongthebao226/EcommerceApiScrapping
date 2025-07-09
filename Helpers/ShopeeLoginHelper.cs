using Microsoft.Playwright;

namespace EcommerceApiScrapingService.Helpers
{
    public class ShopeeLoginHelper
    {
        public async Task<Dictionary<string, string>> LoginShopeeSellerAsync(string username, string password, bool headless = true)
        {
            var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = headless });
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            // 1. Đăng nhập trang Seller
            await page.GotoAsync("https://accounts.shopee.vn/seller/login");

            // 2. Điền user/pass
            await page.FillAsync("input[name=\"loginKey\"]", username);
            await page.FillAsync("input[name=\"password\"]", password);
            await page.ClickAsync("button.ZzzLTG");

            // 3. Đợi redirect thành công
            await page.WaitForURLAsync("https://banhang.shopee.vn/**", new PageWaitForURLOptions { Timeout = 30000 });
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // 4. Lấy cookie từ context
            var cookies = await context.CookiesAsync();
            string cookieHeader = string.Join("; ", cookies.Select(c => $"{c.Name}={c.Value}"));

            // 5. Lấy csrftoken (nếu có)
            string csrftoken = cookies.FirstOrDefault(c => c.Name == "csrftoken")?.Value ?? "";

            // 6. Lấy User-Agent
            var userAgent = await page.EvaluateAsync<string>("() => navigator.userAgent");

            // 7. Lấy các cookie đặc biệt
            string spc_cds = cookies.FirstOrDefault(c => c.Name == "SPC_CDS")?.Value ?? "";
            string spc_cds_ver = cookies.FirstOrDefault(c => c.Name == "SPC_CDS_VER")?.Value ?? "";

            await browser.CloseAsync();

            // 8. Trả về “bộ header” cho API
            var headers = new Dictionary<string, string>
        {
            {"User-Agent", userAgent },
            {"Cookie", cookieHeader },
            {"x-csrftoken", csrftoken },
            {"x-requested-with", "XMLHttpRequest" },
            {"SPC_CDS", spc_cds },
            {"SPC_CDS_VER", spc_cds_ver }
        };
            return headers;
        }
    }
}
