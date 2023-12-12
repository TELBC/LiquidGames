using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiquidAPI;
using MongoDB.Bson;

namespace LiquidAPI.Controllers;

/// <summary>
/// Controller for the /Genres endpoint.
/// </summary>
[ApiController]
[Route("[controller]")]
public class GenresController : ControllerBase
{
    private readonly LiquidGamesDatabase _liquidGamesDb;

    public GenresController(LiquidGamesDatabase liquidGamesDb)
    {
        _liquidGamesDb = liquidGamesDb;
    }

    /// <summary>
    /// Adds a new genre to the database.
    /// </summary>
    /// <param name="genre"></param>
    /// <returns></returns>
    [HttpPost("add")]
    public async Task<IActionResult> AddGenre([FromBody] LiquidGamesDatabase.Genre genre)
    {
        await _liquidGamesDb.Genres.InsertOneAsync(genre);
        return Ok(genre);
    }

    /// <summary>
    /// Adds a new game to the specified genre.
    /// </summary>
    /// <param name="genreName"></param>
    /// <param name="game"></param>
    /// <returns></returns>
    [HttpPost("addGame/{genreName}")]
    public async Task<IActionResult> AddGame(string genreName, [FromBody] LiquidGamesDatabase.Game game)
    {
        var filter = Builders<LiquidGamesDatabase.Genre>.Filter.Eq(g => g.GenreName, genreName);
        var update = Builders<LiquidGamesDatabase.Genre>.Update.Push(g => g.Games, game);
        await _liquidGamesDb.Genres.UpdateOneAsync(filter, update);
        return Ok(game);
    }

    /// <summary>
    /// Gets all genres in the database.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LiquidGamesDatabase.Genre>>> GetGenres()
    {
        var genres = await _liquidGamesDb.Genres.Find(_ => true).ToListAsync();
        return Ok(genres);
    }

    /// <summary>
    /// Gets a specific genre by name.
    /// </summary>
    /// <param name="genreName"></param>
    /// <returns></returns>
    [HttpGet("{genreName}")]
    public async Task<ActionResult<LiquidGamesDatabase.Genre>> GetGenre(string genreName)
    {
        var genre = await _liquidGamesDb.Genres.Find(g => g.GenreName == genreName).FirstOrDefaultAsync();
        if (genre == null)
        {
            return NotFound();
        }

        return Ok(genre);
    }

    /// <summary>
    /// Gets a specific game by name.
    /// </summary>
    /// <param name="genreId"></param>
    /// <param name="updatedGenre"></param>
    /// <returns></returns>
    [HttpPut("update/{genreId}")]
    public async Task<IActionResult> UpdateGenre(string genreId, [FromBody] LiquidGamesDatabase.Genre updatedGenre)
    {
        var filter = Builders<LiquidGamesDatabase.Genre>.Filter.Eq("_id", new ObjectId(genreId));
        var result = await _liquidGamesDb.Genres.ReplaceOneAsync(filter, updatedGenre);

        if (result.IsAcknowledged && result.ModifiedCount > 0)
            return Ok(updatedGenre);
        else
            return NotFound();
    }
    
    /// <summary>
    /// Deletes a specific genre by name.
    /// </summary>
    /// <param name="genreId"></param>
    /// <returns></returns>
    [HttpDelete("{genreId}")]
    public async Task<IActionResult> DeleteGenre(string genreId)
    {
        var filter = Builders<LiquidGamesDatabase.Genre>.Filter.Eq("_id", new ObjectId(genreId));
        var result = await _liquidGamesDb.Genres.DeleteOneAsync(filter);

        if (result.IsAcknowledged && result.DeletedCount > 0)
            return Ok();
        else
            return NotFound();
    }
}