namespace iLib.src.main.Model
{
    public abstract class BaseEntity
    {
        public virtual Guid Id { get; protected set; }
        public virtual string? Uuid { get; set; }

        protected BaseEntity() { }

        protected BaseEntity(string? uuid)
        {
            if (uuid == null)
            {
                throw new ArgumentException("uuid cannot be null!");
            }
            Uuid = uuid;
        }

        public override bool Equals(object? obj)
        {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            return Uuid == ((BaseEntity)obj).Uuid;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
}
