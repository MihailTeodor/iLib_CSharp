namespace iLib.src.main.Model
{
    public class Booking : BaseEntity
    {
        public virtual Article? BookedArticle { get; set; }
        public virtual User? BookingUser { get; set; }
        public virtual DateTime BookingDate { get; set; }
        public virtual DateTime BookingEndDate { get; set; }
        public virtual BookingState State { get; set; }

        public Booking() { }

            public Booking(string uuid) : base(uuid) { }

        public virtual void ValidateState()
        {
            if (State == BookingState.ACTIVE)
            {
                if (BookingEndDate < DateTime.Now)
                {
                    if (BookedArticle != null)
                    {
                        BookedArticle.State = ArticleState.AVAILABLE;
                    }
                    State = BookingState.CANCELLED;
                }
            }
            else
            {
                throw new ArgumentException("The Booking state is not ACTIVE!");
            }
        }
    }
}
