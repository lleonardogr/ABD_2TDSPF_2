namespace CardGameManagement.Api.Models;

public record CardAddOrUpdateModel()
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
};