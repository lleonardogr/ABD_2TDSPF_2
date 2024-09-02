using System.Threading.RateLimiting;
using CardGameManagement.Api.Configuration;
using CardGameManagement.Api.Configuration.Routes;
using CardGameManagement.Data;
using CardGameManagement.Domain.Entities;
using IdempotentAPI.Cache.DistributedCache.Extensions.DependencyInjection;
using IdempotentAPI.Core;
using IdempotentAPI.Extensions.DependencyInjection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

// o builder ele tem a responsabilidade de criar a aplicação
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CardGameMetadataDbContext>(options =>
{
    options.UseOracle(builder.Configuration.GetConnectionString("FiapOracleConnection"));
});

builder.Services.Configure<CardGameRateLimitOptions>(
    builder.Configuration.GetSection(CardGameRateLimitOptions.CardGameRateLimit));

var rateLimitOptions = new CardGameRateLimitOptions();
builder.Configuration.GetSection(CardGameRateLimitOptions.CardGameRateLimit).Bind(rateLimitOptions);
var slidingPolicy = "sliding";

builder.Services.AddIdempotentMinimalAPI(new IdempotencyOptions());
builder.Services.AddDistributedMemoryCache();
builder.Services.AddIdempotentAPIUsingDistributedCache();

builder.Services.AddRateLimiter(options => {
    options.AddSlidingWindowLimiter(slidingPolicy, op =>
    {
        op.PermitLimit = rateLimitOptions.PermitLimit;
        op.Window = TimeSpan.FromSeconds(rateLimitOptions.Window);
        op.SegmentsPerWindow = rateLimitOptions.SegmentsPerWindow;
        op.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        op.QueueLimit = rateLimitOptions.QueueLimit;
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
     options.SwaggerDoc("v1", new OpenApiInfo
     {
            Version = "v1",
            Title = "Card Game Management API",
            Description = "Uma API para gerenciamento de cartas de jogos",
            TermsOfService = new Uri("https://example.com/terms"),
            Contact = new OpenApiContact
            {
                Name = "Example Contact",
                Url = new Uri("https://example.com/contact")
            },
            License = new OpenApiLicense
            {
                Name = "Example License",
                Url = new Uri("https://example.com/license")
            }
     });
});

// o app é a aplicação em si, ele compila o builder, define o ambiente e as rotas
var app = builder.Build();

app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => { 
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.MapGet("/test", () => "Hello World!").RequireRateLimiting(slidingPolicy);//.ExcludeFromDescription();
app.MapCardEndpoints();

app.MapGet("/Set", async (CardGameMetadataDbContext dbContext) =>
{
    var sets = await dbContext.Sets.ToListAsync();
    return Results.Ok(sets);
}).Produces<List<Set>>(StatusCodes.Status200OK);

app.Run();