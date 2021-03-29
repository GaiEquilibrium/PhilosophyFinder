using NUnit.Framework;
using PhilosophyFinder;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System.IO;
using System.Collections.Generic;

namespace SimpleTests
{
    public class Tests
    {
        private IWebDriver _driver;
        private string _resultDirectory;
        private KeyValuePair<string,int> _testCase; // start_link - required_steps_num
        Logger _logger;
        [SetUp]
        public void Setup()
        {
			try
			{
				_driver = new FirefoxDriver();
			}
			catch (DriverServiceNotFoundException)
			{
				_driver = new FirefoxDriver(Directory.GetCurrentDirectory());
			}
            _resultDirectory = Path.Combine(Directory.GetCurrentDirectory(), "test_results");
            Directory.CreateDirectory(_resultDirectory);
            _logger = new Logger(_resultDirectory);
            _testCase = new KeyValuePair<string, int>("https://ru.wikipedia.org/wiki/%D0%90%D0%B2%D1%82%D0%BE%D0%BC%D0%B0%D1%82%D0%B8%D0%B7%D0%B8%D1%80%D0%BE%D0%B2%D0%B0%D0%BD%D0%BD%D0%BE%D0%B5_%D1%82%D0%B5%D1%81%D1%82%D0%B8%D1%80%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D0%B5", 16);
        }

        [Test]
        public void Test1()
        {
            Pathfinder finder = new Pathfinder(_driver, _logger, _testCase.Key);
            finder.FindPath();
            if (finder.Step == _testCase.Value)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }

        [TearDown]
        public void TearDown()
        {
            _driver.Quit();
            _logger.stopLogging();
        }
    }
}