using NHibernate.Criterion;
using iLib.src.main.Model;
using iLib.src.main.IDAO;

namespace iLib.src.main.DAO
{
    public class ArticleDao(NHibernate.ISession session) : BaseDao<Article>(session), IArticleDao
    {
        public IList<Article> FindArticles(string? title, string? genre, string? publisher, DateTime? yearEdition, string? author, int? issueNumber, string? director, int fromIndex, int limit)
        {
            var query = session.QueryOver<Article>()
                .Where(a =>
                    (title == null || a.Title == title) &&
                    (genre == null || a.Genre == genre) &&
                    (publisher == null || a.Publisher == publisher) &&
                    (yearEdition == null || a.YearEdition == yearEdition))
                .OrderBy(a => a.YearEdition).Desc
                .ThenBy(a => a.Title).Asc;

            if (author != null)
            {
                query.And(Restrictions.Where<Book>(b => b.Author == author));
            }
            if (issueNumber.HasValue)
            {
                query.And(Restrictions.Where<Magazine>(m => m.IssueNumber == issueNumber));
            }
            if (director != null)
            {
                query.And(Restrictions.Where<MovieDVD>(m => m.Director == director));
            }

            return query.Skip(fromIndex)
                        .Take(limit)
                        .List<Article>();
        }

        public long CountArticles(string? title, string? genre, string? publisher, DateTime? yearEdition, string? author, int? issueNumber, string? director)
        {
            var query = session.QueryOver<Article>()
                .Where(a =>
                    (title == null || a.Title == title) &&
                    (genre == null || a.Genre == genre) &&
                    (publisher == null || a.Publisher == publisher) &&
                    (yearEdition == null || a.YearEdition == yearEdition));

            if (author != null)
            {
                query.And(Restrictions.Where<Book>(b => b.Author == author));
            }
            if (issueNumber.HasValue)
            {
                query.And(Restrictions.Where<Magazine>(m => m.IssueNumber == issueNumber));
            }
            if (director != null)
            {
                query.And(Restrictions.Where<MovieDVD>(m => m.Director == director));
            }

            return query.Select(Projections.RowCount())
                        .SingleOrDefault<int>();
        }
    }
}
