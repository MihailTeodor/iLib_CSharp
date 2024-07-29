using iLib.src.main.IDAO;
using iLib.src.main.Model;

namespace iLib.src.main.DAO
{
    public class MagazineDao(NHibernate.ISession session) : BaseDao<Magazine>(session), IMagazineDao
    {
        public IList<Magazine> FindMagazinesByIssn(string issn)
        {
            return session.QueryOver<Magazine>()
                .Where(m => m.Issn == issn)
                .List();
        }

        public long CountMagazinesByIssn(string issn)
        {
            return session.QueryOver<Magazine>()
                .Where(m => m.Issn == issn)
                .RowCountInt64();
        }
    }
}
