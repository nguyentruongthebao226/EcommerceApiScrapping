using EcommerceApiScrapingService.Configurations;
using EcommerceApiScrapingService.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EcommerceApiScrapingService.Repositories
{
    public interface IProductClonedRepository : IRepository<ProductCloned>
    {
        // thêm method đặc thù nếu cần, ví dụ:
        Task CreateProductCloned(AccountToken accountToken, string productId);
        Task<ProductCloned> GetByUsername(string username);
    }
    public class ProductClonedRepository
    : MongoRepository<ProductCloned>, IProductClonedRepository
    {
        public ProductClonedRepository(IOptions<ShopeeDatabaseSettings> opts)
        : base(opts)
        {
        }

        public async Task<ProductCloned> GetByUsername(string username)
        {
            var filter = Builders<ProductCloned>.Filter.Eq(t => t.Username, username);
            var existing = await _col.Find(filter).FirstOrDefaultAsync();
            return existing;
        }

        public async Task CreateProductCloned(AccountToken accountToken, string productId)
        {
            // 1) Tìm xem đã có token cho username này chưa
            var filter = Builders<ProductCloned>.Filter.Eq(t => t.Username, accountToken.Username);
            var existing = await _col.Find(filter).FirstOrDefaultAsync();

            if (existing == null)
            {

                existing = new ProductCloned
                {
                    Username = accountToken.Username,
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow,
                };
                await _col.InsertOneAsync(existing);
            }
        }
    }
}