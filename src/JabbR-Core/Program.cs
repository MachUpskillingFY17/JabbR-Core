using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;


namespace JabbR_Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Allow command line options example dotnet run --urls http://*:5030
            //since defualt bindings are only to localhost:5000 and won't be exposed externally.
            var config = new ConfigurationBuilder().AddCommandLine(args).Build();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
               // .UseUrls("http://*:5000")
                .UseConfiguration(config)
                .Build();

            host.Run();
        }
    }
}
