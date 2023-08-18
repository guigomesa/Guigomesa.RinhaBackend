using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
using Data;
using Services;
using Hangfire;
using Hangfire.Redis.StackExchange;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(
  options =>
  {
    options.SuppressAsyncSuffixInActionNames = false;
  }
);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var redisHost = builder.Configuration["redisHost"];

if (!string.IsNullOrEmpty(redisHost))
{
  builder.Services.AddStackExchangeRedisCache(options =>
    {
      options.Configuration = redisHost; // Configure the Redis server connection
      options.InstanceName = "Rinha2023:"; // Prefix for cache keys to avoid conflicts
    });

  // Adiciona o código para configurar o Hangfire com Redis
  var redis = ConnectionMultiplexer.Connect(redisHost);
  builder.Services.AddHangfire(configuration =>
  {
      configuration.UseRedisStorage(redis);
  });

  builder.Services.AddSingleton<CacheMemoria>();
}

builder.Services.AddDbContext<RinhaContext>(options =>
  options.UseNpgsql(builder.Configuration.GetConnectionString("RinhaContext")));

builder.Services.AddScoped<Processamento>();

// Add this code to the ConfigureServices method:
builder.Services.AddHangfireServer(options =>
{
  options.Queues = new[] { "default", "critical" };
});

var app = builder.Build();


// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

app.UseAuthorization();

// Remove this line: app.UseHangfireServer(); 
app.MapControllers();

app.Run();
