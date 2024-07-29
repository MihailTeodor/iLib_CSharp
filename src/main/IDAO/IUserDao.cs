using iLib.src.main.Model;

namespace iLib.src.main.IDAO
{
    public interface IUserDao : IBaseDao<User>
    {
        User FindUserByEmail(string? email);
        IList<User> FindUsers(string? name, string? surname, string? telephoneNumber, int fromIndex, int limit);
        long CountUsers(string? name, string? surname, string? telephoneNumber);
    }
}
