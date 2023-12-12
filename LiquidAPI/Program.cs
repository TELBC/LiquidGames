using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using MongoDB.Driver;

namespace LiquidAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LiquidGamesDatabase>();

        var csvFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "vgsales.csv");
        SeedDatabase(dbContext, csvFilePath);

        host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

    static void SeedDatabase(LiquidGamesDatabase liquidGamesDb, string csvFilePath)
    {
        var alreadySeeded = liquidGamesDb.Genres.Find(_ => true).Any();
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

        foreach (var record in records)
        {
            // Handling potential 'N/A' values and parsing
            if (!int.TryParse(record.Rank, NumberStyles.Any, CultureInfo.InvariantCulture, out int rank))
            {
                continue; // Skip records without a valid rank
            }

            if (!double.TryParse(record.NA_Sales, NumberStyles.Any, CultureInfo.InvariantCulture, out double naSales))
            {
                naSales = 0;
            }

            if (!double.TryParse(record.EU_Sales, NumberStyles.Any, CultureInfo.InvariantCulture, out double euSales))
            {
                euSales = 0;
            }

            if (!double.TryParse(record.JP_Sales, NumberStyles.Any, CultureInfo.InvariantCulture, out double jpSales))
            {
                jpSales = 0;
            }

            if (!double.TryParse(record.Other_Sales, NumberStyles.Any, CultureInfo.InvariantCulture,
                    out double otherSales))
            {
                otherSales = 0;
            }

            if (!double.TryParse(record.Global_Sales, NumberStyles.Any, CultureInfo.InvariantCulture,
                    out double globalSales))
            {
                globalSales = 0;
            }

            var game = new LiquidGamesDatabase.Game
            {
                Rank = rank,
                GameName = record.Name,
                Platform = record.Platform,
                ReleaseYear = int.TryParse(record.Year, out int year) ? year : 0, // Handling year separately
                Publisher = record.Publisher,
                NA_Sales = naSales,
                EU_Sales = euSales,
                JP_Sales = jpSales,
                Other_Sales = otherSales,
                Global_Sales = globalSales,
            };

            // MongoDB filter and update definitions
            var filter = Builders<LiquidGamesDatabase.Genre>.Filter.Eq(g => g.GenreName, record.Genre);
            var update = Builders<LiquidGamesDatabase.Genre>.Update.Push(g => g.Games, game);

            liquidGamesDb.Genres.FindOneAndUpdate(filter, update, new FindOneAndUpdateOptions<LiquidGamesDatabase.Genre>
            {
                IsUpsert = true
            });
        }
    }
}