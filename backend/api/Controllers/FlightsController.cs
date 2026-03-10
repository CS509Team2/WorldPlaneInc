using Microsoft.AspNetCore.Mvc;
using dal;
using System.Data;

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

    ///<summary>
    ///Returns a random set of upcoming flights from the database.
    ///</summary>
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

        var flightDal = new FlightQueryDal(connectionString);
        var dt = flightDal.GetRandomFlights(count);

        var flights = new List<FlightRecord>();

        foreach (DataRow row in dt.Rows)
        {
            flights.Add(new FlightRecord
            {
                Id = Convert.ToInt32(row["Id"]),
                DepartDateTime = Convert.ToDateTime(row["DepartDateTime"]),
                ArriveDateTime = Convert.ToDateTime(row["ArriveDateTime"]),
                DepartAirport = row["DepartAirport"]?.ToString() ?? "",
                ArriveAirport = row["ArriveAirport"]?.ToString() ?? "",
                FlightNumber = row["FlightNumber"]?.ToString() ?? "",
                Airline = row["Airline"]?.ToString() ?? ""
            });
        }

        return Ok(flights);
    }
}