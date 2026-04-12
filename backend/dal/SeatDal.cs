using MySql.Data.MySqlClient;
using System.Data;

namespace dal;

public class SeatDal
{
    private readonly string _connectionString;

    public SeatDal(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<DataTable> GetSeatsForFlightAsync(string flightNumber, string airline)
    {
        var dt = new DataTable();

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            SELECT Id, FlightNumber, Airline, SeatNumber, SeatClass, IsAvailable, Price
            FROM seats
            WHERE FlightNumber = @flightNumber AND Airline = @airline
            ORDER BY SeatNumber;
        ";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@flightNumber", flightNumber);
        command.Parameters.AddWithValue("@airline", airline);

        using var adapter = new MySqlDataAdapter(command);
        adapter.Fill(dt);

        return dt;
    }

    /// <summary>
    /// Auto-generates seats for a flight if none exist yet.
    /// Standard narrow-body: rows 1-30, columns A-F.
    /// Rows 1-2 = First, 3-6 = Business, 7-30 = Economy.
    /// </summary>
    public async Task EnsureSeatsExistAsync(string flightNumber, string airline)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        using var checkCmd = new MySqlCommand(
            "SELECT COUNT(*) FROM seats WHERE FlightNumber = @fn AND Airline = @al", connection);
        checkCmd.Parameters.AddWithValue("@fn", flightNumber);
        checkCmd.Parameters.AddWithValue("@al", airline);
        var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

        if (count > 0) return;

        var columns = new[] { 'A', 'B', 'C', 'D', 'E', 'F' };
        var rng = new Random();

        using var transaction = await connection.BeginTransactionAsync();
        try
        {
            for (int row = 1; row <= 30; row++)
            {
                foreach (var col in columns)
                {
                    var seatNumber = $"{row}{col}";
                    string seatClass;
                    decimal price;

                    if (row <= 2) { seatClass = "First"; price = 450.00m + rng.Next(0, 200); }
                    else if (row <= 6) { seatClass = "Business"; price = 250.00m + rng.Next(0, 150); }
                    else { seatClass = "Economy"; price = 80.00m + rng.Next(0, 120); }

                    bool available = rng.NextDouble() > 0.2;

                    using var cmd = new MySqlCommand(@"
                        INSERT IGNORE INTO seats (FlightNumber, Airline, SeatNumber, SeatClass, IsAvailable, Price)
                        VALUES (@fn, @al, @sn, @sc, @av, @pr)", connection, (MySqlTransaction)transaction);
                    cmd.Parameters.AddWithValue("@fn", flightNumber);
                    cmd.Parameters.AddWithValue("@al", airline);
                    cmd.Parameters.AddWithValue("@sn", seatNumber);
                    cmd.Parameters.AddWithValue("@sc", seatClass);
                    cmd.Parameters.AddWithValue("@av", available);
                    cmd.Parameters.AddWithValue("@pr", price);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> BookSeatAsync(string flightNumber, string airline, string seatNumber, string username)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();
        try
        {
            using var checkCmd = new MySqlCommand(@"
                SELECT IsAvailable FROM seats
                WHERE FlightNumber = @fn AND Airline = @al AND SeatNumber = @sn
                FOR UPDATE", connection, (MySqlTransaction)transaction);
            checkCmd.Parameters.AddWithValue("@fn", flightNumber);
            checkCmd.Parameters.AddWithValue("@al", airline);
            checkCmd.Parameters.AddWithValue("@sn", seatNumber);

            var result = await checkCmd.ExecuteScalarAsync();
            if (result == null || !Convert.ToBoolean(result))
            {
                await transaction.RollbackAsync();
                return false;
            }

            using var updateCmd = new MySqlCommand(@"
                UPDATE seats SET IsAvailable = FALSE
                WHERE FlightNumber = @fn AND Airline = @al AND SeatNumber = @sn",
                connection, (MySqlTransaction)transaction);
            updateCmd.Parameters.AddWithValue("@fn", flightNumber);
            updateCmd.Parameters.AddWithValue("@al", airline);
            updateCmd.Parameters.AddWithValue("@sn", seatNumber);
            await updateCmd.ExecuteNonQueryAsync();

            using var bookCmd = new MySqlCommand(@"
                INSERT INTO bookings (Username, FlightNumber, Airline, SeatNumber)
                VALUES (@user, @fn, @al, @sn)",
                connection, (MySqlTransaction)transaction);
            bookCmd.Parameters.AddWithValue("@user", username);
            bookCmd.Parameters.AddWithValue("@fn", flightNumber);
            bookCmd.Parameters.AddWithValue("@al", airline);
            bookCmd.Parameters.AddWithValue("@sn", seatNumber);
            await bookCmd.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
