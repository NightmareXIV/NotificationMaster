using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationMaster
{
    [Serializable]
    class HttpRequestElement
    {
        public int type = 0;
        public string URI = "";
        public string Content = "";
    }
}
