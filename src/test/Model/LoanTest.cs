using Xunit;
using FluentAssertions;
using iLib.src.main.Model;

public class LoanTest
{
    private readonly User _user;
    private readonly Article _article;
    private readonly Loan _loan;

    public LoanTest()
    {
        _user = ModelFactory.CreateUser();
        _article = ModelFactory.CreateBook();
        _loan = ModelFactory.CreateLoan();
        _loan.LoaningUser = _user;
        _loan.ArticleOnLoan = _article;
    }

    [Fact]
    public void TestValidateState_WhenLoanStateIsNotActive_ThrowsArgumentException()
    {
        _loan.State = LoanState.RETURNED;
        _loan.DueDate = DateTime.Now.AddDays(1);

        Action act = () => _loan.ValidateState();
        act.Should().Throw<ArgumentException>().WithMessage("The Loan state is not ACTIVE!");
    }

    [Fact]
    public void TestValidateState_WhenLoanDueDatePassed_SetsArticleAndLoanStateAccordingly()
    {
        _loan.State = LoanState.ACTIVE;
        _loan.DueDate = DateTime.Now.AddDays(-1);

        _loan.ValidateState();

        _loan.ArticleOnLoan?.State.Should().Be(ArticleState.UNAVAILABLE);
        _loan.State.Should().Be(LoanState.OVERDUE);
    }

    [Fact]
    public void TestValidateState_WhenLoanDueDateNotPassed_DoesNotChangeState()
    {
        _loan.State = LoanState.ACTIVE;
        _loan.DueDate = DateTime.Now.AddDays(1);
        _loan.ArticleOnLoan!.State = ArticleState.ONLOAN;

        _loan.ValidateState();

        _loan.ArticleOnLoan.State.Should().Be(ArticleState.ONLOAN);
        _loan.State.Should().Be(LoanState.ACTIVE);
    }
}
