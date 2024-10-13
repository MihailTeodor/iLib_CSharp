using iLib.src.main.Model;
using System.Reflection;

public static class QueryUtils
{
    public static async Task TruncateAllTables(NHibernate.ISession session)
    {
        using var transaction = session.BeginTransaction();
        await session.CreateSQLQuery("SET FOREIGN_KEY_CHECKS = 0").ExecuteUpdateAsync();
        await session.CreateSQLQuery("TRUNCATE TABLE users").ExecuteUpdateAsync();
        await session.CreateSQLQuery("TRUNCATE TABLE bookings").ExecuteUpdateAsync();
        await session.CreateSQLQuery("TRUNCATE TABLE loans").ExecuteUpdateAsync();
        await session.CreateSQLQuery("TRUNCATE TABLE books").ExecuteUpdateAsync();
        await session.CreateSQLQuery("TRUNCATE TABLE magazines").ExecuteUpdateAsync();
        await session.CreateSQLQuery("TRUNCATE TABLE movies_DVD").ExecuteUpdateAsync();
        await session.CreateSQLQuery("SET FOREIGN_KEY_CHECKS = 1").ExecuteUpdateAsync();
        await transaction.CommitAsync();
    }

private static void SetProtectedProperty(object instance, string propertyName, object value)
{
    var type = instance.GetType();
    PropertyInfo? property = null;

    while (type != null)
    {
        property = type.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        if (property != null)
        {
            break;
        }
        type = type.BaseType;
    }

    if (property == null)
    {
        throw new InvalidOperationException($"Property '{propertyName}' not found on type '{instance.GetType().FullName}' or its base types.");
    }

    property.SetValue(instance, value);
}



    public static async Task<User> CreateUser(NHibernate.ISession session, Guid id, string email, string password, string name, string surname, string address, string telephoneNumber, UserRole role)
    {
        var user = ModelFactory.CreateUser();
        SetProtectedProperty(user, "Id", id);
        user.Email = email;
        user.Password = BCrypt.Net.BCrypt.HashPassword(password);
        user.Name = name;
        user.Surname = surname;
        user.Address = address;
        user.TelephoneNumber = telephoneNumber;
        user.Role = role;

        using (var transaction = session.BeginTransaction())
        {
            await session.SaveAsync(user);
            await transaction.CommitAsync();
        }
        return user;
    }

    public static async Task<Book> CreateBook(NHibernate.ISession session, Guid id, string location, string title, DateTime yearEdition, string publisher, string genre, string description, ArticleState state, string author, string isbn)
    {
        var book = ModelFactory.CreateBook();
        SetProtectedProperty(book, "Id", id);
        book.Location = location;
        book.Title = title;
        book.YearEdition = yearEdition;
        book.Publisher = publisher;
        book.Genre = genre;
        book.Description = description;
        book.State = state;
        book.Author = author;
        book.Isbn = isbn;

        using (var transaction = session.BeginTransaction())
        {
            await session.SaveAsync(book);
            await transaction.CommitAsync();
        }
        return book;
    }

    public static async Task<Magazine> CreateMagazine(NHibernate.ISession session, Guid id, string location, string title, DateTime yearEdition, string publisher, string genre, string description, ArticleState state, int issueNumber, string issn)
    {
        var magazine = ModelFactory.CreateMagazine();
        SetProtectedProperty(magazine, "Id", id);
        magazine.Location = location;
        magazine.Title = title;
        magazine.YearEdition = yearEdition;
        magazine.Publisher = publisher;
        magazine.Genre = genre;
        magazine.Description = description;
        magazine.State = state;
        magazine.IssueNumber = issueNumber;
        magazine.Issn = issn;

        using (var transaction = session.BeginTransaction())
        {
            await session.SaveAsync(magazine);
            await transaction.CommitAsync();
        }
        return magazine;
    }

    public static async Task<MovieDVD> CreateMovieDVD(NHibernate.ISession session, Guid id, string location, string title, DateTime yearEdition, string publisher, string genre, string description, ArticleState state, string director, string isan)
    {
        var movieDVD = ModelFactory.CreateMovieDVD();
        SetProtectedProperty(movieDVD, "Id", id);
        movieDVD.Location = location;
        movieDVD.Title = title;
        movieDVD.YearEdition = yearEdition;
        movieDVD.Publisher = publisher;
        movieDVD.Genre = genre;
        movieDVD.Description = description;
        movieDVD.State = state;
        movieDVD.Director = director;
        movieDVD.Isan = isan;

        using (var transaction = session.BeginTransaction())
        {
            await session.SaveAsync(movieDVD);
            await transaction.CommitAsync();
        }
        return movieDVD;
    }

    public static async Task<Booking> CreateBooking(NHibernate.ISession session, Guid id, DateTime bookingDate, DateTime bookingEndDate, BookingState state, Article bookedArticle, User bookingUser)
    {
        var booking = ModelFactory.CreateBooking();
        SetProtectedProperty(booking, "Id", id);
        booking.BookingDate = bookingDate;
        booking.BookingEndDate = bookingEndDate;
        booking.State = state;
        booking.BookedArticle = bookedArticle;
        booking.BookingUser = bookingUser;

        using (var transaction = session.BeginTransaction())
        {
            await session.SaveAsync(booking);
            await transaction.CommitAsync();
        }
        return booking;
    }

    public static async Task<Loan> CreateLoan(NHibernate.ISession session, Guid id, DateTime loanDate, DateTime dueDate, LoanState state, bool renewed, Article articleOnLoan, User loaningUser)
    {
        var loan = ModelFactory.CreateLoan();
        SetProtectedProperty(loan, "Id", id);
        loan.LoanDate = loanDate;
        loan.DueDate = dueDate;
        loan.State = state;
        loan.Renewed = renewed;
        loan.ArticleOnLoan = articleOnLoan;
        loan.LoaningUser = loaningUser;

        using (var transaction = session.BeginTransaction())
        {
            await session.SaveAsync(loan);
            await transaction.CommitAsync();
        }
        return loan;
    }
}
