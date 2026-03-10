using MySql.Data.MySqlClient;
using System.Data;

namespace dal;

///<summary>
///Handles database queries related to flights.
///</summary>
public class FlightQueryDal
{
    private readonly string _connectionString;

    public FlightQueryDal(string connectionString)
    {
        _connectionString = connectionString;
    }

    ///<summary>
    ///Retrieves a random set of flights from the Delta and Southwest tables.
    ///</summary>
    public DataTable GetRandomFlights(int count = 10)
    {
        var dt = new DataTable();

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

        using var adapter = new MySqlDataAdapter(command);
        adapter.Fill(dt);

        return dt;
    }
}