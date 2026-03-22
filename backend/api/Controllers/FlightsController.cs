using Microsoft.AspNetCore.Mvc;
using dal;
using model;
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

    [HttpPost("search")]
    public IActionResult Search([FromBody] FlightSearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DepartureAirport))
            return BadRequest("DepartureAirport is required.");

        if (string.IsNullOrWhiteSpace(request.ArrivalAirport))
            return BadRequest("ArrivalAirport is required.");

        if (request.DepartureAirport.Trim().Equals(request.ArrivalAirport.Trim(), StringComparison.OrdinalIgnoreCase))
            return BadRequest("DepartureAirport and ArrivalAirport must be different.");

        if (request.DepartureDate == default)
            return BadRequest("DepartureDate is required.");

        if (request.ReturnDate.HasValue && request.ReturnDate.Value < request.DepartureDate)
            return BadRequest("ReturnDate must be on or after DepartureDate.");

        if (request.DepartureTimeStart.HasValue && request.DepartureTimeEnd.HasValue
            && request.DepartureTimeStart.Value > request.DepartureTimeEnd.Value)
            return BadRequest("DepartureTimeStart must be before DepartureTimeEnd.");

        if (request.ArrivalTimeStart.HasValue && request.ArrivalTimeEnd.HasValue
            && request.ArrivalTimeStart.Value > request.ArrivalTimeEnd.Value)
            return BadRequest("ArrivalTimeStart must be before ArrivalTimeEnd.");

        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            return StatusCode(500, "Database connection string is missing.");

        var searchModel = new FlightSearchModel(connectionString);

        var result = searchModel.Search(
            request.DepartureAirport.Trim(),
            request.ArrivalAirport.Trim(),
            request.DepartureDate,
            request.ReturnDate,
            request.DepartureTimeStart,
            request.DepartureTimeEnd,
            request.ArrivalTimeStart,
            request.ArrivalTimeEnd);

        return Ok(result);
    }
}