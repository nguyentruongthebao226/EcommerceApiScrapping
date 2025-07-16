namespace EcommerceApiScrapingService.DTOs
{
    public class ShopeeApiOptions
    {
        public string BaseUrl { get; set; } = "";
        public int TimeoutSeconds { get; set; }
        public EndpointsOptions Endpoints { get; set; } = new();
        public class EndpointsOptions
        {
            public string GetProductList { get; set; } = "";
            public string GetProductDetail { get; set; } = "";
            public string CreateProduct { get; set; } = "";
            public string ShopInfo { get; set; } = "";
            public string GetProductListIsActive { get; set; } = "";
        }
    }

}
