using System.ComponentModel.DataAnnotations;
using iLib.src.main.Attributes;
using iLib.src.main.Model;

namespace iLib.src.main.DTO
{
    public class LoanDTO
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Article ID cannot be null")]
        public Guid ArticleId { get; set; }
        public string? ArticleTitle { get; set; }

        [Required(ErrorMessage = "Loaning user ID cannot be null")]
        public Guid LoaningUserId { get; set; }

        [Required(ErrorMessage = "Loan date cannot be null")]
        [PastOrPresent(ErrorMessage = "Loan date cannot be in the future")]
        public DateTime LoanDate { get; set; }

        [Required(ErrorMessage = "Due date cannot be null")]
        [FutureOrPresent(ErrorMessage = "Due date cannot be in the past")]
        public DateTime DueDate { get; set; }
        public bool Renewed { get; set; }
        public LoanState State { get; set; }

        public LoanDTO() {}

        public LoanDTO(Loan loan)
        {
            Id = loan.Id;
            ArticleId = loan.ArticleOnLoan?.Id ?? Guid.Empty;
            ArticleTitle = loan.ArticleOnLoan?.Title;
            LoaningUserId = loan.LoaningUser?.Id ?? Guid.Empty;
            LoanDate = loan.LoanDate;
            DueDate = loan.DueDate;
            Renewed = loan.Renewed;
            State = loan.State;
        }
    }
}
