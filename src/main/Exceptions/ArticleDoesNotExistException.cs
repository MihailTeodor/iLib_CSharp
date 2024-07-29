namespace iLib.src.main.Exceptions
{
    public class ArticleDoesNotExistException : Exception
    {
        public ArticleDoesNotExistException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ArticleDoesNotExistException(string message)
            : base(message)
        {
        }
    }
}
