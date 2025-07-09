using EcommerceApiScrapingService.Configurations;
using EcommerceApiScrapingService.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EcommerceApiScrapingService.Services
{
    public class AccountService
    {
        private readonly IMongoCollection<Account> _accounts;

        public AccountService(IOptions<ShopeeDatabaseSettings> dbSettings)
        {
            var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
            var database = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
            _accounts = database.GetCollection<Account>(CollectionNames.Account); // Dùng constant ở đây
        }

        public List<Account> Get() => _accounts.Find(acc => true).ToList();
        public Account Get(string id) => _accounts.Find(acc => acc.Id == id).FirstOrDefault();
        public Account Create(Account acc) { _accounts.InsertOne(acc); return acc; }
        public void Update(string id, Account accIn) => _accounts.ReplaceOne(acc => acc.Id == id, accIn);
        public void Remove(string id) => _accounts.DeleteOne(acc => acc.Id == id);

        public List<Account> GetActive() =>
    _accounts.Find(acc => acc.IsActive).ToList();

        public Account GetByShopId(string shopId) =>
            _accounts.Find(acc => acc.ShopId == shopId).FirstOrDefault();

        public List<Account> Search(string keyword) =>
            _accounts.Find(acc => acc.Username.ToLower().Contains(keyword.ToLower()) ||
                                  acc.ShopName.ToLower().Contains(keyword.ToLower()))
                     .ToList();

        public void SetActive(string id, bool isActive)
        {
            var update = Builders<Account>.Update.Set(a => a.IsActive, isActive);
            _accounts.UpdateOne(acc => acc.Id == id, update);
        }

        public void RefreshToken(string id, string token, string refreshToken, DateTime tokenExpiredAt)
        {
            var update = Builders<Account>.Update
                .Set(a => a.ShopeeToken, token)
                .Set(a => a.RefreshToken, refreshToken)
                .Set(a => a.TokenExpiredAt, tokenExpiredAt);
            _accounts.UpdateOne(acc => acc.Id == id, update);
        }

        public void CreateOrUpdateByShopId(Account accIn)
        {
            var acc = _accounts.Find(a => a.ShopId == accIn.ShopId).FirstOrDefault();
            if (acc == null)
            {
                _accounts.InsertOne(accIn);
            }
            else
            {
                acc.ShopeeToken = accIn.ShopeeToken;
                acc.RefreshToken = accIn.RefreshToken;
                acc.TokenExpiredAt = accIn.TokenExpiredAt;
                acc.Platform = accIn.Platform;
                acc.IsActive = true;
                acc.CreatedAt = acc.CreatedAt != DateTime.MinValue ? acc.CreatedAt : DateTime.UtcNow;
                _accounts.ReplaceOne(a => a.Id == acc.Id, acc);
            }
        }

    }
}
