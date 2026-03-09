using Microsoft.AspNetCore.Mvc;
using dal;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class FlightsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public FlightsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("getNextFlights")]
    public IActionResult GetNextFlights([FromQuery] int count = 10)
    {
        if (count < 1 || count > 20)
        {
            return BadRequest("count must be between 1 and 20.");
        }

        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return StatusCode(500, "Database connection string is missing.");
        }

        var flightDal = new FlightDal(connectionString);
        var flights = flightDal.GetRandomFlights(count);

        return Ok(flights);
    }
}