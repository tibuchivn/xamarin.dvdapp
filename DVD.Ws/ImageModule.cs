using System;
using System.Linq;
using System.Net;
using DVD.Ws.Data;
using Nancy;
using Newtonsoft.Json;

namespace DVD.Ws
{
    public class ImageModule : NancyModule
    {
        private readonly Context dbContext;
        public ImageModule() : base("/image")
        {
            this.dbContext = new Context();
            Get["/"] = _ => GetRandomImage();
        }

        private string GetRandomImage()
        {
            string imgUrl = string.Empty;
            bool isDone = false;
            while (!isDone)
            {
                var image = dbContext.Images.Where(x => x.IsBadURL == false).OrderBy(x => Guid.NewGuid()).First();
                if (CheckImageUrlExists(image.linkimg))
                {
                    imgUrl = image.linkimg;
                    isDone = true;
                }
            }
            return JsonConvert.SerializeObject(new ImageApiData {Url = imgUrl});
        }

        private bool CheckImageUrlExists(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";

            bool exists;
            try
            {
                request.GetResponse();
                exists = true;
            }
            catch
            {
                exists = false;
            }
            return exists;
        }
    }

    public class ImageApiData
    {
        public string Url { get; set; }
    }
}