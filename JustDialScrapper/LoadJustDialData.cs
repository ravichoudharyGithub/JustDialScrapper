using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace JustDialScrapper
{
    public class LoadJustDialData : IDisposable
    {
        static readonly List<string> Urls = new List<string>
        {
            "https://www.justdial.com/Jaipur/Direct-Sales-Agencies/nct-10163634",
            "https://www.justdial.com/Jaipur/Estate-Agents/nct-10192623"
        };

        List<Agency> agencies = new List<Agency>();

        BackedSelenium selenium = new BackedSelenium();

        public LoadJustDialData() { }

        public void ProcessForJustDialData()
        {
            CreateJustDialDataFile();
            // after create get data.
            ParseJustDialDataFile();
        }

        public void CreateJustDialDataFile()
        {
            try
            {
                var links = new List<string>();
                foreach (var url in Urls)
                {
                    var page = 1;
                    selenium.OpenUrl(url);
                Top:
                    Thread.Sleep(5000);
                    FindPopUpAndClose();
                    selenium.MoveScreenToElement(500, 500);
                    selenium.MoveScreenToElement(1000, 1000);
                    selenium.MoveScreenToElement(5000, 5000);
                    Thread.Sleep(5000);
                    var html = selenium.GetHtmlSource();
                    // find data
                    var htmldoc = new HtmlDocument();
                    htmldoc.LoadHtml(html);
                    var agenciesAttrs = htmldoc.DocumentNode.SelectNodes("//span[@class='jcn']//a");
                    FindRelatedLinks(agenciesAttrs);

                    var nextElem = selenium.FindElement("xpath=//a[@rel = 'next']");
                    if (nextElem != null)
                    {
                        var disableNext = selenium.FindElement("xpath=//a[@rel = 'next' and @class='dis']");
                        if (disableNext == null)
                        {
                            var number = ++page;
                            var nextUrl = $"{url}/page-{++page}";
                            selenium.Dispose();
                            selenium = new BackedSelenium();
                            Console.WriteLine($"Processing Url :- {nextUrl}");
                            selenium.OpenUrl(nextUrl);
                            //selenium.Click("xpath=//a[@rel = 'next']");
                            goto Top;
                        }
                    }
                }

                if (agencies?.Count > 0)
                {
                    var csvString = Helper.ToCsv(agencies);
                    File.WriteAllText($"C:\\Local Project\\JustDialScrapper\\DataFiles\\Agencies.csv", csvString);
                    Console.WriteLine("Agencies File Created Successfully");
                }
            }
            catch (Exception e)
            {

            }

            selenium.Dispose();
        }

        public void ParseJustDialDataFile()
        {
            try
            {
                var resultFile = new List<Result>();
                var agencies = Helper.LoadCsvFileInObject($"C:\\Local Project\\JustDialScrapper\\DataFiles\\Agencies.csv");
                var screenshotpath = "C:\\Local Project\\JustDialScrapper\\DataFiles\\ScreenShots\\";

                if (agencies?.Count > 0)
                {
                    var count = 0;
                    Console.WriteLine($"Total Recores are :- {agencies.Count}");
                    foreach (var agency in agencies)
                    {
                        Console.WriteLine($"Processing For :- {agency.AgencyName}, {++count}");
                        selenium.OpenUrl(agency.AgencyLink.Replace("\"", "").Trim());
                        Thread.Sleep(2000);
                        var legalName = Regex.Replace(agency.AgencyName, @"[^\w\.@-]", "", RegexOptions.None).Trim();
                        var resultimage = $"{screenshotpath}{legalName}.png";
                        FindPopUpAndClose();
                        var mobileElem = selenium.FindElement("id=comp-contact");
                        if (mobileElem != null)
                        {
                            selenium.MoveScreenToElement(mobileElem.Location.X, mobileElem.Location.Y);
                        }
                        else
                        {
                            selenium.MoveScreenToElement(500, 500);
                        }
                        Thread.Sleep(1500);
                        var image = selenium.GetScreenShot();
                        image.SaveAsFile(resultimage);
                        Thread.Sleep(2000);
                        var imageTexts = Helper.ParseImage(resultimage);
                        if (!string.IsNullOrEmpty(imageTexts))
                        {
                            var mobile = Helper.GetMobileNumber(imageTexts);
                            Console.WriteLine(mobile);

                            resultFile.Add(new Result
                            {
                                Name = agency.AgencyName,
                                FirstMobileNumber = mobile
                            });
                        }
                        Console.WriteLine($"Done For :- {agency.AgencyName}, {count}");
                    }

                    if (resultFile?.Count > 0)
                    {
                        var adjuestedFile = new List<Result>();
                        foreach (var result in resultFile)
                        {
                            var name = result.Name.Replace("\"", "").Trim();
                            var mobile = result.FirstMobileNumber.Replace("\"", "").Replace("+91", "").Replace(".", "").Trim();
                            var splitNumber = mobile.Split(',');
                            adjuestedFile.Add(new Result
                            {
                                Name = name,
                                FirstMobileNumber = splitNumber?.FirstOrDefault() ?? string.Empty,
                                SecondMobileNumber = splitNumber?.Length > 1 ? splitNumber.LastOrDefault() : string.Empty
                            });
                        }

                        var csvString = Helper.ToCsv(adjuestedFile);
                        File.WriteAllText($"C:\\Local Project\\JustDialScrapper\\DataFiles\\JustDialResult.csv", csvString);
                        Console.WriteLine("Result File Created Successfully");
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        public void FindPopUpAndClose()
        {
            var bestDealPopup = selenium.FindElement("class=jpbg");
            if (bestDealPopup != null)
            {
                selenium.Click("xpath=//*[@id='best_deal_div']/section/span");
            }

            var lookingPopup = selenium.FindElement("class=jcl");
            if (lookingPopup != null)
            {
                selenium.Click("xpath=//*[@id='best_deal_detail_div']/section/span");
            }
        }

        public void FindRelatedLinks(HtmlNodeCollection htmlNodes)
        {
            try
            {
                if (htmlNodes?.Count > 0)
                {
                    foreach (var agency in htmlNodes)
                    {
                        var link = agency.GetAttributeValue("href", "");
                        var name = agency.InnerText;
                        agencies.Add(new Agency
                        {
                            AgencyName = name.Replace(",", "").Trim(),
                            AgencyLink = link
                        });
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        public void Dispose()
        {
            selenium.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class Agency
    {
        public string AgencyName { get; set; }
        public string AgencyLink { get; set; }

        public static Agency FromCsv(string csvLine)
        {
            var values = csvLine.Split(',');
            return new Agency
            {
                AgencyName = values[0],
                AgencyLink = values[1]
            };
        }
    }

    public class Result
    {
        public string Name { get; set; }
        public string FirstMobileNumber { get; set; }
        public string SecondMobileNumber { get; set; }
    }
}
