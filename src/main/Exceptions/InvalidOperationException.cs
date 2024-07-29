namespace iLib.src.main.Exceptions
{
    public class InvalidOperationException : Exception
    {
        public InvalidOperationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InvalidOperationException(string message)
            : base(message)
        {
        }
    }
}
