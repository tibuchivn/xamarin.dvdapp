using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DVD.Ws.Data;
using Nancy;
using Newtonsoft.Json;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Web;
using RestSharp;
using System.Threading.Tasks;

namespace DVD.Ws
{
    public class ImageModule : NancyModule
    {
        private readonly Context dbContext;
        private JsonSerializerSettings ignoreNullValueSetting = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };
        public ImageModule() : base("/image")
        {
            StaticConfiguration.DisableErrorTraces = false;
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
            if (!RequestIsInWhiteList()) return GetGithubBuildStatus();
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
                return JsonConvert.SerializeObject(new ImageApiData { Url = imgUrl }, Formatting.None, ignoreNullValueSetting);
            }
            else if (returnType == "slack")
            {
                Task.Factory.StartNew(() => { LogApiCall(Request.Query["user_name"]); });
                return JsonConvert.SerializeObject(new ImageSlackData()
                {
                    ResponseType = "in_channel",
                    Text = "enjoy",
                    ImageSlackAttachmentImages = new List<ImageSlackAttachmentImage>
                    {
                        new ImageSlackAttachmentImage {ImageUrl = imgUrl}
                    }
                }, Formatting.None, ignoreNullValueSetting);
            }
            else
            {
                return imgUrl;
            }
        }

        private string GetGithubBuildStatus()
        {
            string branchName = Request.Query["text"];
            if (!string.IsNullOrEmpty(branchName))
            {
                branchName = HttpUtility.UrlEncode(branchName);
                string circleciUrl =
                    $"api/v1.1/project/github/TINYhr/tinypulse/tree/{branchName}?circle-token=5b0b6f9c43e8e841441b607910d80157156db928&limit=1";
                var client = new RestClient("https://circleci.com");
                var request = new RestRequest(circleciUrl, Method.GET);
                request.AddHeader("Accept", "application/json");

                IRestResponse<List<CircleCiResponse>> response = client.Execute<List<CircleCiResponse>>(request);
                if (response.Data != null && response.Data.Any())
                {
                    var buildResult = response.Data.First();
                    return JsonConvert.SerializeObject(new CircleCiSlackData()
                    {
                        ResponseType = "in_channel",
                        Text = $"Branch {branchName} - Status {buildResult.Status.ToUpper()}",
                        CirlceCiSlackAttachmentTextList = new List<CirlceCiSlackAttachmentText>
                        {
                            new CirlceCiSlackAttachmentText { Text = buildResult.AuthorName },
                            new CirlceCiSlackAttachmentText { Text = buildResult.Subject },
                            new CirlceCiSlackAttachmentText { Text = buildResult.Body },
                            new CirlceCiSlackAttachmentText { Text = buildResult.BuildUrl }
                        }
                    }, Formatting.None, ignoreNullValueSetting);
                }
                else
                {
                    return $"Cannot find {branchName} on circleci";
                }
            }
            else
            {
                return "Branch name is required";
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

        private bool RequestIsInWhiteList()
        {
            var whiteListConfig = ConfigurationManager.AppSettings["whitelist"];
            string queryString = Request.Query["channel_id"];
            //DebugLog();
            WriteLog(queryString);
            if (!string.IsNullOrEmpty(whiteListConfig) && !string.IsNullOrEmpty(queryString))
            {
                var whiteListUrls = whiteListConfig.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var url in whiteListUrls)
                {
                    if (queryString.Contains(url)) return true;
                }
            }
            return false;
        }

        private void LogApiCall(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return;
            try
            {
                var logContext = new Context();
                var currentDate = DateTime.Now.Date;

                var logData = logContext.ApiLogs.FirstOrDefault(x => x.UserName == userName && x.CallDate == currentDate);
                if (logData == null)
                {
                    logData = new ApiLog { UserName = userName, CallDate = currentDate };
                    logContext.ApiLogs.Add(logData);
                }
                logData.CallCount++;
                logContext.SaveChanges();
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString());
            }
        }

        private void DebugLog()
        {
            try
            {
                var sb = new StringBuilder();
                foreach (string item in Request.Query.Keys)
                {
                    sb.AppendFormat("{0}: {1} ---", item, Request.Query[item]);
                }
                WriteLog(sb.ToString());
            }
            catch (Exception e)
            {
                WriteLog(e.ToString());
            }
        }

        private void WriteLog(string content)
        {
            string source = "DVD";
            string log = "Application";
            if (!EventLog.SourceExists(source))
            {
                EventLog.CreateEventSource(source, log);
            }
            EventLog.WriteEntry(source, content, EventLogEntryType.Information);
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

    public class CircleCiSlackData
    {
        [JsonProperty("response_type")]
        public string ResponseType { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("attachments")]
        public List<CirlceCiSlackAttachmentText> CirlceCiSlackAttachmentTextList { get; set; }
    }

    public class CirlceCiSlackAttachmentText
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class CircleCiResponse
    {
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("body")]
        public string Body { get; set; }
        [JsonProperty("build_url")]
        public string BuildUrl { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("author_name")]
        public string AuthorName { get; set; }
    }
}