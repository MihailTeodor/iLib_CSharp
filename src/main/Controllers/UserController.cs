using iLib.src.main.DTO;
using iLib.src.main.Exceptions;
using iLib.src.main.IControllers;
using iLib.src.main.IDAO;
using iLib.src.main.Model;
using iLib.src.main.Utils;

namespace iLib.src.main.Controllers
{
    public class UserController(IUserDao userDao, IBookingController bookingController, ILoanController loanController) : IUserController
    {
        private readonly IUserDao _userDao = userDao;
        private readonly IBookingController _bookingController = bookingController;
        private readonly ILoanController _loanController = loanController;

        public Guid AddUser(UserDTO userDTO)
        {
            User? tmpUser;
            try
            {
                tmpUser = _userDao.FindUserByEmail(userDTO.Email);
            }
            catch (Exception)
            {
                tmpUser = null;
            }
            if (tmpUser != null)
                throw new ArgumentException("Email already registered!");

            if (string.IsNullOrEmpty(userDTO.PlainPassword))
                throw new ArgumentException("Password is required!");

            var userToAdd = userDTO.ToEntity();
            userToAdd.Role = UserRole.CITIZEN;

            _userDao.Save(userToAdd);

            return userToAdd.Id;
        }

        public void UpdateUser(Guid id, UserDTO userDTO)
        {
            var userToUpdate = _userDao.FindById(id) ?? throw new UserDoesNotExistException("User does not exist!");
            if (!string.IsNullOrEmpty(userDTO.PlainPassword))
                userToUpdate.Password = PasswordUtils.HashPassword(userDTO.PlainPassword);

            userToUpdate.Email = userDTO.Email;
            userToUpdate.Name = userDTO.Name;
            userToUpdate.Surname = userDTO.Surname;
            userToUpdate.Address = userDTO.Address;
            userToUpdate.TelephoneNumber = userDTO.TelephoneNumber;

            _userDao.Save(userToUpdate);
        }

        public IList<User> SearchUsers(string? email, string? name, string? surname, string? telephoneNumber, int fromIndex, int limit)
        {
            IList<User> retrievedUsers = [];
            try
            {
                if (!string.IsNullOrEmpty(email))
                {
                    var user = _userDao.FindUserByEmail(email);
                    if (user != null)
                    {
                        retrievedUsers.Add(user);
                    }
                }
                else
                {
                    retrievedUsers = _userDao.FindUsers(name, surname, telephoneNumber, fromIndex, limit);
                }
            }
            catch (Exception)
            {
                retrievedUsers = [];
            }
            if (!retrievedUsers.Any())
                throw new SearchHasGivenNoResultsException("Search has given no results!");
            return retrievedUsers;
        }

        public long CountUsers(string? email, string? name, string? surname, string? telephoneNumber)
        {
            return string.IsNullOrEmpty(email) ? _userDao.CountUsers(name, surname, telephoneNumber) : 1;
        }

        public UserDashboardDTO GetUserInfoExtended(Guid id)
        {
            var user = _userDao.FindById(id) ?? throw new UserDoesNotExistException("User does not exist!");
            List<Booking>? bookings;
            try
            {
                bookings = _bookingController.GetBookingsByUser(user.Id, 0, 5).ToList();
            }
            catch (Exception)
            {
                bookings = null;
            }

            List<Loan>? loans;
            try
            {
                loans = _loanController.GetLoansByUser(user.Id, 0, 5).ToList();
            }
            catch (Exception)
            {
                loans = null;
            }

            return new UserDashboardDTO(user, bookings, loans);
        }
    }
}