using Microsoft.EntityFrameworkCore;
using Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(
  options => 
  {
    options.SuppressAsyncSuffixInActionNames = false;
  }
);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDbContext<RinhaContext>(options =>
  options.UseNpgsql(builder.Configuration.GetConnectionString("RinhaContext")));

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
