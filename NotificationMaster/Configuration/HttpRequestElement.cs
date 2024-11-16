using System;

namespace NotificationMaster;

[Serializable]
internal class HttpRequestElement
{
    public int type = 0;
    public string URI = "";
    public string Content = "";
}
