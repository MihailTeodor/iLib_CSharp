using iLib.src.main.IDAO;
using iLib.src.main.Model;

namespace iLib.src.main.Utils
{
    public class DatabaseInitializer(IServiceProvider serviceProvider) : IHostedService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var userDao = scope.ServiceProvider.GetRequiredService<IUserDao>();

                const string adminEmail = "admin@example.com";
                User? admin;

                try
                {
                    admin = userDao.FindUserByEmail(adminEmail);
                }
                catch (Exception)
                {
                    admin = null;
                }

                if (admin == null)
                {
                    admin = ModelFactory.CreateUser();
                    admin.Email = adminEmail;
                    admin.Password = PasswordUtils.HashPassword("admin password");
                    admin.Role = UserRole.ADMINISTRATOR;
                    admin.Name = "Mihail";
                    admin.Surname = "Gurzu";
                    admin.Address = "admin address";
                    admin.TelephoneNumber = "1234567890";

                    userDao.Save(admin);
                }
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
