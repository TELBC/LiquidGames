using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MongoDB.Driver;
using System.Diagnostics;
using Bogus;
using LiquidAPI.Database.Relational;
using LiquidAPI.Relational;
using Microsoft.EntityFrameworkCore;

namespace LiquidAPI;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var env = host.Services.GetRequiredService<IWebHostEnvironment>();

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LiquidGamesDatabase>();
        var csvFilePath = Path.Combine(env.ContentRootPath, "vgsales.csv");

        //Inserts ~17000 games into the database
        Console.Write(
            "\n ---------------------------------------- \n Database Seeding Speed Test for 17000 entries \n ---------------------------------------- \n");

        // MongoDB
        Console.WriteLine("MongoDB");
        await SeedDatabaseMongoAsync(dbContext, csvFilePath);
        Console.WriteLine();

        // Postgres
        Console.WriteLine("Postgres");
        await SeedDatabasePostgresAsync(host.Services);
        Console.WriteLine();


        //Inserts ~17000 games into the database
        Console.Write(
            "\n ---------------------------------------- \n Database Reading Speed Test for 17000 entries \n ---------------------------------------- \n");


        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls("http://0.0.0.0:5000");
            });

    /// <summary>
    ///    This method will seed the database with the data from the CSV file. Used for NoSQL database.
    /// </summary>
    /// <param name="liquidGamesDb"></param>
    /// <param name="csvFilePath"></param>
    private static async Task SeedDatabaseMongoAsync(LiquidGamesDatabase liquidGamesDb, string csvFilePath)
    {
        var alreadySeeded = await liquidGamesDb.Genres.Find(_ => true).AnyAsync();
        if (alreadySeeded)
        {
            Console.WriteLine("Database already seeded.");
            return;
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };

        using var reader = new StreamReader(csvFilePath);
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<CsvGameRecord>().ToList();
        var gamesByGenre = new Dictionary<string, List<LiquidGamesDatabase.Game>>();
        
        var stopwatch = Stopwatch.StartNew();

        foreach (var record in records)
        {
            if (!int.TryParse(record.Rank, NumberStyles.Any, CultureInfo.InvariantCulture, out int rank))
            {
                continue;
            }

            double.TryParse(record.NA_Sales, NumberStyles.Any, CultureInfo.InvariantCulture, out double naSales);
            double.TryParse(record.EU_Sales, NumberStyles.Any, CultureInfo.InvariantCulture, out double euSales);
            double.TryParse(record.JP_Sales, NumberStyles.Any, CultureInfo.InvariantCulture, out double jpSales);
            double.TryParse(record.Other_Sales, NumberStyles.Any, CultureInfo.InvariantCulture, out double otherSales);
            double.TryParse(record.Global_Sales, NumberStyles.Any, CultureInfo.InvariantCulture,
                out double globalSales);
            
            var game = new LiquidGamesDatabase.Game
            {
                Rank = rank,
                GameName = record.Name,
                Platform = record.Platform,
                ReleaseYear = int.TryParse(record.Year, out int year) ? year : 0,
                Publisher = record.Publisher,
                NA_Sales = naSales,
                EU_Sales = euSales,
                JP_Sales = jpSales,
                Other_Sales = otherSales,
                Global_Sales = globalSales,
            };

            if (!gamesByGenre.ContainsKey(record.Genre))
            {
                gamesByGenre[record.Genre] = new List<LiquidGamesDatabase.Game>();
            }

            gamesByGenre[record.Genre].Add(game);
        }

        // Bulk insert
        var bulkOps = new List<WriteModel<LiquidGamesDatabase.Genre>>();
        foreach (var genre in gamesByGenre)
        {
            var filter = Builders<LiquidGamesDatabase.Genre>.Filter.Eq(g => g.GenreName, genre.Key);
            var update = Builders<LiquidGamesDatabase.Genre>.Update.PushEach(g => g.Games, genre.Value);
            var upsertOne = new UpdateOneModel<LiquidGamesDatabase.Genre>(filter, update) { IsUpsert = true };
            bulkOps.Add(upsertOne);
        }

        if (bulkOps.Any())
        {
            await liquidGamesDb.Genres.BulkWriteAsync(bulkOps);
        }

        stopwatch.Stop();
        Console.WriteLine($"Database seeding completed in {stopwatch.ElapsedMilliseconds} ms.");
    }

    /// <summary>
    ///    This method will seed the database with the data from the CSV file. Used for relational database.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="csvFilePath"></param>
    private static async Task SeedDatabasePostgresAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LiquidGamesContext>();

        await context.Database.EnsureCreatedAsync();

        if (context.Games.Any())
        {
            return;
        }

        // Genres
        var genres = new Faker<Genre>()
            .RuleFor(g => g.Name, f => f.Commerce.Department())
            .Generate(20);

        // Publishers
        var publishers = new Faker<Publisher>()
            .RuleFor(p => p.Name, f => f.Company.CompanyName())
            .Generate(20);

        // Platforms
        var platforms = new Faker<Platform>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .Generate(20);
        
        context.Genres.AddRange(genres);
        context.Publishers.AddRange(publishers);
        context.Platforms.AddRange(platforms);

        await context.SaveChangesAsync();
        
        var faker = new Faker();

        var stopwatch = Stopwatch.StartNew();
        
        for (var i = 0; i < 17000; i++)
        {
            var game = new Game
            {
                Name = faker.Commerce.ProductName(),
                ReleaseYear = faker.Random.Int(1980, 2023),
                NA_Sales = faker.Random.Double(1, 100),
                EU_Sales = faker.Random.Double(1, 100),
                JP_Sales = faker.Random.Double(1, 100),
                Other_Sales = faker.Random.Double(1, 100),
                Global_Sales = faker.Random.Double(1, 100),
                Genre = genres[faker.Random.Int(0, 19)],
                Publisher = publishers[faker.Random.Int(0, 19)],
                Platform = platforms[faker.Random.Int(0, 19)]
            };
            await context.Games.AddAsync(game);
        }
        await context.SaveChangesAsync();
        stopwatch.Stop();
        Console.WriteLine($"Database seeding completed in {stopwatch.ElapsedMilliseconds} ms.");
    }
}