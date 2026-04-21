using MySql.Data.MySqlClient;
using System.Data;

namespace dal;

public class LoginDal
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

    public async Task<(bool success, string errorCode)> UpdateUserSettingsAsync(
        string currentUsername,
        string newUsername,
        string? currentPassword,
        string? newPassword)
    {
        var wantsPasswordChange = !string.IsNullOrWhiteSpace(newPassword);
        var usernameChanged = !string.Equals(currentUsername, newUsername, StringComparison.OrdinalIgnoreCase);

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            string? existingPassword;
            using (var userCmd = new MySqlCommand(
                "SELECT Password FROM users WHERE Username = @currentUsername FOR UPDATE",
                connection,
                (MySqlTransaction)transaction))
            {
                userCmd.Parameters.AddWithValue("@currentUsername", currentUsername);
                existingPassword = (await userCmd.ExecuteScalarAsync()) as string;
            }

            if (existingPassword is null)
            {
                await transaction.RollbackAsync();
                return (false, "user_not_found");
            }

            if (wantsPasswordChange && (string.IsNullOrWhiteSpace(currentPassword) || currentPassword != existingPassword))
            {
                await transaction.RollbackAsync();
                return (false, "invalid_credentials");
            }

            var finalPassword = wantsPasswordChange ? newPassword! : existingPassword;

            if (usernameChanged)
            {
                using (var existsCmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM users WHERE Username = @newUsername",
                    connection,
                    (MySqlTransaction)transaction))
                {
                    existsCmd.Parameters.AddWithValue("@newUsername", newUsername);

                    if (Convert.ToInt32(await existsCmd.ExecuteScalarAsync()) > 0)
                    {
                        await transaction.RollbackAsync();
                        return (false, "username_taken");
                    }
                }

                using (var insertCmd = new MySqlCommand(
                    "INSERT INTO users (Username, Password) VALUES (@newUsername, @finalPassword)",
                    connection,
                    (MySqlTransaction)transaction))
                {
                    insertCmd.Parameters.AddWithValue("@newUsername", newUsername);
                    insertCmd.Parameters.AddWithValue("@finalPassword", finalPassword);
                    await insertCmd.ExecuteNonQueryAsync();
                }

                using (var bookingsCmd = new MySqlCommand(
                    "UPDATE bookings SET Username = @newUsername WHERE Username = @currentUsername",
                    connection,
                    (MySqlTransaction)transaction))
                {
                    bookingsCmd.Parameters.AddWithValue("@newUsername", newUsername);
                    bookingsCmd.Parameters.AddWithValue("@currentUsername", currentUsername);
                    await bookingsCmd.ExecuteNonQueryAsync();
                }

                using (var deleteCmd = new MySqlCommand(
                    "DELETE FROM users WHERE Username = @currentUsername",
                    connection,
                    (MySqlTransaction)transaction))
                {
                    deleteCmd.Parameters.AddWithValue("@currentUsername", currentUsername);
                    await deleteCmd.ExecuteNonQueryAsync();
                }
            }
            else if (wantsPasswordChange)
            {
                using var updatePasswordCmd = new MySqlCommand(
                    "UPDATE users SET Password = @newPassword WHERE Username = @currentUsername",
                    connection,
                    (MySqlTransaction)transaction);
                updatePasswordCmd.Parameters.AddWithValue("@newPassword", newPassword);
                updatePasswordCmd.Parameters.AddWithValue("@currentUsername", currentUsername);
                await updatePasswordCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return (true, "");
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            await transaction.RollbackAsync();
            return (false, "username_taken");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
