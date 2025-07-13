using EcommerceApiScrapingService.Configurations;
using EcommerceApiScrapingService.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EcommerceApiScrapingService.Repositories
{
    public interface IAccountTokenRepository : IRepository<AccountToken>
    {
        // thêm method đặc thù nếu cần, ví dụ:
        Task CreateOrUpdateByUsername(AccountToken tokenIn);
        Task<AccountToken> GetByUsername(string username);
    }
    public class AccountTokenRepository
    : MongoRepository<AccountToken>, IAccountTokenRepository
    {
        public AccountTokenRepository(IOptions<ShopeeDatabaseSettings> opts)
        : base(opts)
        {
        }

        public async Task<AccountToken> GetByUsername(string username)
        {
            var filter = Builders<AccountToken>.Filter.Eq(t => t.Username, username);
            var existing = await _col.Find(filter).FirstOrDefaultAsync();
            return existing;
        }
        public async Task CreateOrUpdateByUsername(AccountToken tokenIn)
        {
            // 1) Tìm xem đã có token cho username này chưa
            var filter = Builders<AccountToken>.Filter.Eq(t => t.Username, tokenIn.Username);
            var existing = await _col.Find(filter).FirstOrDefaultAsync();

            if (existing == null)
            {
                // Không có -> tạo mới
                tokenIn.CreatedAt = DateTime.UtcNow;
                await _col.InsertOneAsync(tokenIn);
            }
            else
            {
                // Có rồi -> update các trường
                tokenIn.Id = existing.Id;
                tokenIn.CreatedAt = existing.CreatedAt;
                tokenIn.ExpiredAt = tokenIn.ExpiredAt;
                tokenIn.RawHeadersJson = tokenIn.RawHeadersJson;
                // (Các property khác đã có trong tokenIn, gán sẵn trước khi gọi)

                await _col.ReplaceOneAsync(
                    Builders<AccountToken>.Filter.Eq(t => t.Id, existing.Id),
                    tokenIn
                );
            }
        }
    }
}
