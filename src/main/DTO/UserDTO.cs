using System.ComponentModel.DataAnnotations;
using iLib.src.main.Model;
using iLib.src.main.Utils;

namespace iLib.src.main.DTO
{
    public class UserDTO
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        public string? PlainPassword { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Surname is required")]
        public string? Surname { get; set; }

        public string? Address { get; set; }

        [Required(ErrorMessage = "Telephone number is required")]
        [StringLength(10, ErrorMessage = "The Telephone Number must be 10 characters long", MinimumLength = 10)]
        public string? TelephoneNumber { get; set; }

        public UserDTO() {}

        public UserDTO(User user)
        {
            Id = user.Id;
            Email = user.Email;
            PlainPassword = null;
            Name = user.Name;
            Surname = user.Surname;
            Address = user.Address;
            TelephoneNumber = user.TelephoneNumber;
        }

        public User ToEntity()
        {
            var user = ModelFactory.CreateUser();

            if (!string.IsNullOrWhiteSpace(PlainPassword))
            {
                user.Password = PasswordUtils.HashPassword(PlainPassword);
            }

            user.Email = Email;
            user.Name = Name;
            user.Surname = Surname;
            user.Address = Address;
            user.TelephoneNumber = TelephoneNumber;

            return user;
        }
    }
}
