using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using MongoDB.Driver;
using System.Diagnostics;

namespace LiquidAPI;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LiquidGamesDatabase>();
        var csvFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "vgsales.csv");

        var stopwatch = Stopwatch.StartNew();
        await SeedDatabaseAsync(dbContext, csvFilePath);

        stopwatch.Stop();
        Console.WriteLine($"Database seeding completed in {stopwatch.ElapsedMilliseconds} ms.");
        // Database seeding took ~882ms last time tested

        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

    private static async Task SeedDatabaseAsync(LiquidGamesDatabase liquidGamesDb, string csvFilePath)
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

        foreach (var record in records)
        {
            // Parsing logic
            if (!int.TryParse(record.Rank, NumberStyles.Any, CultureInfo.InvariantCulture, out int rank))
            {
                continue; // Skip records without a valid rank
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
            Console.WriteLine("Bulk insert operation completed.");
        }
    }
}