namespace iLib.src.main.Exceptions
{
    public class BookingDoesNotExistException : Exception
    {
        public BookingDoesNotExistException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public BookingDoesNotExistException(string message)
            : base(message)
        {
        }
    }
}
