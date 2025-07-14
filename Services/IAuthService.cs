using EcommerceApiScrapingService.Models;
using EcommerceApiScrapingService.Repositories;
using System.Text.Json;

namespace EcommerceApiScrapingService.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Đăng nhập Shopee qua Playwright, lưu token vào DB rồi trả về các header cần thiết.
        /// </summary>
        Task<Dictionary<string, string>> LoginAndSaveAsync(string username, string password, bool isHost);
        Task<List<Account>> ShopInfosAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly ShopeeLoginService _loginSvc;
        private readonly IAccountTokenRepository _tokenRepo;
        private readonly IAccountRepository _accountRepo;
        private readonly IShopeeClient _shopee;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
         ShopeeLoginService loginSvc,
         IAccountTokenRepository tokenRepo,
         IAccountRepository accountRepo,
         IShopeeClient shopee,
         ILogger<AuthService> logger)
        {
            _loginSvc = loginSvc;
            _tokenRepo = tokenRepo;
            _accountRepo = accountRepo;
            _shopee = shopee;
            _logger = logger;
        }

        public async Task<Dictionary<string, string>> LoginAndSaveAsync(string username, string password, bool isHost = false)
        {
            _logger.LogInformation("Đang login Shopee cho user {User}", username);
            // 1) Lấy headers qua Playwright
            var headers = await _loginSvc.LoginAndGetHeaders(username, password);

            // 2) Build AccountToken và lưu hoặc cập nhật
            var token = new AccountToken
            {
                Username = username,
                Cookie = headers["Cookie"],
                UserAgent = headers["User-Agent"],
                Csrftoken = headers.GetValueOrDefault("x-csrftoken", ""),
                SPC_CDS = headers.GetValueOrDefault("SPC_CDS", ""),
                SPC_CDS_VER = headers.GetValueOrDefault("SPC_CDS_VER", ""),
                XSapSec = headers.GetValueOrDefault("x-sap-sec", ""),
                Cookies = headers.ToDictionary(k => k.Key, v => v.Value),
                RawHeadersJson = JsonSerializer.Serialize(headers),
                CreatedAt = DateTime.UtcNow
            };

            await _tokenRepo.CreateOrUpdateByUsername(token);
            _logger.LogInformation("Login thành công và lưu token cho user {User}", username);

            // 3) Gọi Shopee shop-info bằng token mới
            var shopInfo = await _shopee.GetShopInfoAsync(token);

            // 4) Extract các trường bạn cần
            var shopId = shopInfo["shop_id"]?.GetValue<long>().ToString() ?? "";
            var shopRegion = shopInfo["shop_region"]?.GetValue<string>() ?? "";
            var shopName = shopInfo["name"]?.GetValue<string>() ?? "";

            // 5) Build đối tượng Account và lưu/cập nhật
            var account = new Account
            {
                Username = username,
                ShopId = shopId,
                Country = shopRegion,
                ShopName = shopName,
                LastLoginAt = DateTime.UtcNow,
                IsHost = isHost,       // tuỳ logic của bạn
                Platform = "Shopee",
                CreatedAt = DateTime.UtcNow
            };
            await _accountRepo.CreateOrUpdateByUsernameAsync(account);
            _logger.LogInformation("Account đã được lưu/cập nhật cho shop {ShopId}", shopId);


            return headers;
        }

        public async Task<List<Account>> ShopInfosAsync()
        {
            var allShop = await _accountRepo.GetAllAsync();
            if (allShop != null)
                return allShop;
            return new List<Account>();
        }
    }
}
