using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<IEnumerable<LiquidGamesDatabase.Game>>> GetGames()
    {
        var games = _liquidGamesDb.Games.Take(100).ToList();
        return Ok(games);
    }

    /// <summary>
    /// Gets a specific genre by name.
    /// </summary>
    /// <param name="gameName"></param>
    /// <returns></returns>
    [HttpGet("{gameName}")]
    public async Task<ActionResult<LiquidGamesDatabase.Game>> GetGame(string gameName)
    {
        var game = _liquidGamesDb.Games.FirstOrDefault(g => g.GameName == gameName);
        if (game == null)
        {
            return NotFound();
        }

        return Ok(game);
    }
}