using MongoDB.Bson;
using MongoDB.Driver;
public class LiquidGamesDatabase
{
    public MongoClient Client { get; }
    public IMongoDatabase Db { get; }
    public IMongoCollection<Genre> Genres => Db.GetCollection<Genre>("genres");

    public LiquidGamesDatabase(string connectionString, string databaseName)
    {
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        Client = new MongoClient(settings);
        Db = Client.GetDatabase(databaseName);
    }

    public class Genre
    {
        public ObjectId Id { get; set; }
        public string GenreName { get; set; }
        public List<Game> Games { get; set; } = new List<Game>();
    }

    public class Game
    {
        public int Rank { get; set; }
        public string GameName { get; set; }
        public string Platform { get; set; }
        public int ReleaseYear { get; set; }
        public string Publisher { get; set; }
        public double NA_Sales { get; set; }
        public double EU_Sales { get; set; }
        public double JP_Sales { get; set; }
        public double Other_Sales { get; set; }
        public double Global_Sales { get; set; }
    }
}

public record CsvGameRecord
{
    public string Rank { get; set; }
    public string Name { get; set; }
    public string Platform { get; set; }
    public string Year { get; set; }
    public string Genre { get; set; }
    public string Publisher { get; set; }
    public string NA_Sales { get; set; }
    public string EU_Sales { get; set; }
    public string JP_Sales { get; set; }
    public string Other_Sales { get; set; }
    public string Global_Sales { get; set; }
}
