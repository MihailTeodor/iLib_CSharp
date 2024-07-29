namespace iLib.src.main.Model
{
    public class User : BaseEntity
    {
        public virtual string? Name { get; set; }
        public virtual string? Surname { get; set; }
        public virtual string? Email { get; set; }
        public virtual string? Password { get; set; }
        public virtual string? Address { get; set; }
        public virtual string? TelephoneNumber { get; set; }
        public virtual UserRole Role { get; set; }

        public User() { }

        public User(string uuid) : base(uuid) { }
    }
}
