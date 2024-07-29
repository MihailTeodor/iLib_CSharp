using System.ComponentModel.DataAnnotations;

namespace iLib.src.main.DTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "The password must be at least 6 characters long but shorter than 20 characters!")]
        public string? Password { get; set; }
    }
}
