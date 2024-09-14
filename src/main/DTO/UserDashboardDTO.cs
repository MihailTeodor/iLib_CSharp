using iLib.src.main.Model;

namespace iLib.src.main.DTO
{
    public class UserDashboardDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? TelephoneNumber { get; set; }

        public List<BookingDTO>? Bookings { get; set; }
        public List<LoanDTO>? Loans { get; set; }

        public long TotalBookings { get; set; }
        public long TotalLoans { get; set; }

        protected UserDashboardDTO() {}

        public UserDashboardDTO(User user, List<Booking>? bookings, List<Loan>? loans, long totalBookings, long totalLoans)
        {
            Id = user.Id;
            Name = user.Name;
            Surname = user.Surname;
            Email = user.Email;
            Address = user.Address;
            TelephoneNumber = user.TelephoneNumber;

            if (bookings != null && bookings.Count > 0)
            {
                Bookings = bookings.Select(booking => new BookingDTO(booking)).ToList();
            }

            if (loans != null && loans.Count > 0)
            {
                Loans = loans.Select(loan => new LoanDTO(loan)).ToList();
            }

            TotalBookings = totalBookings;
            TotalLoans = totalLoans;
        }
    }
}
