using MySql.Data.MySqlClient;
using System.Data;

namespace dal;

public class LoginDal
{
    private const string connectionString = "server=db;port=3306;uid=root;pwd=rootpassword;database=app";

    public static DataTable GetAllUsers()
    {
        var dt = new DataTable();
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            using (var da = new MySqlDataAdapter(@"select * from users;", connection))
            {
                da.Fill(dt);
            }
        }

        return dt;
    }

    public static bool InsertUser(string username, string password)
    {
        try
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var cmd = new MySqlCommand(
                "INSERT INTO users (Username, Password) VALUES (@username, @password)",
                connection);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return false;
        }
    }

}
