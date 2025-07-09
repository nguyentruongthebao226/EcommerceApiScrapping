namespace EcommerceApiScrapingService.DTOs
{
    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public int? Total { get; set; }  // Nếu trả về list
        public ApiResponse(int statusCode, string message, T data, int? total = null)
        {
            StatusCode = statusCode;
            Message = message;
            Data = data;
            Total = total;
        }
    }
}
