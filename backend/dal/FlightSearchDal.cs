using MySql.Data.MySqlClient;
using System.Data;

namespace dal;

public class FlightSearchDal
{
    private readonly string _connectionString;

    public FlightSearchDal(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Gets all flights departing from a given airport on a specific date,
    /// optionally filtered by a departure time window.
    /// </summary>
    public async Task<DataTable> GetFlightsFromAirportAsync(string departAirport, DateTime date,
        TimeOnly? timeStart = null, TimeOnly? timeEnd = null)
    {
        var dt = new DataTable();

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var dateFilter = "DATE(DepartDateTime) = @date";
        var airportFilter = "DepartAirport LIKE @departAirport";
        var timeFilter = BuildTimeFilter("DepartDateTime", timeStart, timeEnd);

        var query = $@"
            SELECT Id, DepartDateTime, ArriveDateTime, DepartAirport, ArriveAirport, FlightNumber, 'Delta' AS Airline
            FROM deltas
            WHERE {airportFilter} AND {dateFilter} {timeFilter}
            UNION ALL
            SELECT Id, DepartDateTime, ArriveDateTime, DepartAirport, ArriveAirport, FlightNumber, 'Southwest' AS Airline
            FROM southwests
            WHERE {airportFilter} AND {dateFilter} {timeFilter}
            ORDER BY DepartDateTime;
        ";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@departAirport", $"%({departAirport})%");
        command.Parameters.AddWithValue("@date", date.Date);

        if (timeStart.HasValue)
            command.Parameters.AddWithValue("@timeStart", timeStart.Value.ToString("HH:mm:ss"));
        if (timeEnd.HasValue)
            command.Parameters.AddWithValue("@timeEnd", timeEnd.Value.ToString("HH:mm:ss"));

        using var adapter = new MySqlDataAdapter(command);
        adapter.Fill(dt);

        return dt;
    }

    /// <summary>
    /// Gets flights departing from an airport after a specific datetime.
    /// Used to find connecting flights after a previous leg arrives.
    /// </summary>
    public async Task<DataTable> GetFlightsFromAirportAfterAsync(string departAirport, DateTime afterDateTime)
    {
        var dt = new DataTable();

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            SELECT Id, DepartDateTime, ArriveDateTime, DepartAirport, ArriveAirport, FlightNumber, 'Delta' AS Airline
            FROM deltas
            WHERE DepartAirport LIKE @departAirport AND DepartDateTime > @afterDateTime AND DATE(DepartDateTime) = DATE(@afterDateTime)
            UNION ALL
            SELECT Id, DepartDateTime, ArriveDateTime, DepartAirport, ArriveAirport, FlightNumber, 'Southwest' AS Airline
            FROM southwests
            WHERE DepartAirport LIKE @departAirport AND DepartDateTime > @afterDateTime AND DATE(DepartDateTime) = DATE(@afterDateTime)
            ORDER BY DepartDateTime;
        ";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@departAirport", $"%({departAirport})%");
        command.Parameters.AddWithValue("@afterDateTime", afterDateTime);

        using var adapter = new MySqlDataAdapter(command);
        adapter.Fill(dt);

        return dt;
    }

    private static string BuildTimeFilter(string column, TimeOnly? timeStart, TimeOnly? timeEnd)
    {
        var parts = new List<string>();

        if (timeStart.HasValue)
            parts.Add($"TIME({column}) >= @timeStart");
        if (timeEnd.HasValue)
            parts.Add($"TIME({column}) <= @timeEnd");

        return parts.Count > 0 ? " AND " + string.Join(" AND ", parts) : "";
    }
}
