using MySql.Data.MySqlClient;
using System.Data;

namespace dal;

public class LoginDal : ILoginDal
{
    private readonly string _connectionString;

    public LoginDal(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<bool> ValidateUserAsync(string username, string password)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        using var cmd = new MySqlCommand(
            "SELECT COUNT(*) FROM users WHERE Username = @u AND Password = @p", connection);
        cmd.Parameters.AddWithValue("@u", username);
        cmd.Parameters.AddWithValue("@p", password);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
    }

    public async Task<bool> InsertUserAsync(string username, string password)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var cmd = new MySqlCommand(
                "INSERT INTO users (Username, Password) VALUES (@username, @password)",
                connection);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return false;
        }
    }
}

public interface ILoginDal
{
    Task<bool> ValidateUserAsync(string username, string password);
    Task<bool> InsertUserAsync(string username, string password);
}
