using System;
using System.Buffers;
using System.Diagnostics;
using Newtonsoft.Json;
using JabbR_Core.Hubs;
using JabbR_Core.Services;
using JabbR_Core.Middleware;
using JabbR_Core.Data.Models;
using NWebsec.AspNetCore.Core;
using JabbR_Core.Localization;
using Microsoft.AspNetCore.Mvc;
using JabbR_Core.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using JabbR_Core.Data.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using NWebsec.AspNetCore.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using static JabbR_Core.Services.MessageServices;
using JabbR_Core.Data.Logging;
using Microsoft.AspNetCore.SignalR.Hubs;
using JabbRCore.Data.InMemory;
using Microsoft.Extensions.Logging.Console;

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

        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // ***** PLEASE READ FOR DB CONTEXT SETUP ***** 
            // Use `dotnet user-secrets set key value` to save as an env variable
            // on your machine.
            //
            // Store the connection string using the CLI tool. Include your actual username and password
            // >dotnet user-secrets set "connectionString" "Server=MYAPPNAME.database.windows.net,1433;Initial Catalog=MYCATALOG;Persist Security Info=False;User ID={plaintext user};Password={plaintext pass};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

            // Reference the Configuration API with the key you defined, and your env variable will be referenced.
            string connection = _configuration["connectionString"];
            //string connection = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=JabbREFTest;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=True";
            //If not running in Development (ie the env variable ASPNETCORE_ENVIRONMENT!=DEVELOPMENT) then the format to set at cmd line (on Windows)
            //set connectionString=Server=(localdb)\mssqllocaldb;Database=aspnet-application;Trusted_Connection=True;MultipleActiveResultSets=true

            bool inMem = false;

            services.AddTransient<JabbrContext, JabbrContext>();

            bool.TryParse(_configuration["ApplicationSettings:InMemoryDatabase"], out inMem);
            if (!inMem)
            {
                services.AddDbContext<JabbrContext>(
                    options => /*options.UseInMemoryDatabase()*/ options.UseSqlServer(connection));
            }
            else
            {
                //services.AddEntityFrameworkInMemoryDatabase();
                //services.AddDbContext<JabbrContext>();

                // To get around the typeload exception because of transactions as per EF team emails.
                services.AddScoped<InMemoryTransactionManager, TestInMemoryTransactionManager>();
                services.AddEntityFrameworkInMemoryDatabase()
                    .AddDbContext<JabbrContext>((serviceProvider, options) =>
                    {
                        options
                        .UseInternalServiceProvider(serviceProvider)
                        .UseInMemoryDatabase();
                    });
            }

            services.AddAuthorization();
            services.AddMvc(options =>
            {
                //  options.Filters.Add(new RequireHttpsAttribute());
            });
            services.AddSignalR();

            services.AddScoped<ICache>(provider => null);
            services.AddScoped<IChatService, ChatService>();
            services.AddTransient<IJabbrRepository, PersistedRepository>();
            services.AddScoped<ApplicationSettings>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IRecentMessageCache, RecentMessageCache>();
            //services.AddScoped<IMembershipService, MembershipService>();

            // Establish default settings from appsettings.json
            services.Configure<ApplicationSettings>(_configuration.GetSection("ApplicationSettings"));

            // Programmatically add other options that cannot be taken from static strings
            services.Configure<ApplicationSettings>(settings =>
            {
                settings.Version = Version.Parse("0.1");
                settings.Time = DateTimeOffset.UtcNow.ToString();
                settings.ClientLanguageResources = new ClientResourceManager().BuildClientResources();
            });


            services.AddIdentity<ChatUser, IdentityRole>()
                .AddEntityFrameworkStores<JabbrContext>()
                .AddDefaultTokenProviders();

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.Configure<AuthMessageSenderOptions>(_configuration);

            //SignalR currently doesn't use DI to resolve hubs. This will allow it.
            services.AddSingleton<IHubActivator, ServicesHubActivator>();

            services.AddTransient<Chat>(provider =>
            {
                var repository = provider.GetService<IJabbrRepository>();
                var settings = provider.GetService<IOptions<ApplicationSettings>>();
                var recentMessageCache = provider.GetService<IRecentMessageCache>();
                var iCache = provider.GetService<ICache>();
                var chatService = new ChatService(iCache, recentMessageCache, repository, settings.Value);
                //var chatService = provider.GetService<IChatService>();
                //Console.WriteLine($"Created ChatService:{chatService.GetHashCode()}");
                return new Chat(repository, settings, recentMessageCache, chatService);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor)
        {
            //Security headers
            app.UseHsts(options => options.MaxAge(days: 365));

            //TODO: AJS FIX UNSAFEEVAL AFTER INCLUDING ANGULAR JS 
            app.UseCsp(options =>
            options.DefaultSources(s => s.Self())
                    .ScriptSources(s => s.Self().CustomSources("ajax.aspnetcdn.com", "code.jquery.com").UnsafeEval())
                    .StyleSources(s => s.Self().UnsafeInline())
                    .ImageSources(s => s.Self().CustomSources("secure.gravatar.com")));
            app.UseXXssProtection(option => option.EnabledWithBlockMode());
            app.UseXfo(options => options.Deny());
            app.UseXContentTypeOptions();


            bool fileLogging = false;
            bool consoleLogging = false;
            bool.TryParse(_configuration["Logging:FileLogging"], out fileLogging);
            bool.TryParse(_configuration["Logging:ConsoleLogging"], out consoleLogging);
            if (fileLogging)
            {
                string logPath = _configuration["Logging:LogPath"];
                if (!string.IsNullOrEmpty(logPath))
                {
                    loggerFactory.AddProvider(new FileLoggerProvider(_configuration, logPath));
                }
                else
                {
                    Console.WriteLine("File logging was enabled but the logpath wasn't set. Ignoring file logging.");
                }
            }
            if (consoleLogging) loggerFactory.AddConsole(LogLevel.Debug);

            ////////////////////////////////////////////////////////////////
            // TODO: Authorize Attribute Re-routing to '~/Account/Login'
            //app.UseCookieAuthentication(new CookieAuthenticationOptions
            //{
            //    AuthenticationScheme = Constants.JabbRAuthType,
            //    LoginPath = new PathString("/Account/Login/"),
            //    AccessDeniedPath = new PathString("/Account/Forbidden/"),
            //    AutomaticAuthenticate = true, // run with every request and look for cookie if available
            //    AutomaticChallenge = true, // take raw 401 and 403 and use redirect paths as defined
            //    CookieName = "jabbr.id"
            //});
            ////////////////////////////////////////////////////////////////

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseStaticFiles();

            app.UseIdentity();
            app.UseFacebookAuthentication(new FacebookOptions()
            {
                AppId = _configuration["Authentication:Facebook:AppId"],
                AppSecret = _configuration["Authentication:Facebook:AppSecret"]
            });
            //app.UseMicrosoftAccountAuthentication(new MicrosoftAccountOptions()
            //{
            //    ClientId = _configuration["Authentication:Microsoft:AppId"],
            //    ClientSecret = _configuration["Authentication:Microsoft:AppSecret"]
            //});
            app.UseGoogleAuthentication(new GoogleOptions()
            {
                ClientId = _configuration["Authentication:Google:AppId"],
                ClientSecret = _configuration["Authentication:Google:AppSecret"]
            });
            //app.UseTwitterAuthentication(new TwitterOptions()
            //{
            //    ConsumerKey = _configuration["Authentication:Twitter:AppId"],
            //    ConsumerSecret = _configuration["Authentication:Twitter:AppSecret"],
            //});


            app.UseMvcWithDefaultRoute();
            app.UseSignalR();
        }
    }
}
