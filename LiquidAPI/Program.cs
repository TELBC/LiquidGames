using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MongoDB.Driver;


public class Program
{
    static int Main(string[] args)
    {
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Clear();

        string connectionString = "mongodb://localhost:27017";
        string databaseName = "liquidgames";

        var liquidGamesDb = new LiquidGamesDatabase(connectionString, databaseName);

        try
        {
            string csvFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "vgsales.csv");
            SeedDatabase(liquidGamesDb, csvFilePath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An error occurred: {ex.Message}");
            return 1;
        }

        Console.WriteLine("Database seeding completed successfully.");
        return 0;
    }


    static void SeedDatabase(LiquidGamesDatabase liquidGamesDb, string csvFilePath)
    {
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