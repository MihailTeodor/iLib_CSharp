namespace iLib.src.main.Exceptions
{
    public class SearchHasGivenNoResultsException : Exception
    {
        public SearchHasGivenNoResultsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public SearchHasGivenNoResultsException(string message)
            : base(message)
        {
        }
    }
}
