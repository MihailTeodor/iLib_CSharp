using iLib.src.main.DTO;
using iLib.src.main.Model;

namespace iLib.src.main.IControllers
{
    public interface ILoanController
    {
        Guid RegisterLoan(Guid userId, Guid articleId);
        void RegisterReturn(Guid loanId);
        LoanDTO GetLoanInfo(Guid loanId);
        IList<Loan> GetLoansByUser(Guid userId, int fromIndex, int limit);
        void ExtendLoan(Guid loanId);
        long CountLoansByUser(Guid userId);
    }
}
