using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayerApi.Models.Requests;
using PlayerApi.Models.Responses;
using PlayerApi.Services;

namespace PlayerApi.Controllers;

[ApiController]
[Route("api/automationTask")]
[Authorize]
public class PlayersController : ControllerBase
{
    private readonly PlayerStore _store;

    public PlayersController(PlayerStore store)
    {
        _store = store;
    }

    /// <summary>
    /// Create a new player.
    /// </summary>
    /// <param name="request">Player data.</param>
    /// <returns>Created player.</returns>
    [HttpPost("create")]
    public IActionResult Create([FromBody] CreatePlayerRequest? request)
    {
        if (request is null)
        {
            return BadRequest(new ErrorResponse("Request body is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 3 || request.Username.Length > 50)
        {
            return BadRequest(new ErrorResponse("Username must be between 3 and 50 characters."));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new ErrorResponse("Email is required."));
        }

        var emailAttribute = new EmailAddressAttribute();
        if (!emailAttribute.IsValid(request.Email))
        {
            return BadRequest(new ErrorResponse("Email must be a valid email address."));
        }

        if (_store.UsernameExists(request.Username))
        {
            return BadRequest(new ErrorResponse("Username already exists."));
        }

        if (_store.EmailExists(request.Email))
        {
            return BadRequest(new ErrorResponse("Email already exists."));
        }

        var player = _store.Add(request);
        return CreatedAtAction(nameof(GetOne), new { id = player.Id }, player);
    }

    /// <summary>
    /// Get a player by id.
    /// </summary>
    /// <param name="id">Player identifier.</param>
    /// <returns>Player data.</returns>
    [HttpGet("getOne")]
    public IActionResult GetOne([FromQuery] Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("Id is required."));
        }

        var player = _store.GetById(id);
        if (player is null)
        {
            return NotFound(new ErrorResponse("Player not found."));
        }

        return Ok(player);
    }

    /// <summary>
    /// Get all players.
    /// </summary>
    /// <returns>All players.</returns>
    [HttpGet("getAll")]
    public IActionResult GetAll()
        => Ok(_store.GetAll());

    /// <summary>
    /// Delete a player by id.
    /// </summary>
    /// <param name="id">Player identifier.</param>
    [HttpDelete("deleteOne/{id}")]
    public IActionResult DeleteOne(Guid id)
    {
        if (!_store.Delete(id))
        {
            return NotFound(new ErrorResponse("Player not found."));
        }

        return NoContent();
    }
}
