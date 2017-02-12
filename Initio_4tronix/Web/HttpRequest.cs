using System;
using System.Text.RegularExpressions;
using Windows.Data.Json;

namespace Initio_4tronix.Web
{
    public class HttpRequest
    {
        public string Request { get; set; }
        public string RelativeUrl { get; set; }
    }

    public class HttpRequestContent
    {
        public bool Success { get; set;}
        public JsonObject Content { get; set; }
    }

    public static class HttpRequestContentHelper
    {
        public static HttpRequestContent GetContentAsJson(string request)
        {
            var httpRequestContent = new HttpRequestContent();

            var regex = new Regex("<JsonObject>(.*)</JsonObject>");
            var requestJsonObjectMatch = regex.Match(Uri.UnescapeDataString(request));

            if (requestJsonObjectMatch.Groups.Count <= 1)
            {
                return httpRequestContent;
            }

            var requestContent = requestJsonObjectMatch.Groups[1].ToString();
            var jsonObject = JsonObject.Parse(requestContent);

            httpRequestContent.Content = jsonObject;
            httpRequestContent.Success = true;

            return httpRequestContent;
        }
    }
}
