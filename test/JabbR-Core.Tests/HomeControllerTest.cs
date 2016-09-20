using System;
using JabbR_Core.Controllers;
using JabbR_Core.ViewModels;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using JabbR_Core.Configuration;
using Microsoft.Extensions.Options;

namespace JabbR_Core.Tests
{
    public class HomeControllerTest
    {
        private HomeController _homeController;
        private ApplicationSettings _settings;
        
        public HomeControllerTest()
        {
            // Spoofing the ApplicationSettings to mimic options
            _settings = new ApplicationSettings()
            {
                GoogleAnalytics = "",
                AppInsights = "",
                Sha = "",
                Branch = "",
                Time = "",
                DebugMode = false,
                Version = Version.Parse("0.0"),
                IsAdmin = true,
                ClientLanguageResources = "",
                AllowRoomCreation = true,
                MaxMessageLength = 0
            };

            // Setup the options for DI into controller
            var setups = new List<IConfigureOptions<ApplicationSettings>>();
            var action = new Action<ApplicationSettings>(s => s = _settings);
            setups.Add(new ConfigureOptions<ApplicationSettings>(action));
            var options = new OptionsManager<ApplicationSettings>(setups);

            // Options passed to controller as parameter
            _homeController = new HomeController(options);
        }        

        [Fact]
        public void IndexNotNull()
        {
            var indexView = _homeController.Index();

            Assert.True(_homeController.ModelState.IsValid);
            Assert.NotEqual(null, indexView);

            Console.WriteLine("HomeControllerTest.IndexNotNull: Complete");
        }
    }
}
