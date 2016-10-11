using System;
using JabbR_Core.Localization;
using JabbR_Core.Infrastructure;
using JabbR_Core.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using JabbR_Core.Services;
using Microsoft.AspNetCore.Http;
using JabbR_Core.Data.Models;

namespace JabbR_Core
{
    public class Startup
    {
        private IConfigurationRoot _configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();

            _configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Use `dotnet user-secrets set key value` to save as an env variable
            // on your machine.
            //
            // Store the connection string using the CLI tool. Include your actual username and password
            // >dotnet user-secrets set "connectionString" "Server=MYAPPNAME.database.windows.net,1433;Initial Catalog=MYCATALOG;Persist Security Info=False;User ID={plaintext user};Password={plaintext pass};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
            // 
            // Reference the Configuration API with the key you defined, and your env variable will be referenced.
            string connection = _configuration["connectionString"];

            //services.AddEntityFrameworkInMemoryDatabase();
            services.AddDbContext<JabbrContext>();

            // Throws a typeload exception
            //services.AddEntityFrameworkInMemoryDatabase()
            //    .AddDbContext<JabbrContext>((serviceProvider, options) =>
            //    {
            //        options
            //        .UseInternalServiceProvider(serviceProvider)
            //        .UseInMemoryDatabase();
            //    });

            //services.AddDbContext<JabbrContext>(options => options.UseSqlServer(connection));
            //https://stormpath.com/blog/tutorial-entity-framework-core-in-memory-database-asp-net-core

            services.AddMvc();
            services.AddSignalR();

            // Create instances to register. Required for ChatService to work
            var context = new JabbrContext(new DbContextOptions<JabbrContext>());
            var repository = new InMemoryRepository(context);
            var recentMessageCache = new RecentMessageCache();
            var httpContextAccessor = new HttpContextAccessor();

            var chatService = new ChatService(null, recentMessageCache, repository, null);

            services.AddScoped<IJabbrRepository>(provider => repository);
            services.AddScoped<IChatService>(provider => chatService);
            services.AddSingleton<IRecentMessageCache>(provider => recentMessageCache);
            services.AddSingleton<IHttpContextAccessor>(provider => httpContextAccessor);

            // Register the provider that points to the specific instance
            //services.AddScoped<IJabbrRepository, InMemoryRepository>();
            //services.AddScoped<IChatService, ChatService>();
            //services.AddSingleton<IRecentMessageCache, RecentMessageCache>();
            //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Establish default settings from appsettings.json
            services.Configure<ApplicationSettings>(_configuration.GetSection("ApplicationSettings"));

            // Programmatically add other options that cannot be taken from static strings
            services.Configure<ApplicationSettings>(settings =>
            {
                settings.Version = Version.Parse("0.1");
                settings.Time = DateTimeOffset.UtcNow.ToString();
                settings.ClientLanguageResources = new ClientResourceManager().BuildClientResources();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor)
        {

            if (env.IsDevelopment())
            {

                app.UseCookieAuthentication(new CookieAuthenticationOptions()
                {
                    AuthenticationScheme = Constants.JabbRAuthType,
                    LoginPath = new PathString("/Account/Unauthorized/"),
                    AccessDeniedPath = new PathString("/Account/Forbidden/"),
                    AutomaticAuthenticate = true,
                    AutomaticChallenge = true,
                    CookieName = "jabbr.id"
                });
                app.UseFakeLogin();
            }

            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvcWithDefaultRoute();
            app.UseStaticFiles();
            app.UseSignalR();
        }
    }
}
