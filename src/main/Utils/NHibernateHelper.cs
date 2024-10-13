using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using System.Reflection;

namespace iLib.src.main.Utils
{
    public static class NHibernateHelper
    {
        private static ISessionFactory? _sessionFactory;

        public static ISessionFactory SessionFactory => _sessionFactory ??= CreateSessionFactory();

        private static ISessionFactory CreateSessionFactory()
        {
            string dataSource = "localhost";
            string port = "3306";
            // string database = "iLib_C#";
            string database = "iLib_C#_test";
            string userId = "java-client";
            string password = "password";

            var connectionString = $"Server={dataSource};Port={port};Database={database};Uid={userId};Pwd={password};SslMode=None;AllowPublicKeyRetrieval=True;Convert Zero Datetime=True;Allow User Variables=True;default command timeout=600;persist security info=True;";

            return Fluently.Configure()
                .Database(MySQLConfiguration.Standard.ConnectionString(connectionString))
                .Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
                .ExposeConfiguration(cfg => new SchemaUpdate(cfg).Execute(false, true))
                .BuildSessionFactory();
        }

        public static NHibernate.ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }
    }
}
