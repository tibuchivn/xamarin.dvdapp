using System;
using System.Collections.Generic;
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
            Get["/url"] = _ =>
            {
                var response = (Response) GetRandomImage("slack");
                response.ContentType = "application/json";
                return response;
            };
        }

        private string GetRandomImage(string returnType = "json")
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
            if (returnType == "json")
            {
                return JsonConvert.SerializeObject(new ImageApiData { Url = imgUrl });
            }
            else if (returnType == "slack")
            {
                return JsonConvert.SerializeObject(new ImageSlackData()
                {
                    ResponseType = "in_channel",
                    Text = "enjoy",
                    ImageSlackAttachmentImages = new List<ImageSlackAttachmentImage>
                    {
                        new ImageSlackAttachmentImage {ImageUrl = imgUrl}
                    }
                });
            }
            else
            {
                return imgUrl;
            }
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

    public class ImageSlackData
    {
        [JsonProperty("response_type")]
        public string ResponseType { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("attachments")]
        public List<ImageSlackAttachmentImage> ImageSlackAttachmentImages { get; set; }
    }

    public class ImageSlackAttachmentImage
    {
        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }
    }
}