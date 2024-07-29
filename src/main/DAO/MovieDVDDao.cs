using iLib.src.main.IDAO;
using iLib.src.main.Model;

namespace iLib.src.main.DAO
{
    public class MovieDVDDao(NHibernate.ISession session) : BaseDao<MovieDVD>(session), IMovieDVDDao
    {
        public IList<MovieDVD> FindMoviesByIsan(string isan)
        {
            return session.QueryOver<MovieDVD>()
                .Where(m => m.Isan == isan)
                .List();
        }

        public long CountMoviesByIsan(string isan)
        {
            return session.QueryOver<MovieDVD>()
                .Where(m => m.Isan == isan)
                .RowCountInt64();
        }
    }
}
