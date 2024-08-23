using System.ComponentModel.DataAnnotations;

namespace CardGameManagement.Domain.Entities;

public class Card
{
    [Key]
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int SetId { get; set; }
    public Set Set { get; set; }
}