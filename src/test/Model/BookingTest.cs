using Xunit;
using FluentAssertions;
using iLib.src.main.Model;

public class BookingTest
{
    private readonly User _user;
    private readonly Article _article;
    private readonly Booking _booking;

    public BookingTest()
    {
        _user = ModelFactory.CreateUser();
        _article = ModelFactory.CreateBook();
        _booking = ModelFactory.CreateBooking();
        _booking.BookingUser = _user;
        _booking.BookedArticle = _article;
    }

    [Fact]
    public void TestValidateState_WhenBookingStateIsNotActive_ThrowsArgumentException()
    {
        _booking.State = BookingState.COMPLETED;
        _booking.BookingEndDate = DateTime.Now.AddDays(1);

        Action act = () => _booking.ValidateState();
        act.Should().Throw<ArgumentException>().WithMessage("The Booking state is not ACTIVE!");
    }

    [Fact]
    public void TestValidateState_WhenBookingEndDatePassedAndArticleStateIsBOOKED_ChangesArticleAndBookingStateAccordingly()
    {
        _booking.State = BookingState.ACTIVE;
        _booking.BookingEndDate = DateTime.Now.AddDays(-1);
        _article.State = ArticleState.BOOKED;

        _booking.ValidateState();

        _booking.BookedArticle?.State.Should().Be(ArticleState.AVAILABLE);
        _booking.State.Should().Be(BookingState.EXPIRED);
    }

    [Fact]
    public void TestValidateState_WhenBookingEndDatePassedAndArticleStateIsUNAVAILBLE_ChangesOnlyBookingStateAccordingly()
    {
        _booking.State = BookingState.ACTIVE;
        _booking.BookingEndDate = DateTime.Now.AddDays(-1);
        _article.State = ArticleState.UNAVAILABLE;

        _booking.ValidateState();

        _booking.BookedArticle?.State.Should().Be(ArticleState.UNAVAILABLE);
        _booking.State.Should().Be(BookingState.EXPIRED);
    }

    [Fact]
    public void TestValidateState_WhenBookingEndDateNotPassed_DoesNotChangeState()
    {
        _booking.State = BookingState.ACTIVE;
        _booking.BookingEndDate = DateTime.Now.AddDays(1);
        _booking.BookedArticle!.State = ArticleState.BOOKED;

        _booking.ValidateState();

        _booking.BookedArticle.State.Should().Be(ArticleState.BOOKED);
        _booking.State.Should().Be(BookingState.ACTIVE);
    }
}
