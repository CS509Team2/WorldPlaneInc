using dal;

namespace model;

public class LoginModel : ILoginModel
{
    private readonly ILoginDal _dal;

    public LoginModel(ILoginDal dal)
    {
        _dal = dal;
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

public interface ILoginModel
{
    Task<bool> LoginAsync(string username, string password);
    Task<bool> SignupAsync(string username, string password);
}
