using CardGameManagement.Api.Models;
using CardGameManagement.Domain.Entities;

namespace CardGameManagement.Api.Services;

public static class CardService
{
    public static Card MapToCard(this CardAddOrUpdateModel model)
    {
        return new Card() { Name = model.Name, Description = model.Description, ImageUrl = model.ImageUrl };
    }
}