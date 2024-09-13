using CardGameManagement.Api.Models;
using CardGameManagement.Api.Services;
using CardGameManagement.Domain.Entities;
using IdempotentAPI.MinimalAPI;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CardGameManagement.Api.Configuration.Routes;

public static class CardEndpoint
{
    public static void MapCardEndpoints(this WebApplication app)
    {
        var cards = new List<Card>()
        {
            new() { Id = 1, Name = "Card 1" },
            new() { Id = 2, Name = "Card 2" }
        };

        // Define o grupo de endpoints para "card"
        var cardGroup = app.MapGroup("/cards");

        #region Queries

        cardGroup.MapGet("/", () => cards)
            .WithName("Buscar cards")
            .WithOpenApi(operation => new(operation)
            {
                OperationId = "GetCards",
                Summary = "Retorna todos os cards",
                Description = "Retorna todos os cards",
                Deprecated = true
            })
            .Produces<List<Card>>();

        cardGroup.MapGet("/search", (int page, int pagesize) =>
        {
            var skip = (page - 1) * pagesize;
            var results = cards.Skip(skip).Take(pagesize).ToList();
            return new CardListModel()
            {
                Page = page,
                PageSize = pagesize,
                Total = results.Count,
                Cards = results.Select(c => new CardSummaryModel()
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl
                }).ToList()
            };
        });

        cardGroup.MapGet("/{id:int}",
                (int id) =>
                {
                    return cards.FirstOrDefault(c => c.Id == id) is { } card ? Results.Ok(card) : Results.NotFound();
                })
            .Produces<Card>()
            .Produces(StatusCodes.Status404NotFound);

        #endregion

        #region Commands

        cardGroup.MapPost("/", (CardAddOrUpdateModel model) =>
            {
                cards.Add(model.MapToCard());
                return Results.Created("/cards", cards);
            })
            .Accepts<Card>("application/json")
            .AddEndpointFilter<IdempotentAPIEndpointFilter>()
            .WithName("Add card")
            .WithOpenApi();

        cardGroup.MapPut("/{id:int}", (int id, Card card) =>
            {
                var cardToUpdate = cards.FirstOrDefault(c => c.Id == id);

                if (cardToUpdate == null)
                    return;

                cardToUpdate = card;
                cardToUpdate.Id = id;
            })
            .WithName("Atualizar card")
            .WithOpenApi(generatedOperation =>
            {
                var parameter = generatedOperation.Parameters[0];
                parameter.Description = "Id do card a ser atualizado";
                parameter.Required = true;
                return generatedOperation;
            });

        #endregion
    }
}