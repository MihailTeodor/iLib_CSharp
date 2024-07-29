namespace iLib.src.main.Model
{
    public static class ModelFactory
    {
        public static Book CreateBook()
        {
            return new Book(Guid.NewGuid().ToString());
        }

        public static Magazine CreateMagazine()
        {
            return new Magazine(Guid.NewGuid().ToString());
        }

        public static MovieDVD CreateMovieDVD()
        {
            return new MovieDVD(Guid.NewGuid().ToString());
        }

        public static User CreateUser()
        {
            return new User(Guid.NewGuid().ToString());
        }

        public static Booking CreateBooking()
        {
            return new Booking(Guid.NewGuid().ToString());
        }

        public static Loan CreateLoan()
        {
            return new Loan(Guid.NewGuid().ToString());
        }
    }
}
