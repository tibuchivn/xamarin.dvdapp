using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DVDApp
{
    public class Core
    {
        public static async Task<string> GetImage()
        {
            string queryString = "http://dvdws.tibuchivn.com/image";

            string results = await DataService.GetDataFromServiceAsync(queryString);
            var dvdImage = JsonConvert.DeserializeObject<DvdImage>(results);
            if (dvdImage != null)
            {
                return dvdImage.Url;
            }
            return string.Empty;
        }

        public static string GetImageNoAsync()
        {
            string queryString = "http://dvdws.tibuchivn.com/image";

            string results = DataService.GetDataFromService(queryString);
            var dvdImage = JsonConvert.DeserializeObject<DvdImage>(results);

            if (dvdImage != null)
            {
                return dvdImage.Url;
            }
            return string.Empty;
        }
    }
}