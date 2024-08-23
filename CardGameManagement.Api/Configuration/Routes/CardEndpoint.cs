using CardGameManagement.Api.Models;
using CardGameManagement.Api.Services;
using CardGameManagement.Domain.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CardGameManagement.Api.Configuration.Routes;

public static class CardEndpoint
{
    public static void MapCardEndpoints(this WebApplication app)
    {
        var cards = new List<Card>()
        {
            new(){Id = 1, Name = "Card 1"},
            new(){Id = 2, Name = "Card 2"}
        };

        #region Queries
        
        app.MapGet("/cards", () => cards).WithName("Buscar cards").WithTags("card")
            .WithOpenApi(operation => new (operation)
            {
                OperationId = "GetCards",
                Summary = "Retorna todos os cards",
                Description = "Retorna todos os cards",
                Deprecated = true
            }).Produces<List<Card>>().ExcludeFromDescription();

        app.MapGet("/cards/search", (int page, int pagesize) =>
        {
            var skip = (page - 1) * pagesize;
            var results = cards.Skip(skip).Take(pagesize).ToList();
            return new CardListModel() { Page = page, PageSize = pagesize, Total = results.Count, 
                Cards = results.Select(c => 
                    new CardSummaryModel(){Id = c.Id, Name = c.Name, Description = c.Description, ImageUrl = c.ImageUrl}).ToList()};
        });

        app.MapGet("/cards/{id:int}", (int id) =>
            {
                return cards.FirstOrDefault(c => c.Id == id) is { } card ? Results.Ok(card) : Results.NotFound();
            }).WithTags("card")
            .Produces<Card>()
            .Produces(StatusCodes.Status404NotFound);
        
        #endregion

        #region  Commands
      
        app.MapPost("/cards", (CardAddOrUpdateModel model) =>
            {
                cards.Add(model.MapToCard());
            })
            .Accepts<Card>("application/json")
            .WithTags("card").WithName(" Add card").WithOpenApi();

        app.MapPut("/cards/{id:int}", (int id, Card card) =>
        {
            var cardToUpdate = cards.FirstOrDefault(c => c.Id == id);
    
            if (cardToUpdate == null) 
                return;
    
            cardToUpdate = card;
            cardToUpdate.Id = id;
        }).WithTags("card").WithName("Atualizar card").WithOpenApi(generatedOperation =>
        {
            var parameter = generatedOperation.Parameters[0];
            parameter.Description = "Id do card a ser atualizado";
            parameter.Required = true;
            return generatedOperation;
        });
        
        #endregion
    }
}