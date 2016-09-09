using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace se_builder {
  public class RouteToHome {
    static void Main(string[] args) {
      IWebDriver wd = new RemoteWebDriver(DesiredCapabilities.Firefox());
      try {
        var wait = new WebDriverWait(wd, TimeSpan.FromSeconds(60));
        wd.Navigate().GoToUrl("http://localhost:5000/");
        Thread.Sleep(5000);
      } finally { wd.Quit(); }
    }
    
    public static bool isAlertPresent(IWebDriver wd) {
        try {
            wd.SwitchTo().Alert();
            return true;
        } catch (NoAlertPresentException e) {
            return false;
        }
    }
  }
}
