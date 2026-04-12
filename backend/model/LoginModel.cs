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

    //Checks for an existing guest account, signing up if there is not one.
    public static bool GuestSign() {
        var dt = LoginDal.GetAllUsers();

        foreach (DataRow r in dt.Rows) {
            if ((string)r["Username"] == "guest" && (string)r["Password"] == "guestPassword") {
                return true;
            }
        }

        return Signup("guest","guestPassword");
    }
}
