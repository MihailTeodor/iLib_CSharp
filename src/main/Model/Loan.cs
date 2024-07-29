namespace iLib.src.main.Model
{
    public class Loan : BaseEntity
    {
        public virtual Article? ArticleOnLoan { get; set; }
        public virtual User? LoaningUser { get; set; }
        public virtual DateTime LoanDate { get; set; }
        public virtual DateTime DueDate { get; set; }
        public virtual bool Renewed { get; set; }
        public virtual LoanState State { get; set; }

        public Loan() { }

        public Loan(string uuid) : base(uuid) { }

        public virtual void ValidateState()
        {
            if (State == LoanState.ACTIVE)
            {
                if (DueDate < DateTime.Now)
                {
                    if (ArticleOnLoan != null)
                    {
                        ArticleOnLoan.State = ArticleState.UNAVAILABLE;
                    }
                    State = LoanState.OVERDUE;
                }
            }
            else
            {
                throw new ArgumentException("The Loan state is not ACTIVE!");
            }
        }
    }
}
