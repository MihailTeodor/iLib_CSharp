using iLib.src.main.IDAO;
using iLib.src.main.Model;

namespace iLib.src.main.DAO
{
    public class BookDao(NHibernate.ISession session) : BaseDao<Book>(session), IBookDao
    {
        public IList<Book> FindBooksByIsbn(string isbn)
        {
            return session.QueryOver<Book>()
                .Where(b => b.Isbn == isbn)
                .List();
        }

        public long CountBooksByIsbn(string isbn)
        {
            return session.QueryOver<Book>()
                .Where(b => b.Isbn == isbn)
                .RowCountInt64(); 
        }
    }
}
