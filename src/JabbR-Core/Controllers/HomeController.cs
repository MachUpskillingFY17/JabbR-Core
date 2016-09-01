using JabbR_Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using JabbR_Core.Configuration;
using Microsoft.Extensions.Options;

namespace JabbR_Core.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationSettings _settings;

        public HomeController(IOptions<ApplicationSettings> settings)
        {
            _settings = settings.Value;
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            var model = new SettingsViewModel()
            {
                 GoogleAnalytics = _settings.GoogleAnalytics,
                 AppInsights = _settings.AppInsights,
                 Sha = _settings.Sha,
                 Branch = _settings.Branch,
                 Time = _settings.Time,
                 DebugMode = _settings.DebugMode,
                 Version = _settings.Version,
                 IsAdmin = _settings.IsAdmin,
                 AllowRoomCreation = _settings.AllowRoomCreation,
                 MaxMessageLength = _settings.MaxMessageLength
            };

            // Access the settings specified in appsettings.
            return View(model);
        }
    }
}
