using iLib.src.main.DTO;
using iLib.src.main.Exceptions;
using iLib.src.main.IServices;
using iLib.src.main.IDAO;
using iLib.src.main.Model;
using iLib.src.main.Utils;

namespace iLib.src.main.Services
{
    public class UserService(IUserDao userDao, IBookingService bookingController, ILoanService loanController) : IUserService
    {
        private readonly IUserDao _userDao = userDao;
        private readonly IBookingService _bookingController = bookingController;
        private readonly ILoanService _loanController = loanController;

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

            if (string.IsNullOrWhiteSpace(userDTO.PlainPassword))
                throw new ArgumentException("Password is required!");

            var userToAdd = userDTO.ToEntity();
            userToAdd.Role = UserRole.CITIZEN;

            _userDao.Save(userToAdd);

            return userToAdd.Id;
        }

        public void UpdateUser(Guid id, UserDTO userDTO)
        {
            var userToUpdate = _userDao.FindById(id) ?? throw new UserDoesNotExistException("User does not exist!");
            if (!string.IsNullOrWhiteSpace(userDTO.PlainPassword))
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
                if (!string.IsNullOrWhiteSpace(email))
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
            return string.IsNullOrWhiteSpace(email) ? _userDao.CountUsers(name, surname, telephoneNumber) : 1;
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

            var totalBookings = _bookingController.CountBookingsByUser(user.Id);
            var totalLoans = _loanController.CountLoansByUser(user.Id);
            return new UserDashboardDTO(user, bookings, loans, totalBookings, totalLoans);
        }
    }
}
