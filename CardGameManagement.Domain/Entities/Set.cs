using System.ComponentModel.DataAnnotations;

namespace CardGameManagement.Domain.Entities;

public class Set
{
    [Key]
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTime ReleaseDate { get; set; }
    public ICollection<Card> Cards { get; set; }
}