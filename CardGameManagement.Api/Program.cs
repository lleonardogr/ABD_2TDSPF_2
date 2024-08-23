using CardGameManagement.Api.Configuration.Routes;
using CardGameManagement.Data;
using CardGameManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

// o builder ele tem a responsabilidade de criar a aplicação
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CardGameMetadataDbContext>(options =>
{
    options.UseOracle(builder.Configuration.GetConnectionString("FiapOracleConnection"));
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => { 
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.MapGet("/", () => "Hello World!").ExcludeFromDescription();
app.MapCardEndpoints();

app.MapGet("/Set", async (CardGameMetadataDbContext dbContext) =>
{
    var sets = await dbContext.Sets.ToListAsync();
    return Results.Ok(sets);
}).Produces<List<Set>>(StatusCodes.Status200OK);

app.Run();