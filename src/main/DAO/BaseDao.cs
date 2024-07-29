using iLib.src.main.IDAO;
using iLib.src.main.Model;

namespace iLib.src.main.DAO
{
    public abstract class BaseDao<E>(NHibernate.ISession session) : IBaseDao<E> where E : BaseEntity
    {
        protected readonly NHibernate.ISession session = session;

        public E FindById(Guid id)
        {
            return session.Get<E>(id);
        }

        public void Save(E entity)
        {
            using var transaction = session.BeginTransaction();
            if (entity.Id != Guid.Empty)
                session.Merge(entity);
            else
                session.Save(entity);

            transaction.Commit();
        }

        public void Delete(E entity)
        {
            using var transaction = session.BeginTransaction();
            if (entity.Id != Guid.Empty)
            {
                session.Delete(entity);
            }
            else
            {
                throw new ArgumentException("Entity is not persisted!");
            }
            transaction.Commit();
        }
    }
}
