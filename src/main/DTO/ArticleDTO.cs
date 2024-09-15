using System.ComponentModel.DataAnnotations;
using iLib.src.main.Attributes;
using iLib.src.main.Model;

namespace iLib.src.main.DTO
{
    public class ArticleDTO
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Type is required")]
        public ArticleType? Type { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Year of edition is required")]
        [DataType(DataType.Date)]
        [PastOrPresent(ErrorMessage = "Year of edition cannot be in the future")]
        public DateTime? YearEdition { get; set; }

        [Required(ErrorMessage = "Publisher is required")]
        public string? Publisher { get; set; }

        [Required(ErrorMessage = "Genre is required")]
        public string? Genre { get; set; }

        public string? Description { get; set; }
        public ArticleState? State { get; set; }

        public string? Author { get; set; }
        public string? Isbn { get; set; }
        public int? IssueNumber { get; set; }
        public string? Issn { get; set; }
        public string? Director { get; set; }
        public string? Isan { get; set; }
        public BookingDTO? BookingDTO{ get; set; }
        public LoanDTO? LoanDTO{ get; set; }
        public ArticleDTO() { }

    }
}
