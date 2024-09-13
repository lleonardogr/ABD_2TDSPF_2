using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.RateLimiting;
using CardGameManagement.Api.Configuration;
using CardGameManagement.Api.Configuration.HealthChecks;
using CardGameManagement.Api.Configuration.Routes;
using CardGameManagement.Api.Models;
using CardGameManagement.Data;
using CardGameManagement.Domain.Entities;
using HealthChecks.UI.Client;
using IdempotentAPI.Cache.DistributedCache.Extensions.DependencyInjection;
using IdempotentAPI.Core;
using IdempotentAPI.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// o builder ele tem a responsabilidade de criar a aplicação
var builder = WebApplication.CreateBuilder(args);

//TODO: Passar as regions para arquivos separados
#region  Database Services Configuration

builder.Services.AddDbContext<CardGameMetadataDbContext>(options =>
{
    options.UseOracle(builder.Configuration.GetConnectionString("FiapOracleConnection"));
});

#endregion

#region Authentication Configuration

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

#endregion

#region HealthChecks Configuration

builder.Services.ConfigureHealthChecks(builder.Configuration);

#endregion

#region RateLimiting Configuration

builder.Services.Configure<CardGameRateLimitOptions>(
    builder.Configuration.GetSection(CardGameRateLimitOptions.CardGameRateLimit));

var rateLimitOptions = new CardGameRateLimitOptions();
builder.Configuration.GetSection(CardGameRateLimitOptions.CardGameRateLimit).Bind(rateLimitOptions);
var slidingPolicy = "sliding";

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

#endregion

#region Idempotency Configuration

builder.Services.AddIdempotentMinimalAPI(new IdempotencyOptions());
builder.Services.AddDistributedMemoryCache();
builder.Services.AddIdempotentAPIUsingDistributedCache();

#endregion

#region Swagger Configuration

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
     options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
     {
         Name = "Authorization",
         Type = SecuritySchemeType.ApiKey,
         Scheme = "Bearer",
         BearerFormat = "JWT",
         In = ParameterLocation.Header,
         Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\""
     });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
});

#endregion

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

app.MapPost("/authenticate", (JwtAuthModel model) =>
{
    //generate token
    // token handler ele vai gerenciar toda a parte de um token que será gerado
    var tokenHandler = new JwtSecurityTokenHandler();
    // key é a chave
    var key = Encoding.UTF8.GetBytes(model.secretKey);
    // token descriptor é o que vai ser gerado
    var tokenDescriptor = new SecurityTokenDescriptor {
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    // token é o token gerado
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return Results.Ok(tokenHandler.WriteToken(token));
});

app.MapGet("/test", () => "Hello World!")
    .RequireRateLimiting(slidingPolicy)
    .RequireAuthorization();//.ExcludeFromDescription();

app.MapHealthChecks("/api/health", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.UseHealthChecksUI(options =>
{
    options.UIPath = "/healthcheck-ui";
    
});

app.MapCardEndpoints();

app.MapGet("/Set", async (CardGameMetadataDbContext dbContext) =>
{
    var sets = await dbContext.Sets.ToListAsync();
    return Results.Ok(sets);
}).Produces<List<Set>>(StatusCodes.Status200OK);

app.UseAuthentication();
app.UseAuthorization();

app.Run();