using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Cfg;
using System.Reflection;

namespace iLib.src.main.Utils
{
    public static class NHibernateTestHelper
    {
        private static ISessionFactory? _sessionFactory;
        private static Configuration? _configuration;

        public static ISessionFactory SessionFactory => _sessionFactory ??= CreateSessionFactory();

        private static ISessionFactory CreateSessionFactory()
        {
            _configuration = Fluently.Configure()
                .Database(SQLiteConfiguration.Standard.InMemory().ShowSql())
                .Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
                .ExposeConfiguration(cfg =>
                {
                    cfg.SetProperty(NHibernate.Cfg.Environment.ReleaseConnections, "on_close");
                    new SchemaExport(cfg).Create(false, true);
                })
                .BuildConfiguration();

            return _configuration.BuildSessionFactory();
        }

        public static NHibernate.ISession OpenSession()
        {
            var session = SessionFactory.OpenSession();
            new SchemaExport(_configuration).Execute(false, true, false, session.Connection, null);
            return session;
        }
    }
}
