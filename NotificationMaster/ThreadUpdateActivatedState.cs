using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationMaster
{
    internal class ThreadUpdateActivatedState : IDisposable
    {
        bool running = false;
        internal bool IsApplicationActivated = false;
        internal ThreadUpdateActivatedState()
        {
            new Thread((ThreadStart)delegate 
            {
                PluginLog.Information("ThreadUpdateActivatedState started");
                while (running)
                {
                    try
                    {
                        IsApplicationActivated = Native.ApplicationIsActivated();
                    }
                    catch(Exception e)
                    {
                        PluginLog.Error(e.Message + "\n" + e.StackTrace ?? "");
                    }
                    Thread.Sleep(100);
                }
                PluginLog.Information("ThreadUpdateActivatedState finished");
            });
        }

        public void Dispose()
        {
            running = false;
        }
    }
}
