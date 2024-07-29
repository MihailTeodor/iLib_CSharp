using System.ComponentModel.DataAnnotations;
using iLib.src.main.Attributes;
using iLib.src.main.Model;

namespace iLib.src.main.DTO
{
    public class BookingDTO
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Booked Article ID is required")]
        public Guid BookedArticleId { get; set; }
        public string? BookedArticleTitle { get; set; }

        [Required(ErrorMessage = "Booking User ID is required")]
        public Guid BookingUserId { get; set; }

        [Required(ErrorMessage = "Booking date cannot be null")]
        [PastOrPresent(ErrorMessage = "Booking date cannot be in the future")]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Booking end date cannot be null")]
        public DateTime BookingEndDate { get; set; }
        public BookingState State { get; set; }

        public BookingDTO() {}

        public BookingDTO(Booking booking)
        {
            Id = booking.Id;
            BookedArticleId = booking.BookedArticle?.Id ?? Guid.Empty;
            BookedArticleTitle = booking.BookedArticle?.Title;
            BookingUserId = booking.BookingUser?.Id ?? Guid.Empty;
            BookingDate = booking.BookingDate;
            BookingEndDate = booking.BookingEndDate;
            State = booking.State;
        }
    }
}
