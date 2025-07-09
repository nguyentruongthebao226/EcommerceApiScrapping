using EcommerceApiScrapingService.Configurations;
using EcommerceApiScrapingService.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EcommerceApiScrapingService.Services
{
    public class AccountTokenService
    {
        private readonly IMongoCollection<AccountToken> _accountTokens;

        public AccountTokenService(IOptions<ShopeeDatabaseSettings> dbSettings)
        {
            var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
            var database = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
            // Lưu ý đổi CollectionNames.AccountToken thành tên bạn đặt cho collection này, ví dụ "account_tokens"
            _accountTokens = database.GetCollection<AccountToken>(CollectionNames.AccountToken);
        }

        public List<AccountToken> Get() => _accountTokens.Find(t => true).ToList();
        public AccountToken Get(string id) => _accountTokens.Find(t => t.Id == id).FirstOrDefault();

        public AccountToken Create(AccountToken token)
        {
            _accountTokens.InsertOne(token);
            return token;
        }

        public void Update(string id, AccountToken tokenIn) =>
            _accountTokens.ReplaceOne(t => t.Id == id, tokenIn);

        public void Remove(string id) =>
            _accountTokens.DeleteOne(t => t.Id == id);

        public AccountToken GetByUsername(string username) =>
            _accountTokens.Find(t => t.Username == username).FirstOrDefault();

        public List<AccountToken> Search(string keyword) =>
            _accountTokens.Find(t => t.Username.ToLower().Contains(keyword.ToLower()))
                          .ToList();

        public void CreateOrUpdateByUsername(AccountToken tokenIn)
        {
            var token = _accountTokens.Find(t => t.Username == tokenIn.Username).FirstOrDefault();
            if (token == null)
            {
                _accountTokens.InsertOne(tokenIn);
            }
            else
            {
                // Update các trường bạn muốn
                token.Cookie = tokenIn.Cookie;
                token.UserAgent = tokenIn.UserAgent;
                token.Csrftoken = tokenIn.Csrftoken;
                token.SPC_CDS = tokenIn.SPC_CDS;
                token.SPC_CDS_VER = tokenIn.SPC_CDS_VER;
                token.XSapSec = tokenIn.XSapSec;
                token.RawHeadersJson = tokenIn.RawHeadersJson;
                token.CreatedAt = DateTime.UtcNow;
                token.ExpiredAt = tokenIn.ExpiredAt;

                _accountTokens.ReplaceOne(t => t.Id == token.Id, token);
            }
        }
    }
}
