using EcommerceApiScrapingService.Configurations;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace EcommerceApiScrapingService.Repositories
{
    public class MongoRepository<T> : IRepository<T> where T : IEntity
    {
        protected readonly IMongoCollection<T> _col;

        public MongoRepository(
            IOptions<ShopeeDatabaseSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var db = client.GetDatabase(dbSettings.Value.DatabaseName);
            //var collName = typeof(T).Name.ToLower() + "s";
            var collName = typeof(T).Name;
            _col = db.GetCollection<T>(collName);
        }

        public async Task<List<T>> GetAllAsync() =>
            await _col.Find(_ => true).ToListAsync();

        public async Task<T?> GetByIdAsync(string id) =>
            await _col.Find(e => e.Id == id).FirstOrDefaultAsync();

        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> filter) =>
            await _col.Find(filter).ToListAsync();

        public async Task<T> CreateAsync(T entity)
        {
            await _col.InsertOneAsync(entity);
            return entity;
        }

        public async Task<bool> ReplaceAsync(string id, T entity) =>
            (await _col.ReplaceOneAsync(e => e.Id == id, entity))
                     .ModifiedCount > 0;

        public async Task<bool> DeleteAsync(string id) =>
            (await _col.DeleteOneAsync(e => e.Id == id))
                     .DeletedCount > 0;
    }
}
