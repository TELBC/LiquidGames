namespace LiquidAPI.Relational;

public class Publisher
{
    public int PublisherId { get; set; }
    public string Name { get; set; }

    public List<Game> Games { get; set; } // Navigation property
}
