using Ark.WeiXin.Middlewares;
using Scalar.AspNetCore;
using WebApi.WeiXin;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<ISnsApiClient, SnsApiClient>();

// https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/servers/kestrel/options?view=aspnetcore-8.0#maximum-request-body-size
builder.WebHost
    .ConfigureKestrel(
        serverOptions =>
        {
            serverOptions.Limits.MaxRequestBodySize = int.MaxValue;
            serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(100);
        });

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.MapScalarApiReference(); // scalar/v1
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionMiddleware>();
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
