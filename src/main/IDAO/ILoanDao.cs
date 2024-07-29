using iLib.src.main.Model;

namespace iLib.src.main.IDAO
{
    public interface ILoanDao : IBaseDao<Loan>
    {
        User? GetUserFromLoan(Loan loan);
        Article? GetArticleFromLoan(Loan loan);
        IList<Loan> SearchLoans(User? loaningUser, Article? articleOnLoan, int fromIndex, int limit);
        long CountLoans(User? loaningUser, Article? articleOnLoan);
    }
}
