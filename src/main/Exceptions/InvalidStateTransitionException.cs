namespace iLib.src.main.Exceptions
{
    public class InvalidStateTransitionException : Exception
    {
        public InvalidStateTransitionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InvalidStateTransitionException(string message)
            : base(message)
        {
        }
    }
}
