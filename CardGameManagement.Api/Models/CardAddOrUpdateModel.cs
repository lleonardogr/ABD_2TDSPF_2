namespace CardGameManagement.Api.Models;

[Serializable]
public record CardAddOrUpdateModel()
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
};