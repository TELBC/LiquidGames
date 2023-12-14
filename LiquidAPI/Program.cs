using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MongoDB.Driver;
using System.Diagnostics;
using Bogus;
using LiquidAPI.Database.Relational;
using LiquidAPI.Relational;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

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
            "\n ---------------------------------------- \n Database Seeding Benchmark \n ---------------------------------------- \n");

        Console.WriteLine("Both databases will be seeded with 17000 entries.");
        
        // MongoDB
        Console.WriteLine("MongoDB");
        await SeedDatabaseMongoAsync(dbContext, csvFilePath);
        Console.WriteLine();

        // Postgres
        Console.WriteLine("Postgres");
        await SeedDatabasePostgresAsync(host.Services);
        Console.WriteLine("---------------------------------------- \n");
        

        //Read games from the database
        Console.Write(
            "\n ---------------------------------------- \n Read Delete Benchmark \n ---------------------------------------- \n");

        // MongoDB
        Console.WriteLine("MongoDB");
        await ReadOperationsMongoAsync(dbContext, 2000);
        Console.WriteLine();
        
        // Postgres
        Console.WriteLine("Postgres");
        await ReadOperationsPostgresAsync(host.Services.GetRequiredService<LiquidGamesContext>(), 2000);
        Console.WriteLine("---------------------------------------- \n");
        

        //Update games from the database
        Console.Write(
            "\n ---------------------------------------- \n Database Update Benchmark \n ---------------------------------------- \n");

        // MongoDB
        Console.WriteLine("MongoDB");
        await UpdateOperationsMongoAsync(dbContext, 2000);
        Console.WriteLine();
        
        // Postgres
        Console.WriteLine("Postgres");
        await UpdateOperationsPostgresAsync(host.Services.GetRequiredService<LiquidGamesContext>(), 2000);
        Console.WriteLine("---------------------------------------- \n");
        
        
        //Delete games from the database
        Console.Write(
            "\n ---------------------------------------- \n Database Delete Benchmark \n ---------------------------------------- \n");

        // MongoDB
        Console.WriteLine("MongoDB");
        await DeleteOperationsMongoAsync(dbContext, 2000);
        Console.WriteLine();
        
        // Postgres
        Console.WriteLine("Postgres");
        await DeleteOperationsPostgresAsync(host.Services.GetRequiredService<LiquidGamesContext>(), 2000);
        Console.WriteLine("---------------------------------------- \n");


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
            Console.WriteLine("MongoDB already seeded.");
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
    private static async Task SeedDatabasePostgresAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LiquidGamesContext>();

        await context.Database.EnsureCreatedAsync();

        if (context.Games.Any())
        {
            Console.WriteLine("Postgres already seeded.");
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
    
    /// <summary>
    ///   This method will read all games from the database. Used for NoSQL database.
    /// </summary>
    /// <param name="liquidGamesDb"></param>
    /// <param name="operationsCount"></param>
    private static async Task UpdateOperationsMongoAsync(LiquidGamesDatabase liquidGamesDb, int operationsCount)
    {
        Console.WriteLine($"Updating {operationsCount} games.");
        var stopwatch = Stopwatch.StartNew();

        for (var i = 0; i < operationsCount; i++)
        {
            var genreFilter = Builders<LiquidGamesDatabase.Genre>.Filter.Eq(g => g.GenreName, $"Genre {i}");
            var gameFilter = Builders<LiquidGamesDatabase.Game>.Filter.Eq(g => g.GameName, $"Game to Update {i}");
            var update = Builders<LiquidGamesDatabase.Genre>.Update.Set("Games.$[game].NA_Sales", 200);
            var arrayFilters = new List<ArrayFilterDefinition> { new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("game", gameFilter.ToBsonDocument())) };
            await liquidGamesDb.Genres.UpdateOneAsync(genreFilter, update, new UpdateOptions { ArrayFilters = arrayFilters });
        }

        stopwatch.Stop();
        Console.WriteLine($"{operationsCount} update operations completed in {stopwatch.ElapsedMilliseconds} ms.");
    }

    
    /// <summary>
    ///  This method will read all games from the database. Used for relational database.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="operationsCount"></param>
    private static async Task UpdateOperationsPostgresAsync(LiquidGamesContext context, int operationsCount)
    {
        Console.WriteLine($"Updating {operationsCount} games.");
        var stopwatch = Stopwatch.StartNew();

        for (var i = 0; i < operationsCount; i++)
        {
            var game = context.Games.FirstOrDefault(g => g.Name == $"Game to Update {i}");
            if (game == null) continue;
            game.NA_Sales = 200;
            context.Games.Update(game);
        }

        await context.SaveChangesAsync();

        stopwatch.Stop();
        Console.WriteLine($"{operationsCount} update operations completed in {stopwatch.ElapsedMilliseconds} ms.");
    }
    
    private static async Task DeleteOperationsMongoAsync(LiquidGamesDatabase liquidGamesDb, int operationsCount)
    {
        Console.WriteLine($"Deleting {operationsCount} games.");
        var stopwatch = Stopwatch.StartNew();

        for (var i = 0; i < operationsCount; i++)
        {
            var genreFilter = Builders<LiquidGamesDatabase.Genre>.Filter.Eq(g => g.GenreName, $"Genre {i}");
            var gameFilter = Builders<LiquidGamesDatabase.Game>.Filter.Eq(g => g.GameName, $"Game to Delete {i}");
            var update = Builders<LiquidGamesDatabase.Genre>.Update.PullFilter(g => g.Games, gameFilter);
            await liquidGamesDb.Genres.UpdateOneAsync(genreFilter, update);
        }

        stopwatch.Stop();
        Console.WriteLine($"{operationsCount} delete operations completed in {stopwatch.ElapsedMilliseconds} ms.");
    }

    private static async Task DeleteOperationsPostgresAsync(LiquidGamesContext context, int operationsCount)
    {
        Console.WriteLine($"Deleting {operationsCount} games.");
        var stopwatch = Stopwatch.StartNew();

        for (var i = 0; i < operationsCount; i++)
        {
            var game = context.Games.FirstOrDefault(g => g.Name == $"Game to Delete {i}");
            if (game != null)
            {
                context.Games.Remove(game);
            }
        }

        await context.SaveChangesAsync();

        stopwatch.Stop();
        Console.WriteLine($"{operationsCount} delete operations completed in {stopwatch.ElapsedMilliseconds} ms.");
    }

    private static async Task ReadOperationsMongoAsync(LiquidGamesDatabase liquidGamesDb, int operationsCount)
    {
        var stopwatch = Stopwatch.StartNew();

        // Find all without filter
        await liquidGamesDb.Genres.Find(_ => true).ToListAsync();

        // Find with filter
        var genreFilter = Builders<LiquidGamesDatabase.Genre>.Filter.Eq(g => g.GenreName, $"Genre {operationsCount}");
        await liquidGamesDb.Genres.Find(genreFilter).ToListAsync();

        // Find with filter and projection
        var projection = Builders<LiquidGamesDatabase.Genre>.Projection.Include(g => g.GenreName);
        await liquidGamesDb.Genres.Find(genreFilter).Project(projection).ToListAsync();

        // Find with filter, projection and sorting
        var sort = Builders<LiquidGamesDatabase.Genre>.Sort.Ascending(g => g.GenreName);
        await liquidGamesDb.Genres.Find(genreFilter).Project(projection).Sort(sort).ToListAsync();

        stopwatch.Stop();
        Console.WriteLine($"{operationsCount} read operations completed in {stopwatch.ElapsedMilliseconds} ms.");
    }
    
    private static async Task ReadOperationsPostgresAsync(LiquidGamesContext context, int operationsCount)
    {
        var stopwatch = Stopwatch.StartNew();

        // Find all without filter
        await context.Games.ToListAsync();

        // Find with filter
        var gameFilter = context.Games.Where(g => g.Name == $"Game to Read {operationsCount}");
        await gameFilter.ToListAsync();

        // Find with filter and projection
        var gameFilterAndProjection = gameFilter.Select(g => new { g.Name, g.ReleaseYear });
        await gameFilterAndProjection.ToListAsync();

        // Find with filter, projection and sorting
        var gameFilterProjectionSort = gameFilterAndProjection.OrderBy(g => g.Name);
        await gameFilterProjectionSort.ToListAsync();

        stopwatch.Stop();
        Console.WriteLine($"{operationsCount} read operations completed in {stopwatch.ElapsedMilliseconds} ms.");
    }


}