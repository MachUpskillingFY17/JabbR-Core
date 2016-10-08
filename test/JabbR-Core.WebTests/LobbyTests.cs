using System;
using System.Threading;
//using OpenQA.Selenium;
//using OpenQA.Selenium.Remote;
using Selenium;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using NUnit.Framework;

namespace JabbR_Core.WebTests
{
    [TestFixture]
    public class LobbyTests
    {

        private ISelenium _selenium;
        private StringBuilder _builder;

        [SetUp]
        public void SetUpTest()
        {
            // Related github issue
            //https://github.com/dotnet/cli/issues/3199
            _selenium = new DefaultSelenium("localhost", 4444, "*firefox", "http://www.google.com/");
            _selenium.Start();
            _builder = new StringBuilder();
        }

        [TearDown]
        public void TearDownTest()
        {
            try
            {
                _selenium.Stop();
            }
            catch (Exception)
            {
                // Ignore errors if unable to close the browser
            }
            Assert.AreEqual("", _builder.ToString());
        }

        [Test]
        public void SomeTest()
        {
            _selenium.Open("/");
            _selenium.Type("q", "_selenium rc");
            _selenium.Click("btnG");
            _selenium.WaitForPageToLoad("30000");
            Assert.AreEqual("selenium rc - Google Search", _selenium.GetTitle());
        }


        //[TestMethod]
        //public void LoadAndWait()
        //{


        //    IWebDriver wd = new RemoteWebDriver(DesiredCapabilities.Firefox());
        //    try
        //    {
        //        wd.Navigate().GoToUrl("http://localhost:59395/");

        //        Thread.Sleep(5000);

        //        if (!(wd.FindElements(By.CssSelector("section#page")).Count != 0))
        //        {
        //            Console.Error.WriteLine("verifyElementPresent failed");
        //        }
        //    }
        //    finally
        //    {
        //        wd.Quit();
        //    }
        //}
    }
}
