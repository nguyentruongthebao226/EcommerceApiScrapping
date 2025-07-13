using EcommerceApiScrapingService.Repositories;
using System.Text.Json.Nodes;

namespace EcommerceApiScrapingService.Services
{
    public interface IProductService
    {
        Task<JsonNode> GetShopInfoAsync(string username);
        Task<JsonNode> GetProductListAsync(string username, int pageNumber, int pageSize);
        Task<JsonNode> CloneProductAsync(string username, string productId);
    }

    public class ProductService : IProductService
    {
        private readonly IAccountTokenRepository _accountTokenRepo;
        private readonly IShopeeClient _shopee;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IAccountTokenRepository accountTokenRepo,
            IShopeeClient shopee,
            ILogger<ProductService> logger)
        {
            _accountTokenRepo = accountTokenRepo;
            _shopee = shopee;
            _logger = logger;
        }

        public async Task<JsonNode> GetShopInfoAsync(string username)
        {
            var token = await _accountTokenRepo.GetByUsername(username);
            if (token == null)
                throw new ArgumentException($"Không tìm thấy token cho user '{username}'");

            _logger.LogInformation("Lấy shop-info cho {User}", username);
            var node = await _shopee.GetShopInfoAsync(token);
            return node;
        }

        public async Task<JsonNode> GetProductListAsync(string username, int pageNumber, int pageSize)
        {
            var token = await _accountTokenRepo.GetByUsername(username);
            if (token == null)
                throw new ArgumentException($"Không tìm thấy token cho user '{username}'");

            _logger.LogInformation("Lấy product-list trang {Page}", pageNumber);
            var node = await _shopee.GetProductListAsync(token, pageNumber, pageSize);
            return node;
        }

        public async Task<JsonNode> CloneProductAsync(string username, string productId)
        {
            var token = await _accountTokenRepo.GetByUsername(username);
            if (token == null)
                throw new ArgumentException($"Không tìm thấy token cho user '{username}'");

            _logger.LogInformation("Clone product {ProductId} cho {User}", productId, username);

            // 1) Lấy detail
            var detail = await _shopee.GetProductDetailAsync(token, productId);

            // 2) Map sang payload create (MapDetailToCreatePayload có thể đưa vào đây)
            var payload = MapDetailToCreatePayload(detail);

            // 3) Gửi create
            var result = await _shopee.CreateProductAsync(token, payload);

            return result;
        }

        private JsonObject MapDetailToCreatePayload(JsonNode detail)
        {
            var dest = new JsonObject();

            // 1) Các trường cơ bản
            if (detail["name"] is JsonNode name)
                dest["name"] = name.DeepClone();
            if (detail["enable_model_level_dts"] is JsonNode dts)
                dest["enable_model_level_dts"] = dts.DeepClone();
            if (detail["category_path"] is JsonArray cp)
                dest["category_path"] = cp.DeepClone();
            if (detail["weight"] is JsonObject wgt)
                dest["weight"] = wgt.DeepClone();
            if (detail["condition"] is JsonNode cond)
                dest["condition"] = cond.DeepClone();
            if (detail["parent_sku"] is JsonNode ps)
                dest["parent_sku"] = ps.DeepClone();

            // brand_id nằm trong brand_info
            if (detail["brand_info"] is JsonObject bi && bi["brand_id"] is JsonNode bid)
                dest["brand_id"] = bid.DeepClone();

            // 2) Attributes, images, long_images
            if (detail["attributes"] is JsonArray attrs)
                dest["attributes"] = attrs.DeepClone();
            if (detail["images"] is JsonArray imgs)
                dest["images"] = imgs.DeepClone();
            if (detail["long_images"] is JsonArray limgs)
                dest["long_images"] = limgs.DeepClone();

            // 3) Tier variations
            if (detail["std_tier_variation_list"] is JsonArray stdTier)
                dest["std_tier_variation_list"] = stdTier.DeepClone();

            // 4) Các trường thông tin khác
            if (detail["size_chart_info"] is JsonObject sci)
                dest["size_chart_info"] = sci.DeepClone();
            if (detail["video_list"] is JsonArray vl)
                dest["video_list"] = vl.DeepClone();
            if (detail["description_info"] is JsonObject di)
                dest["description_info"] = di.DeepClone();
            if (detail["dimension"] is JsonObject dim)
                dest["dimension"] = dim.DeepClone();
            if (detail["pre_order_info"] is JsonObject poi)
                dest["pre_order_info"] = poi.DeepClone();
            if (detail["wholesale_list"] is JsonArray wl)
                dest["wholesale_list"] = wl.DeepClone();

            // unlisted (hoặc is_unlisted tuỳ field)
            if (detail["is_unlisted"] is JsonNode un)
                dest["unlisted"] = un.DeepClone();

            // 5) Logistics channels: phải có ít nhất một channel.enabled = true
            JsonArray logisticsClone;
            if (detail["logistics_channels"] is JsonArray chans && chans.Count > 0)
            {
                logisticsClone = chans.DeepClone()!.AsArray();
                // bật channel đầu tiên nếu chưa có channel nào enabled
                if (!logisticsClone.OfType<JsonObject>().Any(c => c["enabled"]?.GetValue<bool>() == true))
                    logisticsClone.OfType<JsonObject>().First()["enabled"] = true;
            }
            else
            {
                // fallback: thêm một channel mặc định (ví dụ 5002)
                logisticsClone = new JsonArray {
                    new JsonObject {
                        ["size"] = 0,
                        ["price"] = "0",
                        ["cover_shipping_fee"] = false,
                        ["enabled"] = true,
                        ["channelid"] = 5002,
                        ["sizeid"] = 0
                    }
                };
            }
            dest["logistics_channels"] = logisticsClone;

            // 6) Build model_list theo đúng format payload mẫu
            var newModels = new JsonArray();
            if (detail["model_list"] is JsonArray modelList)
            {
                foreach (var item in modelList.OfType<JsonObject>())
                {
                    var nm = new JsonObject
                    {
                        ["id"] = 0
                    };

                    if (item["tier_index"] is JsonArray ti)
                        nm["tier_index"] = ti.DeepClone();
                    if (item["is_default"] is JsonNode df)
                        nm["is_default"] = df.DeepClone();
                    if (item["sku"] is JsonNode sku)
                        nm["sku"] = sku.DeepClone();

                    // price: lấy input_normal_price
                    if (item["price_info"] is JsonObject pi && pi["input_normal_price"] is JsonNode inp)
                        nm["price"] = inp.DeepClone();

                    // stock_setting_list: chỉ lấy sellable_stock
                    if (item["stock_detail"] is JsonObject sd &&
                        sd["seller_stock_info"] is JsonArray ssi &&
                        ssi.FirstOrDefault() is JsonObject firstSeller &&
                        firstSeller["sellable_stock"] is JsonNode ss)
                    {
                        nm["stock_setting_list"] = new JsonArray {
                            new JsonObject { ["sellable_stock"] = ss.DeepClone() }
                        };
                    }

                    newModels.Add(nm);
                }
            }
            dest["model_list"] = newModels;

            return dest;
        }
    }
}
