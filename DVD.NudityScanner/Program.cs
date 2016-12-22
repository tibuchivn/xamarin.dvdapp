using DVD.Ws.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using unirest_net.http;

namespace DVD.NudityScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            var isDone = false;
            var threadNum = 4;
            var dbContext = new Context();
            while (!isDone)
            {
                var images = dbContext.Images.Where(x => x.IsBadURL == false && x.IsNice == null)
                    .Take(threadNum).ToList();
                if (images.Count == 0)
                {
                    isDone = true;
                    break;
                }
                System.Threading.Tasks.Parallel.ForEach(images, (image) =>
                {
                    image.IsNice = IsNudeImage(image.linkimg);
                });
                dbContext.SaveChanges();
            }
        }

        static bool IsNudeImage(string url)
        {
            try
            {
                HttpResponse<MashapeData> response = Unirest
                    .get($"https://sphirelabs-advanced-porn-nudity-and-adult-content-detection.p.mashape.com/v1/get/index.php?url={HttpUtility.UrlEncode(url)}")
                    .header("X-Mashape-Key", "25Q9Q5E7fLmshx0PrYEJX4a01urwp1KOvbYjsnvUhAr7zeOz4p")
                    .header("Accept", "application/json")
                    .asJson<MashapeData>();
                return response.Body != null && response.Body.IsPorn.ToLower() == "true";
            }
            catch
            {
                throw;
                return false;
            }
        }
    }

    class MashapeData
    {
        [JsonProperty("Is Porn")]
        public string IsPorn { get; set; }
    }
}
