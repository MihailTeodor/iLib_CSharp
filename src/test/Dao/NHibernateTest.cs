using NHibernate;
using iLib.src.main.Utils;

public abstract class NHibernateTest : IDisposable
{
    protected NHibernate.ISession Session { get; private set; }
    protected ITransaction Transaction { get; private set; }

    protected NHibernateTest()
    {
        Session = NHibernateTestHelper.OpenSession();
        Transaction = Session.BeginTransaction();
        Initialize();
        Transaction.Commit();
        Session.Clear();
        Transaction = Session.BeginTransaction();
    }

    public void Dispose()
    {
        if (Transaction.IsActive)
        {
            Transaction.Rollback();
        }
        Session.Close();
    }

    protected abstract void Initialize();
}
