using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace LiquidAPI.Controllers;

/// <summary>
/// Controller for the /Games endpoint.
/// </summary>
[ApiController]
[Route("[controller]")]
public class GamesController : ControllerBase
{
    private readonly LiquidGamesDatabase _liquidGamesDb;

    public GamesController(LiquidGamesDatabase liquidGamesDb)
    {
        _liquidGamesDb = liquidGamesDb;
    }
    
    /// <summary>
    /// Gets all games in the database.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LiquidGamesDatabase.Game>>> GetGames(OrderBy orderBy, OrderType orderType, int page = 1, int pageSize = 100)
    {
        var games = _liquidGamesDb.Games.AsQueryable();

        switch (orderType)
        {
            case OrderType.Asc:
                switch (orderBy)
                {
                    case OrderBy.Year:
                        games = games.OrderBy(g => g.ReleaseYear);
                        break;
                    case OrderBy.Global_Sales:
                        games = games.OrderBy(g => g.Global_Sales);
                        break;
                    case OrderBy.EU_Sales:
                        games = games.OrderBy(g => g.EU_Sales);
                        break;
                    case OrderBy.NA_Sales:
                        games = games.OrderBy(g => g.NA_Sales);
                        break;
                    case OrderBy.JP_Sales:
                        games = games.OrderBy(g => g.JP_Sales);
                        break;
                    case OrderBy.Other_Sales:
                        games = games.OrderBy(g => g.Other_Sales);
                        break;
                }
                break;
            case OrderType.Desc:
                switch (orderBy)
                {
                    case OrderBy.Year:
                        games = games.OrderByDescending(g => g.ReleaseYear);
                        break;
                    case OrderBy.Global_Sales:
                        games = games.OrderByDescending(g => g.Global_Sales);
                        break;
                    case OrderBy.EU_Sales:
                        games = games.OrderByDescending(g => g.EU_Sales);
                        break;
                    case OrderBy.NA_Sales:
                        games = games.OrderByDescending(g => g.NA_Sales);
                        break;
                    case OrderBy.JP_Sales:
                        games = games.OrderByDescending(g => g.JP_Sales);
                        break;
                    case OrderBy.Other_Sales:
                        games = games.OrderByDescending(g => g.Other_Sales);
                        break;
                }
                break;
        }

        games = games.Skip((page - 1) * pageSize).Take(pageSize);

        return Ok(games);
    }


    [HttpGet("similar/{gameName}")]
    public async Task<ActionResult<IEnumerable<LiquidGamesDatabase.Game>>> GetSimilarGames(string gameName, OrderBy orderBy, OrderType orderType)
    {
        var games = from g in _liquidGamesDb.Games
            select g;

        if (!String.IsNullOrEmpty(gameName))
        {
            games = _liquidGamesDb.Games.Where(g => g.GameName.Contains(gameName,StringComparison.OrdinalIgnoreCase));
        }
        switch (orderType)
        {
            case OrderType.Asc:
                switch (orderBy)
                {
                    case OrderBy.Year:
                        games = games.OrderBy(g => g.ReleaseYear);
                        break;
                    case OrderBy.Global_Sales:
                        games = games.OrderBy(g => g.Global_Sales);
                        break;
                    case OrderBy.EU_Sales:
                        games = games.OrderBy(g => g.EU_Sales);
                        break;
                    case OrderBy.NA_Sales:
                        games = games.OrderBy(g => g.NA_Sales);
                        break;
                    case OrderBy.JP_Sales:
                        games = games.OrderBy(g => g.JP_Sales);
                        break;
                    case OrderBy.Other_Sales:
                        games = games.OrderBy(g => g.Other_Sales);
                        break;
                }
                break;
            case OrderType.Desc:
                switch (orderBy)
                {
                    case OrderBy.Year:
                        games = games.OrderByDescending(g => g.ReleaseYear);
                        break;
                    case OrderBy.Global_Sales:
                        games = games.OrderByDescending(g => g.Global_Sales);
                        break;
                    case OrderBy.EU_Sales:
                        games = games.OrderByDescending(g => g.EU_Sales);
                        break;
                    case OrderBy.NA_Sales:
                        games = games.OrderByDescending(g => g.NA_Sales);
                        break;
                    case OrderBy.JP_Sales:
                        games = games.OrderByDescending(g => g.JP_Sales);
                        break;
                    case OrderBy.Other_Sales:
                        games = games.OrderByDescending(g => g.Other_Sales);
                        break;
                }
                break;
        }

        return Ok(games.ToList());
    }
    
    /// <summary>
    /// Deletes a specific game by name.
    /// </summary>
    /// <param name="gameName"></param>
    /// <returns></returns>
    [HttpDelete("{gameName}/{platform}")]
    public IActionResult DeleteGame(string gameName, string platform)
    {
        var gameToDelete = _liquidGamesDb.Games.FirstOrDefault(g => g.GameName == gameName && g.Platform == platform);

        if (gameToDelete == null)
        {
            return NotFound("Game not found");
        }

        var filter = Builders<LiquidGamesDatabase.Genre>.Filter.Where(genre => genre.Games.Any(g => g.GameName == gameName && g.Platform == platform));
        var update = Builders<LiquidGamesDatabase.Genre>.Update.PullFilter(genre => genre.Games, g => g.GameName == gameName && g.Platform == platform);

        _liquidGamesDb.Genres.FindOneAndUpdate(filter, update);

        return Ok($"Game '{gameName}' on '{platform}' platform has been deleted.");
    }
}