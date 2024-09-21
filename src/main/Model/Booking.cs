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
                    State = BookingState.EXPIRED;
                    // at this point, the state of the booked article can be only one of {UNAVAILBLE, BOOKED}
                    if (BookedArticle!.State == ArticleState.BOOKED)
                    {
                        BookedArticle.State = ArticleState.AVAILABLE;
                    }
                }
            }
            else
            {
                throw new ArgumentException("The Booking state is not ACTIVE!");
            }
        }
    }
}
