using MongoDB.Bson;
using MongoDB.Driver;

namespace LiquidAPI;

public class LiquidGamesDatabase
{
    public MongoClient Client { get; }
    public IMongoDatabase Db { get; }
    public IMongoCollection<Genre> Genres { get; }

    public ICollection<Game> Games => Genres.AsQueryable().SelectMany(genre => genre.Games).ToList();

    public LiquidGamesDatabase(string connectionString, string databaseName, string collectionName)
    {
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        Client = new MongoClient(settings);
        Db = Client.GetDatabase(databaseName);
        Genres = Db.GetCollection<Genre>(collectionName);
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