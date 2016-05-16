using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DVDApp
{
    public class DataService
    {
        public static async Task<string> GetDataFromServiceAsync(string queryString)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(queryString);
            
            if (response != null)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return string.Empty;
        }

        public static string GetDataFromService(string queryString)
        {
            HttpClient client = new HttpClient();
            var response = client.GetAsync(queryString).Result;
            
            if (response != null)
            {
                return response.Content.ReadAsStringAsync().Result;
            }

            return string.Empty;
        }
    }
}