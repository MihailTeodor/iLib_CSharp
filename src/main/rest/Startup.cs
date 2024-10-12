using Microsoft.AspNetCore.Authentication;
using iLib.src.main.IDAO;
using iLib.src.main.Services;
using iLib.src.main.DAO;
using iLib.src.main.IServices;
using iLib.src.main.Utils;
using Newtonsoft.Json.Converters;

namespace iLib.src.main.rest
{
    public class Startup(IConfiguration configuration)
    {
        public IConfiguration Configuration { get; } = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });
            // Register DAOs with their interfaces
            services.AddScoped<IUserDao, UserDao>();
            services.AddScoped<IArticleDao, ArticleDao>();
            services.AddScoped<IBookDao, BookDao>();
            services.AddScoped<IMagazineDao, MagazineDao>();
            services.AddScoped<IMovieDVDDao, MovieDVDDao>();
            services.AddScoped<IBookingDao, BookingDao>();
            services.AddScoped<ILoanDao, LoanDao>();

            // Register Services with their interfaces
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IArticleService, ArticleService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<ILoanService, LoanService>();

            // Add authentication
            services.AddAuthentication("Bearer")
                .AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>("Bearer", null);

            // Register the DatabaseInitializer as a hosted service
            services.AddHostedService<DatabaseInitializer>();

            // Register NHibernate
            services.AddSingleton(factory =>
            {
                return NHibernateHelper.SessionFactory;
            });
            services.AddScoped(factory =>
            {
                return NHibernateHelper.OpenSession();
            });

            // Configure CORS
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                        builder.SetIsOriginAllowed(_ => true) // Allows any origin
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            // Other service registrations
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UsePathBase("/ilib/v1");
            app.UseCors("CorsPolicy");
            app.UseRouting();
            app.UseMiddleware<JwtMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
