namespace iLib.src.main.Exceptions
{
    public class LoanDoesNotExistException : Exception
    {
        public LoanDoesNotExistException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public LoanDoesNotExistException(string message)
            : base(message)
        {
        }
    }
}
