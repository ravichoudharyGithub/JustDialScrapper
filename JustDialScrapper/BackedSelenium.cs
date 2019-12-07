using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Selenium;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using System.Threading;
using Verq.VBHelper;

namespace JustDialScrapper
{
    public class BackedSelenium : IDisposable
    {
        public BackedSelenium()
        {

        }

        ChromeDriver _driver = new ChromeDriver();

        public void OpenUrl(string url)
        {
            _driver.Navigate().GoToUrl(url);
            Thread.Sleep(8000);
        }

        public void Dispose()
        {
            _driver.Dispose();
            GC.SuppressFinalize(this);
        }

        public string GetHtmlSourceVisibleTextOnly()
        {
            try
            {
                if (_driver != null)
                {

                    var bodyElement = _driver.FindElement(By.TagName("body"));

                    return bodyElement.Text;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "";
            }
            return "";
        }

        public void Type(string name, string value)
        {
            var nameStartsWithName = name.StartsWithCaseInsensitive("name=");
            var nameStartsWithId = name.StartsWithCaseInsensitive("id=");
            var nameStartsWithClassName = name.StartsWithCaseInsensitive("class=");
            var nameStartsWithXpath = name.StartsWithCaseInsensitive("xpath=");
            try
            {
                IWebElement elem = null;
                var backSpaces = new string('\b', 20);

                if (nameStartsWithId)
                {
                    var locname = name.SubstringLimit("id=".Length);
                    elem = _driver.FindElement(By.Id(locname));
                }
                else if (nameStartsWithName)
                {
                    var locname = name.SubstringLimit("name=".Length);
                    elem = _driver.FindElement(By.Name(locname));
                }
                else if (nameStartsWithClassName)
                {
                    var locname = name.SubstringLimit("class=".Length);
                    elem = _driver.FindElement(By.ClassName(locname));
                }
                else if (nameStartsWithXpath)
                {
                    var locname = name.SubstringLimit("xpath=".Length);
                    elem = _driver.FindElement(By.XPath(locname));
                }
                else
                {
                    elem = _driver.FindElement(By.Id(name));
                }

                if (elem != null)
                {
                    elem.Clear();
                    elem.SendKeys(value);
                }
            }
            catch
            {
                var s = "nothing";
            }
        }

        public void Click(string xpath, string DropDownValue = "")
        {
            var nameStartsWithName = xpath.StartsWithCaseInsensitive("name=");
            var nameStartsWithId = xpath.StartsWithCaseInsensitive("id=");
            var nameStartsWithClassName = xpath.StartsWithCaseInsensitive("class=");
            var nameStartwithXpath = xpath.StartsWithCaseInsensitive("xpath=");
            var nameStartwithXpath2 = xpath.StartsWithCaseInsensitive("Xpath2=");
            try
            {

                IWebElement elem = null;

                if (nameStartsWithId)
                {
                    var locname = xpath.SubstringLimit("id=".Length);
                    elem = _driver.FindElement(By.Id(locname));
                }
                else if (nameStartsWithName)
                {
                    var locname = xpath.SubstringLimit("name=".Length);
                    elem = _driver.FindElement(By.Name(locname));
                }
                else if (nameStartsWithClassName)
                {
                    var locname = xpath.SubstringLimit("class=".Length);
                    elem = _driver.FindElement(By.ClassName(locname));
                }
                else if (nameStartwithXpath)
                {
                    var locname = xpath.SubstringLimit("xpath=".Length);
                    elem = _driver.FindElement(By.XPath(locname));
                }

                else if (nameStartwithXpath2)
                {
                    var locname = xpath.SubstringLimit("xpath2=".Length);
                    var elems = _driver.FindElements(By.XPath(locname));
                    foreach (var item in elems)
                    {
                        if (item.Text.ContainsCaseInsensitive(DropDownValue))
                        {
                            elem = item;
                            break;
                        }
                    }
                }

                else
                {
                    elem = _driver.FindElement(By.Id(xpath));
                }

                elem?.Click();
            }
            catch (Exception e)
            {
                var s = "nothing";
            }

        }

        public void SelectExtended(string locator, string Value, ref int Return, bool Search = false)
        {
            Value = Value.Replace("label=", "");
            try
            {

                var type = Helper.GetIdentifierBeforeEquals(locator);
                var noprefix = Helper.ExtractAfterEquals(locator);

                SelectElement select = null;

                if (type.EqualsCaseInsensitive("xpath"))
                {
                    select = new SelectElement(_driver.FindElement(By.XPath(noprefix)));

                }
                else if (type.EqualsCaseInsensitive("id"))
                {
                    select = new SelectElement(_driver.FindElement(By.Id(noprefix)));
                }
                else if (type.EqualsCaseInsensitive("name"))
                {
                    select = new SelectElement(_driver.FindElement(By.Name(noprefix)));

                }
                else if (type.EqualsCaseInsensitive("css"))
                {
                    select = new SelectElement(_driver.FindElement(By.CssSelector(noprefix)));
                }
                else
                {
                    select = new SelectElement(_driver.FindElement(By.Id(noprefix)));
                }

                if (Search)
                {
                    var options = select.Options;
                    var selected = options.FirstOrDefault(x => x.Text.ContainsCaseInsensitive(Value));
                    if (selected == null)
                    {
                        foreach (var s in Value.Replace(" ", "|").Split('|'))
                        {
                            selected = options.FirstOrDefault(x => x.Text == s);
                            if (selected != null)
                            {
                                break;
                            }
                        }
                    }
                    if (selected == null) selected = options.FirstOrDefault();
                    if (selected != null)
                        Value = selected.Text;
                    else
                        Value = null;
                }
                //if (string.IsNullOrEmpty(select.SelectedOption.GetAttribute("value")))
                select.SelectByText(Value);
                Return = 0;
            }
            catch
            {
                var ex = "nothing,...";
                Return = 1;
            }
        }

        public IWebElement FindElement(string locator)
        {
            IWebElement element = null;
            try
            {
                var attribute = Helper.GetIdentifierBeforeEquals(locator);
                var noprefix = Helper.ExtractAfterEquals(locator);
                if (attribute.EqualsCaseInsensitive("xpath"))
                    element = _driver.FindElement(By.XPath(noprefix));
                else if (attribute.EqualsCaseInsensitive("id"))
                    element = _driver.FindElement(By.Id(noprefix));
                else if (attribute.EqualsCaseInsensitive("name"))
                    element = _driver.FindElement(By.Name(noprefix));
                else if (attribute.EqualsCaseInsensitive("css"))
                    element = _driver.FindElement(By.CssSelector(noprefix));
                else if (attribute.EqualsCaseInsensitive("linkText"))
                    element = _driver.FindElement(By.LinkText(noprefix));
                else if (attribute.EqualsCaseInsensitive("class"))
                    element = _driver.FindElement(By.ClassName(noprefix));
            }
            catch (Exception e)
            {
                var ex = "nothing,...";
            }
            return element;
        }

        public ICollection<IWebElement> FindElementCollection(string locator)
        {

            ICollection<IWebElement> element = null;
            try
            {
                var attribute = Helper.GetIdentifierBeforeEquals(locator);
                var noprefix = Helper.ExtractAfterEquals(locator);
                if (attribute.EqualsCaseInsensitive("xpath"))
                    element = _driver.FindElements(By.XPath(noprefix));
                else if (attribute.EqualsCaseInsensitive("id"))
                    element = _driver.FindElements(By.Id(noprefix));
                else if (attribute.EqualsCaseInsensitive("name"))
                    element = _driver.FindElements(By.Name(noprefix));
                else if (attribute.EqualsCaseInsensitive("css"))
                    element = _driver.FindElements(By.CssSelector(noprefix));
                else if (attribute.EqualsCaseInsensitive("linkText"))
                    element = _driver.FindElements(By.LinkText(noprefix));
                else if (attribute.EqualsCaseInsensitive("class"))
                    element = _driver.FindElements(By.ClassName(noprefix));
            }
            catch (Exception e)
            {
                var ex = "nothing,...";
            }
            return element;
        }

        public IList<IWebElement> FindElementCollectionInList(string locator)
        {

            IList<IWebElement> element = null;
            try
            {
                var attribute = Helper.GetIdentifierBeforeEquals(locator);
                var noprefix = Helper.ExtractAfterEquals(locator);
                if (attribute.EqualsCaseInsensitive("xpath"))
                    element = _driver.FindElements(By.XPath(noprefix));
                else if (attribute.EqualsCaseInsensitive("id"))
                    element = _driver.FindElements(By.Id(noprefix));
                else if (attribute.EqualsCaseInsensitive("name"))
                    element = _driver.FindElements(By.Name(noprefix));
                else if (attribute.EqualsCaseInsensitive("css"))
                    element = _driver.FindElements(By.CssSelector(noprefix));
                else if (attribute.EqualsCaseInsensitive("linkText"))
                    element = _driver.FindElements(By.LinkText(noprefix));
                else if (attribute.EqualsCaseInsensitive("class"))
                    element = _driver.FindElements(By.ClassName(noprefix));
            }
            catch (Exception e)
            {
                var ex = "nothing,...";
            }
            return element;
        }

        public IReadOnlyCollection<IWebElement> FindElementCollection(string lookupType, string locator)
        {
            IReadOnlyCollection<IWebElement> elements = null;
            try
            {
                switch (lookupType)
                {
                    case "xpath":
                        elements = _driver.FindElements(By.XPath(locator));
                        break;
                    case "id":
                        elements = _driver.FindElements(By.Id(locator));
                        break;
                    case "name":
                        elements = _driver.FindElements(By.Name(locator));
                        break;
                    case "css":
                        elements = _driver.FindElements(By.CssSelector(locator));
                        break;
                    case "class":
                        elements = _driver.FindElements(By.ClassName(locator));
                        break;
                    default:
                        break;
                }

                return elements;
            }
            catch
            {
                //string ex = "nothing,...";
            }
            return elements;
        }

        public string GetHtmlSource()
        {
            var a = _driver.PageSource;
            return a;
        }

        public void MoveElement(string className)
        {
            var element = _driver.FindElement(By.ClassName(className));
            var actions = new Actions(_driver);
            actions.MoveToElement(element);
            actions.Perform();
        }

        public void MoveScreenToElement(int X, int Y)
        {
            try
            {
                var js = $"window.scrollTo({X}, {Y})";
                ((IJavaScriptExecutor)_driver).ExecuteScript(js);
            }
            catch (Exception e)
            {
                // nothing
            }
        }

        public Screenshot GetScreenShot()
        {
            return _driver.GetScreenshot();
        }

        public void MoveScreenToElement(IWebElement element)
        {
            try
            {
                var actions = new Actions(_driver);
                actions.MoveToElement(element);
                actions.Perform();
            }
            catch (Exception e)
            {
                //Nothing
            }
        }

    }
}
