﻿using System;
using JabbR_Core.Localization;
using JabbR_Core.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using JabbR_Core.Models;
using Microsoft.EntityFrameworkCore;

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
            services.AddDbContext<JabbrContext>(options => options.UseSqlServer(connection));

            services.AddMvc();
            services.AddSignalR();

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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvcWithDefaultRoute(); //Needed for routing of additional controllers; without it, only the home controller will work.
            app.UseStaticFiles();
            app.UseSignalR();
        }
    }
}
