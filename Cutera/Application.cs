using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ExcelDataReader;
using HtmlAgilityPack;

namespace Cutera
{
    public class Application
    {
        public List<string> processedZipcodes = new List<string>();
        public List<string> zipCodes = new List<string>();
        public async Task Run(string inputFileName, string outputFileName)
        {
            zipCodes = GetZipCodesFromFile(inputFileName);
            var geoCodeProvider = new GeocoderGeocodeProvider();
            var cancellationTokenSource = new CancellationTokenSource();
            using (StreamWriter writer = new StreamWriter(outputFileName + ".csv", false, Encoding.UTF8, 65536))
            {
                List<Task> tasks = new List<Task>();
                writer.WriteLine("Name,Address,City,State,ZipCode,Country,Website,Phone");
                foreach (var zipCode in zipCodes)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var geocode = await geoCodeProvider.GetGeoCode(zipCode, cancellationTokenSource);
                        if (!string.IsNullOrWhiteSpace(geocode.Latitude) && !string.IsNullOrWhiteSpace(geocode.Longitude))
                        {
                            var providers = await GetProviders(zipCode, geocode);
                            var inputs = await GetFileInput(providers);
                            if (!string.IsNullOrWhiteSpace(inputs?.ToString()))
                                writer.WriteLine(inputs.ToString());
                        }
                        processedZipcodes.Add(zipCode);
                    }, cancellationTokenSource.Token));
                    if (cancellationTokenSource.IsCancellationRequested)
                        break;
                    await Task.Delay(700);
                }
                await Task.WhenAll(tasks);
            }
        }

        public ValueTask<StringBuilder> GetFileInput(string htmlData)
        {
            if (htmlData == null)
                return new ValueTask<StringBuilder>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlData);
            var results = doc.DocumentNode.SelectNodes("//div[@class='physician-office text-center']");
            if (results == null)
                return new ValueTask<StringBuilder>();
            var builder = new StringBuilder();
            foreach (var element in results)
            {
                try
                {
                    var children = element?.ChildNodes[3]?.ChildNodes;
                    string name = children?.FirstOrDefault(child => child.Attributes.Any(attr => attr.Name == "class" && attr.Value == "name")).InnerHtml;
                    name = Regex.Replace(name, @"\t|\n|\r", " ").Trim();
                    name = Regex.Replace(name, @"[ ]{2,}", " ").Trim();
                    string street = children?.FirstOrDefault(child => child.Attributes.Any(attr => attr.Name == "class" && attr.Value == "address")).InnerHtml;
                    street = Regex.Replace(street, @"\t|\n|\r", " ").Trim();
                    street = Regex.Replace(street, @"[ ]{2,}", " ").Trim();
                    var city_state_zip = children?.FirstOrDefault(child => child.Attributes.Any(attr => attr.Name == "class" && attr.Value == "city-state-zip")).InnerHtml.Split(',');
                    city_state_zip[0] = Regex.Replace(city_state_zip[0], @"\t|\n|\r", " ").Trim();
                    city_state_zip[0] = Regex.Replace(city_state_zip[0], @"[ ]{2,}", " ");
                    city_state_zip[1] = Regex.Replace(city_state_zip[1], @"\t|\n|\r", " ").Trim();
                    city_state_zip[1] = Regex.Replace(city_state_zip[1], @"[ ]{2,}", " ");
                    string address = city_state_zip?[0].Trim(',').Trim();
                    var state_zip_country = city_state_zip?[1].Split(' ').ToList();
                    var state = string.Join(" ", state_zip_country.Take(state_zip_country.Count - 3)).Trim();
                    var zipCode = state_zip_country?[state_zip_country.Count - 3].Trim();
                    var country = state_zip_country?.Last().Trim();
                    var website = children?.FirstOrDefault(child => child.Attributes.Any(attr => attr.Name == "class" && attr.Value == "website"))?.FirstChild?.Attributes?.FirstOrDefault(attr => attr.Name == "href")?.Value;
                    var phone = children?.FirstOrDefault(child => child.Name == "p" && child.Attributes.Count == 0 && child.FirstChild?.Name == "a")?.FirstChild.InnerHtml;
                    if (!string.IsNullOrWhiteSpace(phone))
                    {
                        phone = Regex.Replace(phone, @"\t|\n|\r", " ").Trim();
                        phone = Regex.Replace(phone, @"[ ]{2,}", " ").Trim();
                    }
                    builder.AppendLine(name?.Replace(',', ' ') + "," + street?.Replace(',', ' ') + "," + address?.Replace(',', ' ') + "," + state?.Replace(',', ' ') + "," + zipCode?.Replace(',', ' ') + "," + country?.Replace(',', ' ') + "," + website?.Replace(',', ' ') + "," + phone);
                }
                catch { }
            }
            return new ValueTask<StringBuilder>(builder);
        }

        private List<string> GetZipCodesFromFile(string inputFileName)
        {
            var zipCodes = new List<string>();
            using (var stream = File.Open(inputFileName, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.NextResult()) ;
                    var result = reader.AsDataSet();
                    var table = result.Tables[0];
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        for (int j = 0; j < table.Columns.Count; j++)
                        {
                            string zipCode = table.Rows[i].ItemArray[j].ToString();
                            if (zipCode.Length > 0 && zipCode.Length < 5)
                                zipCode = zipCode.Insert(0, "0");
                            if (!string.IsNullOrWhiteSpace(zipCode))
                            {
                                zipCodes.Add(zipCode);
                            }
                        }
                    }
                }
            }
            return zipCodes;
        }

        private async Task<string> GetProviders(string zipCode, Geocode geocode)
        {
            string body = "lat=" + geocode.Latitude + "&lng=" + geocode.Longitude + "&address=" + zipCode + "&radius=50&product=*";
            return await HttpHelper.GetCuteraResponse(body);
        }
    }
}
