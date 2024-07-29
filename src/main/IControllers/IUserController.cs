using iLib.src.main.DTO;
using iLib.src.main.Model;

namespace iLib.src.main.IControllers
{
    public interface IUserController
    {
        Guid AddUser(UserDTO userDTO);
        void UpdateUser(Guid id, UserDTO userDTO);
        IList<User> SearchUsers(string? email, string? name, string? surname, string? telephoneNumber, int fromIndex, int limit);
        long CountUsers(string? email, string? name, string? surname, string? telephoneNumber);
        UserDashboardDTO GetUserInfoExtended(Guid id);
    }
}
