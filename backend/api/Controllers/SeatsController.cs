using Microsoft.AspNetCore.Mvc;
using model;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class SeatsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public SeatsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetSeats([FromQuery] string flightNumber, [FromQuery] string airline)
    {
        if (string.IsNullOrWhiteSpace(flightNumber))
            return BadRequest("flightNumber is required.");
        if (string.IsNullOrWhiteSpace(airline))
            return BadRequest("airline is required.");

        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            return StatusCode(500, "Database connection string is missing.");

        var seatModel = new SeatModel(connectionString);
        var seats = await seatModel.GetSeatMapAsync(flightNumber.Trim(), airline.Trim());

        return Ok(seats);
    }

    [HttpPost("book")]
    public async Task<IActionResult> BookSeat([FromBody] BookingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FlightNumber))
            return BadRequest(new { message = "FlightNumber is required." });
        if (string.IsNullOrWhiteSpace(request.Airline))
            return BadRequest(new { message = "Airline is required." });
        if (string.IsNullOrWhiteSpace(request.SeatNumber))
            return BadRequest(new { message = "SeatNumber is required." });
        if (string.IsNullOrWhiteSpace(request.Username))
            return BadRequest(new { message = "Username is required." });

        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            return StatusCode(500, "Database connection string is missing.");

        var seatModel = new SeatModel(connectionString);
        var success = await seatModel.ReserveSeatAsync(
            request.FlightNumber.Trim(),
            request.Airline.Trim(),
            request.SeatNumber.Trim(),
            request.Username.Trim());

        if (success)
            return Ok(new { message = "Seat booked successfully." });

        return Conflict(new { message = "Seat is no longer available." });
    }
}
