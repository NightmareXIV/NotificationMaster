using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static NotificationMaster.Static;

namespace NotificationMaster
{
    class HttpMaster:IDisposable
    {
        HttpClient client;
        public HttpMaster()
        {
            client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
        }

        public void Dispose()
        {
            client.Dispose();
        }

        void Request(string uri, string content, int type = 0)
        {
            try
            {
                var request = new HttpRequestMessage()
                {
                    Method = type == 0 ? HttpMethod.Get : HttpMethod.Post,
                    RequestUri = new Uri(uri),
                    Content = new StringContent(content)
                };
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(type == 2?"application/json":"application/x-www-form-urlencoded");
                //PluginLog.Information("Requesting " + request.RequestUri + "\n" +
                //    request.Content.ReadAsStringAsync().Result);
                client.SendAsync(request);
            }
            catch(Exception e)
            {
                PluginLog.Error($"Error while sending a request: {e.Message}\n{e.StackTrace ?? ""}");
                Svc.Chat.Print($"[NotificationMaster] Error occurred while sending an HTTP request: {e.Message}");
            }
        }

        internal void DoRequests(object loginError_HttpRequests, string[][] vs)
        {
            throw new NotImplementedException();
        }

        internal void DoRequests(List<HttpRequestElement> elements, string[][] replacements)
        {
            foreach (var e in elements)
            {
                Request(e.URI.ReplaceAll(replacements, ReplaceType.URLEncode), e.Content.ReplaceAll(replacements, e.type==2?ReplaceType.JSON:ReplaceType.Normal), e.type);
            }
        }
    }
}
