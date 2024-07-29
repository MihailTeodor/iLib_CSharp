using iLib.src.main.Model;

namespace iLib.src.main.IDAO
{
    public interface IArticleDao : IBaseDao<Article>
    {
        IList<Article> FindArticles(string? title, string? genre, string? publisher, DateTime? yearEdition, string? author, int? issueNumber, string? director, int fromIndex, int limit);
        long CountArticles(string? title, string? genre, string? publisher, DateTime? yearEdition, string? author, int? issueNumber, string? director);
    }
}
