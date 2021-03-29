using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System.IO;


namespace PhilosophyFinder
{
    class Program
    {
        static void Main()
        {
            string resultDirectory = Path.Combine(Directory.GetCurrentDirectory(), "results");
            Directory.CreateDirectory(resultDirectory);
            Logger logger = new Logger(resultDirectory);
            IWebDriver driver = null;
            try
            {
                driver = new FirefoxDriver();
                Pathfinder finder = new Pathfinder(driver, logger);
                finder.FindPath();
            }
            catch (WebDriverException exc)
            {
                logger.Log("Error ocured.");
                logger.Log(exc.Message);
                logger.Log(exc.StackTrace);
            }
            if (driver != null)
            {
                driver.Quit();
            }
            logger.stopLogging();
        }
    }
}
