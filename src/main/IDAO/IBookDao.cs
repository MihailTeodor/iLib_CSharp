using iLib.src.main.Model;

namespace iLib.src.main.IDAO
{
    public interface IBookDao : IBaseDao<Book>
    {
        IList<Book> FindBooksByIsbn(string isbn);
        long CountBooksByIsbn(string isbn);
    }
}
