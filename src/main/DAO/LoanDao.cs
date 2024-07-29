using iLib.src.main.IDAO;
using iLib.src.main.Model;
using NHibernate;
using NHibernate.Criterion;

namespace iLib.src.main.DAO
{
    public class LoanDao(NHibernate.ISession session) : BaseDao<Loan>(session), ILoanDao
    {
        public User? GetUserFromLoan(Loan loan)
        {
            var result = session.QueryOver<Loan>()
                .Where(l => l.Id == loan.Id)
                .Fetch(SelectMode.Fetch, l => l.LoaningUser) // Eager fetching LoaningUser
                .SingleOrDefault();

            return result?.LoaningUser;
        }

        public Article? GetArticleFromLoan(Loan loan)
        {
            var result = session.QueryOver<Loan>()
                .Where(l => l.Id == loan.Id)
                .Fetch(SelectMode.Fetch, l => l.ArticleOnLoan) // Eager fetching LoaningUser
                .SingleOrDefault();

            return result?.ArticleOnLoan;
        }

        public IList<Loan> SearchLoans(User? loaningUser, Article? articleOnLoan, int fromIndex, int limit)
        {
            var query = session.QueryOver<Loan>();

            if (loaningUser != null)
            {
                query.Where(l => l.LoaningUser == loaningUser);
            }

            if (articleOnLoan != null)
            {
                query.Where(l => l.ArticleOnLoan == articleOnLoan);
            }

            query.OrderBy(l => l.State).Asc
                 .ThenBy(l => l.DueDate).Desc
                 .Skip(fromIndex)
                 .Take(limit);

            return query.List<Loan>();
        }

        public long CountLoans(User? loaningUser, Article? articleOnLoan)
        {
            var query = session.QueryOver<Loan>();

            if (loaningUser != null)
            {
                query.Where(l => l.LoaningUser == loaningUser);
            }

            if (articleOnLoan != null)
            {
                query.Where(l => l.ArticleOnLoan == articleOnLoan);
            }

            return query.Select(Projections.RowCount()).SingleOrDefault<int>();
        }
    }
}
