using Microsoft.Playwright;

namespace EcommerceApiScrapingService.Services
{
    public class ShopeeLoginService
    {
        public async Task<Dictionary<string, string>> LoginAndGetHeaders(string username, string password)
        {
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            // Truy cập trang login Seller
            await page.GotoAsync("https://accounts.shopee.vn/seller/login");

            // Điền username & password
            await page.FillAsync("input[name='loginKey']", username);
            await page.FillAsync("input[name='password']", password);

            // Click login button (cần đúng selector, có thể phải update theo giao diện)
            await page.ClickAsync("button.ZzzLTG"); // Có thể thay đổi nếu selector đổi

            // Đợi chuyển trang thành công
            await page.WaitForURLAsync("https://banhang.shopee.vn/**", new() { Timeout = 20000 }); // chỉ chờ chuyển trang
                                                                                                   // KHÔNG cần WaitForLoadStateAsync(NetworkIdle) nữa
                                                                                                   // (Nếu vẫn muốn, chỉ dùng WaitForLoadStateAsync(Load) để tránh bị treo)
            await page.WaitForLoadStateAsync(LoadState.Load);

            // Lấy cookies sau khi đã login và load xong!
            var cookies = await context.CookiesAsync();
            var cookieHeader = string.Join("; ", cookies.Select(c => $"{c.Name}={c.Value}"));

            var csrftoken = cookies.FirstOrDefault(c => c.Name == "csrftoken")?.Value ?? "";

            var userAgent = await page.EvaluateAsync<string>("() => navigator.userAgent");

            var spcCds = cookies.FirstOrDefault(c => c.Name == "SPC_CDS")?.Value ?? "";
            var spcCdsVer = cookies.FirstOrDefault(c => c.Name == "SPC_CDS_VER")?.Value ?? "";

            // Không đóng browser/context ở đây nếu bạn muốn tiếp tục thao tác
            // Nếu chắc chắn đã lấy đủ thì mới đóng
            await browser.CloseAsync();

            return new Dictionary<string, string>
        {
            { "User-Agent", userAgent },
            { "Cookie", cookieHeader },
            { "x-csrftoken", csrftoken },
            { "x-requested-with", "XMLHttpRequest" },
            { "SPC_CDS", spcCds },
            { "SPC_CDS_VER", spcCdsVer }
        };
        }
    }
}
