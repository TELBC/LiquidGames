namespace LiquidAPI.Relational;

public class Platform
{
    public int PlatformId { get; set; }
    public string Name { get; set; }

    public List<Game> Games { get; set; } // Navigation property
}