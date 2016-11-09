using System;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using static JabbR_Core.Services.MessageServices;
using JabbR_Core.Data.Logging;
using Microsoft.AspNetCore.SignalR.Hubs;

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
            //string connection = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=JabbREFTest;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=True";
            //If not running in Development (ie the env variable ASPNETCORE_ENVIRONMENT!=DEVELOPMENT) then the format to set at cmd line (on Windows)
            //set connectionString=Server=(localdb)\mssqllocaldb;Database=aspnet-application;Trusted_Connection=True;MultipleActiveResultSets=true
            string connection = _configuration["connectionString"];
            services.AddDbContext<JabbrContext>(options => /*options.UseInMemoryDatabase()*/ options.UseSqlServer(connection));
           
            services.AddAuthorization();
            services.AddMvc(options =>
            {
              //  options.Filters.Add(new RequireHttpsAttribute());
            });
            services.AddSignalR();

            services.AddScoped<ICache>(provider => null);
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IJabbrRepository, PersistedRepository>();
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

            // Microsoft.AspNetCore.Identity.EntityFrameworkCore
            services.AddIdentity<ChatUser, IdentityRole>()
                .AddEntityFrameworkStores<JabbrContext>()
                .AddDefaultTokenProviders();

            services.AddTransient<IEmailSender, AuthMessageSender>();  
            services.Configure<AuthMessageSenderOptions>(_configuration);

            //SignalR currently doesn't use DI to resolve hubs. This will allow it.
            services.AddSingleton<IHubActivator, ServicesHubActivator>();
            // This code has no effects right now, Chat hubs aren't called via DI
            // in SignalR, so at the moment we can't control the same objects being 
            // passed to hubs and ChatService
            services.AddTransient<Chat>(provider => 
            {
                // This is never hit
                var repository = provider.GetService<IJabbrRepository>();
                var settings = provider.GetService<IOptions<ApplicationSettings>>();
                var recentMessageCache = provider.GetService<IRecentMessageCache>();
                var chatService = provider.GetService<IChatService>();

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
                    .StyleSources(s=> s.Self().UnsafeInline())
                    .ImageSources(s=> s.Self().CustomSources("secure.gravatar.com")));
            app.UseXXssProtection(option => option.EnabledWithBlockMode());
            app.UseXfo(options => options.Deny());
            app.UseXContentTypeOptions();


            loggerFactory.AddProvider(new FileLoggerProvider());
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

            //if (env.IsDevelopment())
            //{

            //    app.UseCookieAuthentication(new CookieAuthenticationOptions()
            //    {
            //        AuthenticationScheme = Constants.JabbRAuthType,
            //        LoginPath = new PathString("/Account/Unauthorized/"),
            //        AccessDeniedPath = new PathString("/Account/Forbidden/"),
            //        AutomaticAuthenticate = true,
            //        AutomaticChallenge = true,
            //        CookieName = "jabbr.id"
            //    });
            //    //app.UseFakeLogin();
            //}

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            loggerFactory.AddConsole();
            app.UseStaticFiles();

            app.UseIdentity();

            // Facebook Social oAuth
            var facebookAppId = _configuration["Authentication:Facebook:AppId"];
            var facebookAppSecret = _configuration["Authentication:Facebook:AppSecret"];
            if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret))
            {
                app.UseFacebookAuthentication(new FacebookOptions()
                {
                    AppId = facebookAppId,
                    AppSecret = facebookAppSecret
                });
            }

            // Microsoft Social oAuth
            var microsoftAppId = _configuration["Authentication:Microsoft:AppId"];
            var microsoftAppSecret = _configuration["Authentication:Microsoft:AppSecret"];
            if (!string.IsNullOrEmpty(microsoftAppId) && !string.IsNullOrEmpty(microsoftAppSecret))
            {
                app.UseMicrosoftAccountAuthentication(new MicrosoftAccountOptions()
                {
                    ClientId = microsoftAppId,
                    ClientSecret = microsoftAppSecret
                });
            }

            // Google Social oAuth
            var googleAppId = _configuration["Authentication:Google:AppId"];
            var googleAppSecret = _configuration["Authentication:Google:AppSecret"];
            if (!string.IsNullOrEmpty(googleAppId) && !string.IsNullOrEmpty(googleAppSecret))
            {
                app.UseGoogleAuthentication(new GoogleOptions()
                {
                    ClientId = googleAppId,
                    ClientSecret = googleAppSecret
                });
            }

            // Twitter Social oAuth
            var twitterAppId = _configuration["Authentication:Twitter:AppId"];
            var twitterAppSecret = _configuration["Authentication:Twitter:AppSecret"];
            if (!string.IsNullOrEmpty(twitterAppId) && !string.IsNullOrEmpty(twitterAppSecret))
            {
                app.UseTwitterAuthentication(new TwitterOptions()
                {
                    ConsumerKey = twitterAppId,
                    ConsumerSecret = twitterAppSecret,
                });
            }

            app.UseMvcWithDefaultRoute();
            app.UseSignalR();
        }
    }
}
