using iLib.src.main.Model;

namespace iLib.src.main.IDAO
{
    public interface IMovieDVDDao : IBaseDao<MovieDVD>
    {
        IList<MovieDVD> FindMoviesByIsan(string isan);
        long CountMoviesByIsan(string isan);
    }
}
