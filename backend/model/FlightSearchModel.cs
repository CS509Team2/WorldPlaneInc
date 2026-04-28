using System.Data;
using dal;

namespace model;

public class FlightSearchModel
{
    private readonly FlightSearchDal _dal;

    public FlightSearchModel(string connectionString)
    {
        _dal = new FlightSearchDal(connectionString);
    }

    public async Task<FlightSearchResult> SearchAsync(
        string departureAirport,
        string arrivalAirport,
        DateTime departureDate,
        DateTime? returnDate,
        TimeOnly? departureTimeStart,
        TimeOnly? departureTimeEnd,
        TimeOnly? arrivalTimeStart,
        TimeOnly? arrivalTimeEnd)
    {
        var result = new FlightSearchResult();

        result.OutboundItineraries = await FindItinerariesAsync(
            departureAirport, arrivalAirport, departureDate,
            departureTimeStart, departureTimeEnd,
            arrivalTimeStart, arrivalTimeEnd);

        if (returnDate.HasValue)
        {
            result.ReturnItineraries = await FindItinerariesAsync(
                arrivalAirport, departureAirport, returnDate.Value,
                departureTimeStart, departureTimeEnd,
                arrivalTimeStart, arrivalTimeEnd);
        }

        return result;
    }

    private async Task<List<Itinerary>> FindItinerariesAsync(
        string origin, string destination, DateTime date,
        TimeOnly? departTimeStart, TimeOnly? departTimeEnd,
        TimeOnly? arriveTimeStart, TimeOnly? arriveTimeEnd)
    {
        var itineraries = new List<Itinerary>();

        var firstLegFlights = await _dal.GetFlightsFromAirportAsync(ExtractAirportCode(origin), date, departTimeStart, departTimeEnd);
        var firstLegSegments = MapToSegments(firstLegFlights);

        foreach (var firstLeg in firstLegSegments)
        {
            var destinationCode = ExtractAirportCode(destination);
            var firstLegArriveCode = ExtractAirportCode(firstLeg.ArriveAirport);

            if (firstLegArriveCode == destinationCode)
            {
                if (IsWithinArrivalWindow(firstLeg.ArriveDateTime, arriveTimeStart, arriveTimeEnd))
                {
                    itineraries.Add(new Itinerary { Segments = new List<FlightSegment> { firstLeg } });
                }
                continue;
            }

            var secondLegFlights = await _dal.GetFlightsFromAirportAfterAsync(
                firstLegArriveCode, firstLeg.ArriveDateTime);
            var secondLegSegments = MapToSegments(secondLegFlights);

            foreach (var secondLeg in secondLegSegments)
            {
                if (!LayoverLogic.IsValidConnection(firstLeg, secondLeg))
                    continue;

                var secondLegArriveCode = ExtractAirportCode(secondLeg.ArriveAirport);

                if (secondLegArriveCode == firstLegArriveCode) continue;

                if (secondLegArriveCode == destinationCode)
                {
                    if (IsWithinArrivalWindow(secondLeg.ArriveDateTime, arriveTimeStart, arriveTimeEnd))
                    {
                        itineraries.Add(new Itinerary
                        {
                            Segments = new List<FlightSegment> { firstLeg, secondLeg }
                        });
                    }
                    continue;
                }

                var thirdLegFlights = await _dal.GetFlightsFromAirportAfterAsync(
                    secondLegArriveCode, secondLeg.ArriveDateTime);
                var thirdLegSegments = MapToSegments(thirdLegFlights);

                foreach (var thirdLeg in thirdLegSegments)
                {
                    if (!LayoverLogic.IsValidConnection(secondLeg, thirdLeg))
                        continue;

                    var thirdLegArriveCode = ExtractAirportCode(thirdLeg.ArriveAirport);

                    if (thirdLegArriveCode == firstLegArriveCode ||
                        thirdLegArriveCode == secondLegArriveCode) continue;

                    if (thirdLegArriveCode == destinationCode)
                    {
                        if (IsWithinArrivalWindow(thirdLeg.ArriveDateTime, arriveTimeStart, arriveTimeEnd))
                        {
                            itineraries.Add(new Itinerary
                            {
                                Segments = new List<FlightSegment> { firstLeg, secondLeg, thirdLeg }
                            });
                        }
                    }
                }
            }
        }

        itineraries.Sort((a, b) =>
        {
            var durationCompare = a.TotalDurationMinutes.CompareTo(b.TotalDurationMinutes);
            return durationCompare != 0 ? durationCompare : a.Stops.CompareTo(b.Stops);
        });

        return itineraries;
    }

    /// <summary>
    /// Extracts the IATA code from airport strings like "Atlanta (ATL)" -> "ATL".
    /// If the input is already a plain code (e.g. "ATL"), returns it as-is.
    /// </summary>
    private static string ExtractAirportCode(string airport)
    {
        var openParen = airport.LastIndexOf('(');
        var closeParen = airport.LastIndexOf(')');

        if (openParen >= 0 && closeParen > openParen)
            return airport.Substring(openParen + 1, closeParen - openParen - 1).Trim().ToUpper();

        return airport.Trim().ToUpper();
    }

    private static bool IsWithinArrivalWindow(DateTime arriveDateTime,
        TimeOnly? arriveTimeStart, TimeOnly? arriveTimeEnd)
    {
        if (!arriveTimeStart.HasValue && !arriveTimeEnd.HasValue)
            return true;

        var arriveTime = TimeOnly.FromDateTime(arriveDateTime);

        if (arriveTimeStart.HasValue && arriveTime < arriveTimeStart.Value)
            return false;
        if (arriveTimeEnd.HasValue && arriveTime > arriveTimeEnd.Value)
            return false;

        return true;
    }

    private static List<FlightSegment> MapToSegments(DataTable dt)
    {
        var segments = new List<FlightSegment>();

        foreach (DataRow row in dt.Rows)
        {
            segments.Add(new FlightSegment
            {
                Id = Convert.ToInt32(row["Id"]),
                DepartDateTime = Convert.ToDateTime(row["DepartDateTime"]),
                ArriveDateTime = Convert.ToDateTime(row["ArriveDateTime"]),
                DepartAirport = row["DepartAirport"]?.ToString() ?? "",
                ArriveAirport = row["ArriveAirport"]?.ToString() ?? "",
                FlightNumber = row["FlightNumber"]?.ToString() ?? "",
                Airline = row["Airline"]?.ToString() ?? "",
                DepartTimeZoneId = AirportTimeZoneResolver.GetTimeZoneId(ExtractAirportCode(row["DepartAirport"].ToString() ?? "")),
                ArriveTimeZoneId = AirportTimeZoneResolver.GetTimeZoneId(ExtractAirportCode(row["ArriveAirport"].ToString() ?? ""))
            });
        }

        return segments;
    }
}
