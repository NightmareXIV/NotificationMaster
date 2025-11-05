using System.Net.Http;
using System.Net.Http.Headers;
using static NotificationMaster.Static;

namespace NotificationMaster;

internal class HttpMaster : IDisposable
{
    private HttpClient client;
    public HttpMaster()
    {
        client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public void Dispose()
    {
        client.Dispose();
    }

    private void Request(string uri, string content, int type = 0)
    {
        try
        {
            PluginLog.Debug("Preparing http request");
            var RequestUri = new Uri(uri);
            var request = new HttpRequestMessage()
            {
                Method = type == 0 ? HttpMethod.Get : HttpMethod.Post,
                RequestUri = RequestUri,
                Content = new StringContent(content)
            };
            if(RequestUri.UserInfo.Length != 0)
            {
                var b64string = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(RequestUri.UserInfo));
                PluginLog.Debug($"Authorization: Basic {b64string}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", b64string);
            }
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(type == 2 ? "application/json" : "application/x-www-form-urlencoded");
            #if DEBUG
            PluginLog.Debug("Requesting " + request.RequestUri + "\n" +
                request.Content.ReadAsStringAsync().Result);
            #endif
            PluginLog.Debug("Preparing to send http request");
            PluginLog.Debug(client.SendAsync(request).Result.ToString());
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
        foreach(var e in elements)
        {
            Request(e.URI.ReplaceAll(replacements, ReplaceType.URLEncode), e.Content.ReplaceAll(replacements, e.type == 2 ? ReplaceType.JSON : ReplaceType.Normal), e.type);
        }
    }
}
