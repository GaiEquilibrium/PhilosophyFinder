//TODO:
//
//иногда ссылки на википедии ведут на определённую часть страницы через # в ссылке брать всё что до этого т.к. по факту страница одна что с указанием на определённый участок, что брать в целом
//
//добавить ожидание загрузки страницы
//
//подумать над поиском ссылки в тексте для GetElementsNotInParentheses
//
//написать интерфейс класса и добавить второй тип обхода
//
//вынести параметры из класса в файл
//
//добавить параметризацию скобок
//
//продумать, как правильно обработать случай, если стартовая страница == целевой
//
//изредко, превышается ожидание запроса - надо понять, почему? 
//пример: "The HTTP request to the remote WebDriver server for URL http://localhost:49882/session/7441b99d-34bb-478e-9e95-0138a9fc7d25/back timed out after 60 seconds."

using OpenQA.Selenium;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PhilosophyFinder
{
    public class Pathfinder
    {
        private readonly string _defaultStartPage = "https://ru.wikipedia.org";
        private readonly string _endPage = "https://ru.wikipedia.org/wiki/%D0%A4%D0%B8%D0%BB%D0%BE%D1%81%D0%BE%D1%84%D0%B8%D1%8F";  //Философия
        private readonly By _randomPage = By.XPath("//li[@id='n-randompage']/a");
        private readonly By _articaleLinks = By.XPath("./a[@href and not(@style='font-style:italic;')] | " +
                                                     "./b/a[@href and not(@style='font-style:italic;')]");
        private readonly By _articaleTextElemets = By.XPath("//div[@class='mw-parser-output']/p | " +
                                                           "//div[@class='mw-parser-output']/table[contains(@class,'wikitable')]/tbody/tr/th | " +
                                                           "//div[@class='mw-parser-output']/table[contains(@class,'wikitable')]/tbody/tr/td | " +
                                                           "//div[@class='mw-parser-output']/ul/li | " +
                                                           "//div[@class='mw-parser-output']/ol/li");

        private readonly IWebDriver _driver;
        private readonly Logger _logger;

        private IWebElement _nextPage;
        private List<string> _allVisited;

        public int Step { private set; get; }


        public Pathfinder(IWebDriver driver, Logger logger, string startPage = null)
        {
            Debug.Assert(driver != null && logger != null, "Pathfinder requires both params");

            _driver = driver;
            _logger = logger;

            _allVisited = new List<string>();
            Step = 0;
            if (startPage != null)
            {
                _driver.Navigate().GoToUrl(startPage);
                _nextPage = null;
                Step++;
                _allVisited.Add(startPage);
                _logger.Log(Step.ToString(), _driver.FindElement(By.XPath("//h1")).Text, _driver.Url);
            }
            else 
            {
                _driver.Navigate().GoToUrl(_defaultStartPage);
                _nextPage = _driver.FindElement(_randomPage);
            }
        }

        private List<IWebElement> GetElementsNotInParentheses(string paragraphText, System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> links)
        {
            var res = new List<IWebElement>();
            foreach (var link in links)
            {
                var linkText = link.Text;
                var linkUrl = link.GetAttribute("href");
                if (linkUrl.IndexOf("https://ru.wikipedia.org") != 0
                 || linkUrl.IndexOf("&action=edit&redlink=1") != -1
                 || linkText.Length == 0
                 || linkText[0] == '(' && linkText[linkText.Length - 1] == ')'
                 || IsParenthesesClosed(paragraphText.Substring(0, paragraphText.IndexOf(linkText))))
                {
                    continue;
                }
                res.Add(link);
            }
            return res;
        }

        private bool IsParenthesesClosed(string text)
        {
            int firstLeft = text.IndexOf('(');
            if (firstLeft < 0)
            {
                return false;
            }
            int LeftNum = text.Substring(firstLeft).Count(c => c == '(');
            int RightNum = text.Substring(firstLeft).Count(c => c == ')');
            if (LeftNum <= RightNum)
            {
                return false;
            }
            return true;
        }

        public void FindPath()
        {
            while (true)
            {
                string clickLink = null;
                if (_nextPage != null)
                {
                    clickLink = _nextPage.GetAttribute("href");
                    _nextPage.Click();
                    _nextPage = null;
                    Step++;

                    _logger.Log(Step.ToString(), _driver.FindElement(By.XPath("//h1")).Text, _driver.Url);
                    if (_driver.Url == _endPage)
                    {
                        break;
                    }
                    if (!_allVisited.Contains(_driver.Url))
                    {
                        _allVisited.Add(_driver.Url);
                    }
                    if (!_allVisited.Contains(clickLink))
                    {
                        _allVisited.Add(clickLink);
                    }
                }
                if (_nextPage == null)
                {
                    var elements = _driver.FindElements(_articaleTextElemets);
                    var links = new List<IWebElement>();
                    bool haveLinksNotInVisited = false;
                    for (int i = 1; i <= elements.Count && !haveLinksNotInVisited; i++)
                    {
                        var paragraphLinks = elements[i - 1].FindElements(_articaleLinks);
                        var normalLinks = GetElementsNotInParentheses(elements[i - 1].Text, paragraphLinks);
                        links.AddRange(normalLinks);
                        haveLinksNotInVisited = normalLinks.Any(x => !_allVisited.Contains(x.GetAttribute("href")));
                    }
                    for (int i = 0; i < links.Count; i++)
                    {
                        if (!_allVisited.Contains(links[i].GetAttribute("href")))
                        {
                            _nextPage = links[i];
                            break;
                        }
                    }
                    if (_nextPage == null)
                    {
                        _driver.Navigate().Back();
                        if (Step == 1)
                        {
                            _nextPage = _driver.FindElement(_randomPage);
                        }
                        Step--;
                    }

                }
            }
        }
    }
}
