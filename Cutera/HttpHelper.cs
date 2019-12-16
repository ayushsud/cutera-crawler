using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Cutera
{
    public static class HttpHelper
    {
        private static readonly string cuteraURL = "https://cutera.com/patients/find-a-provider";

        public static async Task<T> GETHttpResponse<T>(string url, bool ensureSuccess = true)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                if (ensureSuccess)
                    response.EnsureSuccessStatusCode();
                using var responseStream = await response.Content.ReadAsStreamAsync();
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    var responseString = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<T>(responseString);
                }
            }
        }

        public static async Task<string> GetCuteraResponse(string body)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(@"*/*"));
                var response = await client.PostAsync(cuteraURL, new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded"));
                response.EnsureSuccessStatusCode();
                using var responseStream = await response.Content.ReadAsStreamAsync();
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
