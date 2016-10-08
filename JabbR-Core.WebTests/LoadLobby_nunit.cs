using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using NUnit.Framework;

namespace se_builder {
  [TestFixture()]
  public class LoadLobby_nunit {
    [Test()]
    public void TestCase() {
      IWebDriver wd = new RemoteWebDriver(DesiredCapabilities.Firefox());
      try {
        wd.Navigate().GoToUrl("http://localhost:59395/");
        Thread.Sleep(5000);
        if (!(wd.FindElements(By.CssSelector("section#page")).Count != 0)) {
            Console.Error.WriteLine("verifyElementPresent failed");
        }
      } finally { wd.Quit(); }
    }
  }
}
