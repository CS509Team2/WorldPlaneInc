using MySql.Data.MySqlClient;
using System.Data;

namespace dal;

public class FlightQueryDal
{
    private readonly string _connectionString;

    public FlightQueryDal(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<DataTable> GetRandomFlightsAsync(int count = 10)
    {
        var dt = new DataTable();

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        // Uses subquery-based random offset instead of ORDER BY RAND()
        // to avoid a full table scan on large datasets.
        var query = @"
            SELECT * FROM (
                SELECT Id, DepartDateTime, ArriveDateTime, DepartAirport, ArriveAirport, FlightNumber, 'Delta' AS Airline
                FROM deltas
                WHERE Id >= (SELECT FLOOR(RAND() * (SELECT MAX(Id) FROM deltas)))
                LIMIT @count
            ) AS d
            UNION ALL
            SELECT * FROM (
                SELECT Id, DepartDateTime, ArriveDateTime, DepartAirport, ArriveAirport, FlightNumber, 'Southwest' AS Airline
                FROM southwests
                WHERE Id >= (SELECT FLOOR(RAND() * (SELECT MAX(Id) FROM southwests)))
                LIMIT @count
            ) AS s
            LIMIT @totalCount;
        ";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@count", count);
        command.Parameters.AddWithValue("@totalCount", count);

        using var adapter = new MySqlDataAdapter(command);
        adapter.Fill(dt);

        return dt;
    }
}
