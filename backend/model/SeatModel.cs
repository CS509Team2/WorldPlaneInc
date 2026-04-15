using System.Data;
using dal;

namespace model;

public class SeatInfo
{
    public int Id { get; set; }
    public int FlightId { get; set; }
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

    public async Task<List<SeatInfo>> GetSeatMapAsync(int flightId, string airline)
    {
        await _dal.EnsureSeatsExistAsync(flightId, airline);
        var dt = await _dal.GetSeatsForFlightAsync(flightId, airline);

        var seats = new List<SeatInfo>();
        foreach (DataRow row in dt.Rows)
        {
            seats.Add(new SeatInfo
            {
                Id = Convert.ToInt32(row["Id"]),
                FlightId = Convert.ToInt32(row["FlightId"]),
                Airline = row["Airline"]?.ToString() ?? "",
                SeatNumber = row["SeatNumber"]?.ToString() ?? "",
                SeatClass = row["SeatClass"]?.ToString() ?? "Economy",
                IsAvailable = Convert.ToBoolean(row["IsAvailable"]),
                Price = Convert.ToDecimal(row["Price"])
            });
        }

        return seats;
    }

    public async Task<bool> ReserveSeatAsync(int flightId, string airline, string seatNumber, string username)
    {
        return await _dal.BookSeatAsync(flightId, airline, seatNumber, username);
    }

    public class Reservation
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string FlightNumber { get; set; } = "";
        public string Airline { get; set; } = "";
        public string Seat { get; set; } = "";
        public string BookedAt { get; set; } = "";
    }

    public async Task<List<Reservation>> GetUserReservation(string username)
    {

        var dt = await _dal.GetUserReservation(username);

        var reservations = new List<Reservation>();
        foreach (DataRow row in dt.Rows)
        {
            reservations.Add(new Reservation
            {
                Id = Convert.ToInt32(row["Id"]),
                Username = row["Username"]?.ToString() ?? "",
                FlightNumber = row["FlightNumber"]?.ToString() ?? "",
                Airline = row["Airline"]?.ToString() ?? "",
                Seat = row["SeatNumber"]?.ToString() ?? "",
                BookedAt = row["BookedAt"]?.ToString() ?? ""
            });
        }

        return reservations;
    }
}
