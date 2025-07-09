namespace EcommerceApiScrapingService.Helpers
{
    public static class ShopeeSignHelper
    {
        public static string GetSign(string partnerKey, string baseString)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(partnerKey));
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(baseString));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
