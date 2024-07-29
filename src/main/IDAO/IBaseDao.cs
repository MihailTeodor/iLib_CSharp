using iLib.src.main.Model;

namespace iLib.src.main.IDAO
{
    public interface IBaseDao<E> where E : BaseEntity
    {
        E FindById(Guid id);
        void Save(E entity);
        void Delete(E entity);
    }
}
