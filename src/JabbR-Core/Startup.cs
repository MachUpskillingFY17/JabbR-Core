using System;
using System.Collections.Generic;
using System.Security.Claims;
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
using JabbR_Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
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

            if(env.IsDevelopment())
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
            //services.AddDbContext<JabbrContext>(options => options.UseInMemoryDatabase());
            //services.AddDbContext<JabbrContext>(options => options.UseSqlServer(connection));

            services.AddMvc();
            services.AddSignalR();

            // Create instances to register. Required for ChatService to work
            var repository = new InMemoryRepository();
            var recentMessageCache = new RecentMessageCache();
            var httpContextAccessor = new HttpContextAccessor();

            var chatService = new ChatService(null, recentMessageCache, repository, null);

            // Register the provider that points to the specific instance
            services.AddSingleton<IJabbrRepository>(provider => repository);
            services.AddSingleton<IRecentMessageCache>(provider => recentMessageCache);
            services.AddSingleton<IHttpContextAccessor>(provider => httpContextAccessor);
            services.AddSingleton<IChatService>(provider => chatService);

            // investigate the below
            //services.AddIdentity<>();

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
                    AuthenticationScheme =  Constants.JabbRAuthType,
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

            //var context = app.ApplicationServices.GetService<JabbrContext>();
            //AddTestData(context);
           
        }

        private void AddTestData(JabbrContext context)
        {
            var room = new Data.Models.ChatRoom
            {
                Name = "Fun Room",
                Topic = "Fun things & JabbR",
            };
            var user = new Data.Models.ChatUser
            {
                Name = "Jimmy Johnson",
                ChatRooms = new List<Data.Models.ChatRoom>() { room }
            };
            var userRoom = new ChatUserChatRooms()
            {
                ChatUserKey = user.Key,
                ChatRoomKey = room.Key
            };
            var userRoomList = new List<ChatUserChatRooms>() { userRoom };

            room.Users = userRoomList;
            user.Rooms = userRoomList;

            context.Rooms.Add(room);
            context.Users.Add(user);
            context.ChatUserChatRooms.Add(userRoom);
        }
    }


}
