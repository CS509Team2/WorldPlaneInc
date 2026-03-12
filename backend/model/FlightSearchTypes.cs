namespace model;

public class FlightSegment
{
    public int Id { get; set; }
    public string FlightNumber { get; set; } = "";
    public string Airline { get; set; } = "";
    public string DepartAirport { get; set; } = "";
    public string ArriveAirport { get; set; } = "";
    public DateTime DepartDateTime { get; set; }
    public DateTime ArriveDateTime { get; set; }
}

public class Itinerary
{
    public List<FlightSegment> Segments { get; set; } = new();
    public int Stops => Segments.Count - 1;
    public double TotalDurationMinutes =>
        Segments.Count > 0
            ? (Segments.Last().ArriveDateTime - Segments.First().DepartDateTime).TotalMinutes
            : 0;
}

public class FlightSearchResult
{
    public List<Itinerary> OutboundItineraries { get; set; } = new();
    public List<Itinerary>? ReturnItineraries { get; set; }
}
