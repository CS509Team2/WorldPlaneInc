using dal;

namespace model;

public class LoginModel
{
    private readonly LoginDal _dal;

    public LoginModel(string connectionString)
    {
        _dal = new LoginDal(connectionString);
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        return await _dal.ValidateUserAsync(username, password);
    }

    public async Task<bool> SignupAsync(string username, string password)
    {
        return await _dal.InsertUserAsync(username, password);
    }
}
