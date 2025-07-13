using EcommerceApiScrapingService.Configurations;
using EcommerceApiScrapingService.DTOs;
using EcommerceApiScrapingService.Repositories;
using EcommerceApiScrapingService.Services;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ShopeeDatabaseSettings>(
    builder.Configuration.GetSection("ShopeeDatabaseSettings"));

var playwright = await Playwright.CreateAsync();
var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = true,
    Args = new[]
                {
                    "--no-sandbox",                // Bỏ sandbox (chạy trên root)
                    "--disable-setuid-sandbox",
                    "--disable-dev-shm-usage",     // Tránh chia sẻ bộ nhớ /dev/shm bị đầy
                    "--disable-gpu"                // Tắt GPU nếu container không hỗ trợ
                }
});

// 1) Bind ShopeeApiOptions
builder.Services.Configure<ShopeeApiOptions>(
    builder.Configuration.GetSection("ShopeeApi"));

// 2) Đăng ký Typed HttpClient cho Shopee
builder.Services.AddHttpClient<IShopeeClient, ShopeeClient>((sp, client) =>
{
    var opt = sp.GetRequiredService<IOptions<ShopeeApiOptions>>().Value;
    client.BaseAddress = new Uri(opt.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(opt.TimeoutSeconds);
    client.DefaultRequestHeaders.Accept
          .Add(new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddTransientHttpErrorPolicy(p =>
    p.WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(2)));

builder.Services.AddSingleton(browser);
builder.Services.AddSingleton<ShopeeLoginService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services
       .AddScoped(typeof(IRepository<>), typeof(MongoRepository<>))
       .AddScoped<IAccountTokenRepository, AccountTokenRepository>();
builder.Services.Configure<ShopeeOAuthSettings>(
    builder.Configuration.GetSection("ShopeeOAuth"));
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/weatherforecast", () =>
{
    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapControllers();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
