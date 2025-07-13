using System.Text.Json.Serialization;

namespace EcommerceApiScrapingService.DTOs
{
    // Shopee trả về wrapper { code, msg, data }
    public class ShopeeResponse<T>
    {
        [JsonPropertyName("code")] public int Code { get; set; }
        [JsonPropertyName("msg")] public string Msg { get; set; }
        [JsonPropertyName("data")] public T Data { get; set; }
    }

    public class ProductDetailData
    {
        [JsonPropertyName("product_info")]
        public ShopeeProductInfo ProductInfo { get; set; }
    }

    public class ShopeeProductInfo
    {
        [JsonPropertyName("id")] public long Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("enable_model_level_dts")]
        public bool EnableModelLevelDts { get; set; }
        [JsonPropertyName("category_path")] public List<int> CategoryPath { get; set; }
        [JsonPropertyName("weight")] public WeightInfo Weight { get; set; }
        // … định nghĩa đầy đủ theo JSON bạn cần xài …
        [JsonPropertyName("model_list")]
        public List<ShopeeModel> ModelList { get; set; }
        [JsonPropertyName("logistics_channels")]
        public List<LogisticsChannel> LogisticsChannels { get; set; }
        // …
    }

    public class WeightInfo
    {
        [JsonPropertyName("unit")] public int Unit { get; set; }
        [JsonPropertyName("value")] public string Value { get; set; }
    }

    public class ShopeeModel
    {
        [JsonPropertyName("sku")] public string Sku { get; set; }
        [JsonPropertyName("tier_index")] public int[] TierIndex { get; set; }
        [JsonPropertyName("price_info")] public PriceInfo PriceInfo { get; set; }
        [JsonPropertyName("stock_detail")] public StockDetail StockDetail { get; set; }
    }

    public class PriceInfo
    {
        [JsonPropertyName("input_normal_price")]
        public string InputNormalPrice { get; set; }
    }
    public class StockDetail
    {
        [JsonPropertyName("seller_stock_info")]
        public List<SellerStock> SellerStockInfo { get; set; }
    }
    public class SellerStock
    { [JsonPropertyName("sellable_stock")] public int SellableStock { get; set; } }

    public class LogisticsChannel
    {
        [JsonPropertyName("channelid")] public int ChannelId { get; set; }
        [JsonPropertyName("price")] public string Price { get; set; }
        [JsonPropertyName("enabled")] public bool Enabled { get; set; }
        // … các trường khác nếu cần …
    }

}
