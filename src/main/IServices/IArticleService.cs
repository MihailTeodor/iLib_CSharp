using iLib.src.main.DTO;
using iLib.src.main.Model;

namespace iLib.src.main.IServices
{
    public interface IArticleService
    {
        Guid AddArticle(ArticleDTO articleDTO);
        void UpdateArticle(Guid id, ArticleDTO articleDTO);
        IList<Article> SearchArticles(string? isbn, string? issn, string? isan, string? title, string? genre,
            string? publisher, DateTime? yearEdition, string? author, int? issueNumber, string? director, int fromIndex, int limit);
        long CountArticles(string? isbn, string? issn, string? isan, string? title, string? genre, string? publisher,
            DateTime? yearEdition, string? author, int? issueNumber, string? director);
        void RemoveArticle(Guid articleId);
        ArticleDTO? GetArticleInfoExtended(Guid articleId);
    }
}
