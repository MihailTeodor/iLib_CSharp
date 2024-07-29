namespace iLib.src.main.Exceptions
{
    public class UserDoesNotExistException : Exception
    {
        public UserDoesNotExistException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public UserDoesNotExistException(string message)
            : base(message)
        {
        }
    }
}
