using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationMaster
{
    public class ThreadUpdateActivatedState : IDisposable
    {
        volatile bool running = true;
        public volatile bool IsApplicationActivated = false;
        internal ThreadUpdateActivatedState()
        {
            new Thread((ThreadStart)delegate 
            {
                PluginLog.Information("ThreadUpdateActivatedState started");
                while (running)
                {
                    try
                    {
                        if (Native.ApplicationIsActivated())
                        {
                            if (!IsApplicationActivated)
                            {
                                IsApplicationActivated = true;
                                PluginLog.Debug("ThreadUpdateActivatedState: application just got activated");
                            }
                        }
                        else
                        {
                            if (IsApplicationActivated)
                            {
                                IsApplicationActivated = false;
                                PluginLog.Debug("ThreadUpdateActivatedState: application just got deactivated");
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        PluginLog.Error(e.Message + "\n" + e.StackTrace ?? "");
                    }
                    Thread.Sleep(100);
                }
                PluginLog.Information("ThreadUpdateActivatedState finished");
            }).Start();
        }

        public void Dispose()
        {
            running = false;
        }
    }
}
