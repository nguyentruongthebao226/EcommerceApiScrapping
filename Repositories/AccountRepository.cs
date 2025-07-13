using EcommerceApiScrapingService.Configurations;
using EcommerceApiScrapingService.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EcommerceApiScrapingService.Repositories
{
    public interface IAccountRepository : IRepository<Account>
    {
        /// <summary>
        /// Tìm account theo username (loginKey).
        /// </summary>
        Task<Account?> GetByUsernameAsync(string username);

        /// <summary>
        /// Tạo mới hoặc cập nhật account theo username.
        /// </summary>
        Task CreateOrUpdateByUsernameAsync(Account account);
    }

    public class AccountRepository
        : MongoRepository<Account>,
          IAccountRepository
    {
        public AccountRepository(IOptions<ShopeeDatabaseSettings> opts)
            : base(opts)
        {
        }

        public async Task<Account?> GetByUsernameAsync(string username) =>
            await _col
                .Find(a => a.Username == username)
                .FirstOrDefaultAsync();

        public async Task CreateOrUpdateByUsernameAsync(Account acc)
        {
            // Tìm xem có record nào cùng username không
            var filter = Builders<Account>.Filter.Eq(a => a.Username, acc.Username);
            var existing = await _col.Find(filter).FirstOrDefaultAsync();

            if (existing == null)
            {
                // Chưa có, tạo mới
                acc.CreatedAt = DateTime.UtcNow;
                await _col.InsertOneAsync(acc);
            }
            else
            {
                // Đã có, giữ nguyên Id và CreatedAt, cập nhật các field khác
                acc.Id = existing.Id;
                acc.CreatedAt = existing.CreatedAt;
                acc.LastLoginAt = DateTime.UtcNow;  // ví dụ bạn muốn cập nhật login time

                await _col.ReplaceOneAsync(
                    Builders<Account>.Filter.Eq(a => a.Id, existing.Id),
                    acc
                );
            }
        }
    }
}
