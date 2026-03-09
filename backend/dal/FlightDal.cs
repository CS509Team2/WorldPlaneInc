using MySql.Data.MySqlClient;
using model;

namespace dal;

public class FlightDal
{
    private readonly string _connectionString;

    public FlightDal(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Flight> GetRandomFlights(int count = 10)
    {
        var flights = new List<Flight>();

        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        var query = @"
            SELECT Id, DepartDateTime, ArriveDateTime, DepartAirport, ArriveAirport, FlightNumber, 'Delta' AS Airline
            FROM deltas
            UNION ALL
            SELECT Id, DepartDateTime, ArriveDateTime, DepartAirport, ArriveAirport, FlightNumber, 'Southwest' AS Airline
            FROM southwests
            ORDER BY RAND()
            LIMIT @count;
        ";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@count", count);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            flights.Add(new Flight
            {
                Id = reader.GetInt32("Id"),
                DepartDateTime = reader.GetDateTime("DepartDateTime"),
                ArriveDateTime = reader.GetDateTime("ArriveDateTime"),
                DepartAirport = reader.GetString("DepartAirport"),
                ArriveAirport = reader.GetString("ArriveAirport"),
                FlightNumber = reader.GetString("FlightNumber"),
                Airline = reader.GetString("Airline")
            });
        }

        return flights;
    }
}