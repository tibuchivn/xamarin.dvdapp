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
using System.Net.Http;

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
                if (!RequestIsInWhiteList(Request.Query["channel_id"]))
                {
                    var githubStatus = GetGithubBuildStatus();
                    var githubResponse = (Response)githubStatus;
                    githubResponse.ContentType = "application/json";
                    return githubResponse;
                }

                var data = JsonConvert.SerializeObject(new ImageSlackData()
                {
                    ResponseType = "in_channel",
                    //Text = $"{Request.Query["user_name"]} is requesting {Request.Query["text"]}"
                    Text = $""
                }, Formatting.None, ignoreNullValueSetting);
                var response = (Response) data;
                response.ContentType = "application/json";
                Task.Factory.StartNew(() => { GetRandomImage("slack", Request.Query["response_url"], Request.Query["user_name"], Request.Query["channel_id"], Request.Query["text"]); });
                return response;
            };
        }

        private string GetRandomImage(string returnType = "json", string responseUrl = "", string userName = "", string channelID = "", string text = "")
        {
            var query = $"select top 1 linkimg from imglink where isbadurl=0";
            if (RequestContainKeyword(text))
            {
                query = $"{query} and isnice = 1";
            }
            else
            {
                query = $"{query} and (isnice = 0 or isnice is null)";
            }
            query = $"{query} order by checksum(newid())";
            WriteLog(query);
            var imgUrl = dbContext.Database.SqlQuery<string>(query).First();
            while (!CheckImageUrlExists(imgUrl))
            {
                imgUrl = dbContext.Database.SqlQuery<string>(query).First();
            }
            if (returnType == "json")
            {
                return JsonConvert.SerializeObject(new ImageApiData { Url = imgUrl }, Formatting.None, ignoreNullValueSetting);
            }
            else if (returnType == "slack")
            {
                Task.Factory.StartNew(() => { LogApiCall(userName); });
                var data = JsonConvert.SerializeObject(new ImageSlackData()
                {
                    ResponseType = "in_channel",
                    Text = $"enjoy {userName}",
                    ImageSlackAttachmentImages = new List<ImageSlackAttachmentImage>
                    {
                        new ImageSlackAttachmentImage {ImageUrl = imgUrl}
                    }
                }, Formatting.None, ignoreNullValueSetting);
                try
                {
                    var client = new HttpClient();
                    var content = new StringContent(data, Encoding.UTF8, "application/json");
                    var result = client.PostAsync(responseUrl, content).Result;
                    //WriteLog($"{responseUrl} - {result}");
                }
                catch (Exception ex)
                {
                    WriteLog(ex.ToString());
                }
                
                return data;
            }
            else
            {
                return imgUrl;
            }
        }

        private string GetGithubBuildStatus()
        {
            string branchName = string.IsNullOrEmpty(Request.Query["text"]) ? "master" : Request.Query["text"];
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
                            new CirlceCiSlackAttachmentText { Text = "Author: " + buildResult.AuthorName },
                            new CirlceCiSlackAttachmentText { Text = "Subject: " + buildResult.Subject },
                            new CirlceCiSlackAttachmentText { Text = "Body: " + buildResult.Body },
                            new CirlceCiSlackAttachmentText { Text = "CircleCI Url: " + buildResult.BuildUrl }
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

        private bool RequestIsInWhiteList(string channelId)
        {
            var whiteListConfig = ConfigurationManager.AppSettings["whitelist"];
            string queryString = channelId;
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

        private bool RequestContainKeyword(string command)
        {
            var keywordConfig = ConfigurationManager.AppSettings["keyword"];
            string text = command;
            if (!string.IsNullOrEmpty(keywordConfig) && !string.IsNullOrEmpty(text))
            {
                var keywords = keywordConfig.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var splittedText = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                return keywords.Intersect(splittedText).Any();
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