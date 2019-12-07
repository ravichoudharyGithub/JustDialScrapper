using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verq.VBHelper;
using IronOcr;
using System.IO;
using System.Text.RegularExpressions;

namespace JustDialScrapper
{
    internal static class Helper
    {
        public static string ExtractAfterEquals(string field)
        {
            var posequals = field.IndexOf('=');
            if (posequals >= 0)
                return field.SubstringLimit(posequals + 1);
            // nothing found, just return main part
            return field;
        }

        public static string GetDisplayName(PropertyInfo info)
        {
            try
            {
                if (info.GetCustomAttributesData().Any())
                {
                    var propertyInfo = info.GetCustomAttributesData().FirstOrDefault();
                    var customAttributeNamedArgument = propertyInfo?.NamedArguments?.FirstOrDefault();
                    if (customAttributeNamedArgument != null)
                    {
                        var value = customAttributeNamedArgument.Value.ToString().Substring(7);
                        return value.Replace("\"", string.Empty).Trim();
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return info.Name;
        }

        public static string GetIdentifierBeforeEquals(string field)
        {
            var index = field.IndexOf("=", StringComparison.CurrentCulture);
            return field.Remove(index);
        }

        public static string GetMobileNumber(string resultText)
        {
            try
            {
                if (!string.IsNullOrEmpty(resultText))
                {
                    var index = 0;
                    if (resultText.ContainsCaseInsensitive("+91"))
                        index = resultText.IndexOf("+91");
                    else if (resultText.ContainsCaseInsensitive("141"))
                        index = resultText.IndexOf("141");

                    var subStringRes = resultText.Substring(index).TrimStart();
                    subStringRes = subStringRes.Substring(0, subStringRes.IndexOf(' ')).TrimStart();
                    if (subStringRes.ContainsCaseInsensitive("\r"))
                    {
                        var num = subStringRes.Substring(0, subStringRes.IndexOf('\r')).TrimStart();
                        if (!string.IsNullOrEmpty(num))
                            return num;
                        else
                            throw new Exception("Number not found.");
                    }

                    return $"+{Regex.Replace(subStringRes, @"[^\w\.@-]", "", RegexOptions.None).Trim()}";
                }
            }
            catch (Exception e)
            {

            }

            return null;
        }

        public static List<string> LoadCsvFile(string filePath)
        {
            using (var reader = new StreamReader(File.OpenRead(filePath)))
            {
                var searchList = new List<string>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    searchList.Add(line);
                }

                return searchList;
            }
        }

        public static List<Agency> LoadCsvFileInObject(string filePath)
        {
            var values = File.ReadAllLines(filePath)
                                           .Skip(1)
                                           .Select(v => Agency.FromCsv(v))
                                           .ToList();

            return values;
        }

        /// <summary>
        /// This function return image parse text.
        /// </summary>
        /// <param name="fileName">file name with extension.</param>
        /// <returns></returns>
        public static string ParseImage(string fileName)
        {
            try
            {
                // https://ironsoftware.com/csharp/ocr/tutorials/how-to-read-text-from-an-image-in-csharp-net/
                var OCR = new AutoOcr { ReadBarCodes = false };
                var Results = OCR.Read(fileName);
                //Console.WriteLine(Results.Text);
                return Results.Text;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static string ToCsv<T>(IEnumerable<T> objectlist)
        {
            var csvdata = new StringBuilder();
            try
            {
                var separator = "\",\"";
                var fields = typeof(T).GetFields();
                var properties = typeof(T).GetProperties().ToArray();
                var heading = new string[properties.Length];
                var i = 0;
                foreach (var item in properties)
                {
                    heading[i] = GetDisplayName(item);
                    i++;
                }
                var header = "\"" + string.Join(separator, heading) + "\"";
                csvdata.AppendLine(header);
                foreach (var o in objectlist)
                {
                    var res = string.Join(separator, fields.Select(f => (f.GetValue(o) ?? "").ToString()).Concat(properties.Select(p => (p.GetValue(o, null) ?? ""))).ToArray());
                    //if (res.ToUpper() == "TRUE") res = "1";
                    //if (res.ToUpper() == "FALSE") res = "0";
                    res = "\"" + res + "\"";
                    csvdata.AppendLine(res);
                }
                return csvdata.ToString();
            }
            catch
            {
                return csvdata.ToString();
            }
        }
    }
}
