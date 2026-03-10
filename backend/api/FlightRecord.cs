namespace api;

///<summary>
///Represents a flight record returned from the database.
///</summary>
public class FlightRecord
{
    public int Id { get; set; }

    public DateTime DepartDateTime { get; set; }

    public DateTime ArriveDateTime { get; set; }

    public string DepartAirport { get; set; } = "";

    public string ArriveAirport { get; set; } = "";

    public string FlightNumber { get; set; } = "";

    public string Airline { get; set; } = "";
}