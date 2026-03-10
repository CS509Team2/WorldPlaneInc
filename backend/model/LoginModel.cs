using System.Data;
using dal;

namespace model;

public static class LoginModel
{
    public static bool Login(string username, string password)
    {
        var dt = LoginDal.GetAllUsers();

        foreach (DataRow r in dt.Rows)
        {
            if ((string)r["Username"] == username && (string)r["Password"] == password)
            {
                return true;
            }
        }

        return false;
    }

    public static bool Signup(string username, string password)
    {
        return LoginDal.InsertUser(username, password);
    }
}
