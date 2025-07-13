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
        Task<Dictionary<string, string>> LoginAndSaveAsync(string username, string password);
    }

    public class AuthService : IAuthService
    {
        private readonly ShopeeLoginService _loginSvc;
        private readonly IAccountTokenRepository _repo;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ShopeeLoginService loginSvc,
            IAccountTokenRepository repo,
            ILogger<AuthService> logger)
        {
            _loginSvc = loginSvc;
            _repo = repo;
            _logger = logger;
        }

        public async Task<Dictionary<string, string>> LoginAndSaveAsync(string username, string password)
        {
            _logger.LogInformation("Đang login Shopee cho user {User}", username);
            var headers = await _loginSvc.LoginAndGetHeaders(username, password);

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

            await _repo.CreateOrUpdateByUsername(token);
            _logger.LogInformation("Login thành công và lưu token cho user {User}", username);

            return headers;
        }
    }
}
