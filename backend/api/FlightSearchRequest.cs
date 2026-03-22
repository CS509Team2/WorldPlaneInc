namespace api;

public class FlightSearchRequest
{
    public string DepartureAirport { get; set; } = "";
    public string ArrivalAirport { get; set; } = "";
    public DateTime DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public TimeOnly? DepartureTimeStart { get; set; }
    public TimeOnly? DepartureTimeEnd { get; set; }
    public TimeOnly? ArrivalTimeStart { get; set; }
    public TimeOnly? ArrivalTimeEnd { get; set; }
}
