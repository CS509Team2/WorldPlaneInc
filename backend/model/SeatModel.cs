using System.Data;
using dal;

namespace model;

public class SeatInfo
{
    public int Id { get; set; }
    public string FlightNumber { get; set; } = "";
    public string Airline { get; set; } = "";
    public string SeatNumber { get; set; } = "";
    public string SeatClass { get; set; } = "Economy";
    public bool IsAvailable { get; set; } = true;
    public decimal Price { get; set; }
}

public class SeatModel
{
    private readonly SeatDal _dal;

    public SeatModel(string connectionString)
    {
        _dal = new SeatDal(connectionString);
    }

    public async Task<List<SeatInfo>> GetSeatMapAsync(string flightNumber, string airline)
    {
        await _dal.EnsureSeatsExistAsync(flightNumber, airline);
        var dt = await _dal.GetSeatsForFlightAsync(flightNumber, airline);

        var seats = new List<SeatInfo>();
        foreach (DataRow row in dt.Rows)
        {
            seats.Add(new SeatInfo
            {
                Id = Convert.ToInt32(row["Id"]),
                FlightNumber = row["FlightNumber"]?.ToString() ?? "",
                Airline = row["Airline"]?.ToString() ?? "",
                SeatNumber = row["SeatNumber"]?.ToString() ?? "",
                SeatClass = row["SeatClass"]?.ToString() ?? "Economy",
                IsAvailable = Convert.ToBoolean(row["IsAvailable"]),
                Price = Convert.ToDecimal(row["Price"])
            });
        }

        return seats;
    }

    public async Task<bool> ReserveSeatAsync(string flightNumber, string airline, string seatNumber, string username)
    {
        return await _dal.BookSeatAsync(flightNumber, airline, seatNumber, username);
    }
}
