using iLib.src.main.IDAO;
using iLib.src.main.Model;
using NHibernate.Criterion;

namespace iLib.src.main.DAO
{
    public class UserDao(NHibernate.ISession session) : BaseDao<User>(session), IUserDao
    {
        public User FindUserByEmail(string? email)
        {
            User user = session.QueryOver<User>()
                .Where(u => u.Email == email)
                .SingleOrDefault() ?? throw new InvalidOperationException("User not found");
            return user;
        }

        public IList<User> FindUsers(string? name, string? surname, string? telephoneNumber, int fromIndex, int limit)
        {
            var query = session.QueryOver<User>()
                .Where(u =>
                    (name == null || u.Name == name) &&
                    (surname == null || u.Surname == surname) &&
                    (telephoneNumber == null || u.TelephoneNumber == telephoneNumber))
                .OrderBy(u => u.Name).Asc
                .ThenBy(u => u.Surname).Asc
                .Skip(fromIndex)
                .Take(limit);

            return query.List<User>();
        }

        public long CountUsers(string? name, string? surname, string? telephoneNumber)
        {
            var query = session.QueryOver<User>()
                .Where(u =>
                    (name == null || u.Name == name) &&
                    (surname == null || u.Surname == surname) &&
                    (telephoneNumber == null || u.TelephoneNumber == telephoneNumber))
                .Select(Projections.RowCount());

        return query.SingleOrDefault<int>();
        }
    }
}
