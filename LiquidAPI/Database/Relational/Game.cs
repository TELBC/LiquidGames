namespace LiquidAPI.Relational;

public class Game
{
    public int GameId { get; set; }
    public string Name { get; set; }
    public int ReleaseYear { get; set; }
    public double NA_Sales { get; set; }
    public double EU_Sales { get; set; }
    public double JP_Sales { get; set; }
    public double Other_Sales { get; set; }
    public double Global_Sales { get; set; }

    public int GenreId { get; set; } // Foreign key
    public Genre Genre { get; set; } // Navigation property

    public int PublisherId { get; set; } // Foreign key
    public Publisher Publisher { get; set; } // Navigation property

    public int PlatformId { get; set; } // Foreign key
    public Platform Platform { get; set; } // Navigation property
}
