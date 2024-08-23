namespace CardGameManagement.Api.Models;

public record CardListModel()
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
    public List<CardSummaryModel> Cards { get; init; } = [];
}