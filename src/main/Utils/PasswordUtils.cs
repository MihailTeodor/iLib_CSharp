namespace iLib.src.main.Utils
{
    public static class PasswordUtils
    {
        public static string HashPassword(string plainTextPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plainTextPassword);
        }
    }
}
