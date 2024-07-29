using iLib.src.main.Model;

namespace iLib.src.main.IDAO
{
    public interface IMagazineDao : IBaseDao<Magazine>
    {
        IList<Magazine> FindMagazinesByIssn(string issn);
        long CountMagazinesByIssn(string issn);
    }
}
