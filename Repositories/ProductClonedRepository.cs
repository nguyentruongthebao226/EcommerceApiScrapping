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
        Task<List<ProductCloned>> GetByUsername(string username);
        Task<ProductCloned> GetByUsernameAndProductId(string username, string productId);
    }
    public class ProductClonedRepository
    : MongoRepository<ProductCloned>, IProductClonedRepository
    {
        public ProductClonedRepository(IOptions<ShopeeDatabaseSettings> opts)
        : base(opts)
        {
        }

        public async Task<List<ProductCloned>> GetByUsername(string username)
        {
            var filter = Builders<ProductCloned>.Filter.Eq(t => t.Username, username);
            var existing = await _col.Find(filter).ToListAsync();
            return existing;
        }

        public async Task<ProductCloned> GetByUsernameAndProductId(string username, string productId)
        {
            var filter = Builders<ProductCloned>.Filter.And(
                Builders<ProductCloned>.Filter.Eq(t => t.Username, username),
                Builders<ProductCloned>.Filter.Eq(t => t.ProductId, productId)
            );
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