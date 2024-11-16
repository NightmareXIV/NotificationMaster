using ECommons.Logging;
using System;
using System.Threading;

namespace NotificationMaster;

public class ThreadUpdateActivatedState : IDisposable
{
    private volatile bool running = true;
    public volatile bool IsApplicationActivated = false;
    internal ThreadUpdateActivatedState()
    {
        new Thread((ThreadStart)delegate
        {
            PluginLog.Debug("ThreadUpdateActivatedState started");
            while(running)
            {
                try
                {
                    if(Native.ApplicationIsActivated())
                    {
                        if(!IsApplicationActivated)
                        {
                            IsApplicationActivated = true;
                            PluginLog.Verbose("ThreadUpdateActivatedState: application just got activated");
                        }
                    }
                    else
                    {
                        if(IsApplicationActivated)
                        {
                            IsApplicationActivated = false;
                            PluginLog.Verbose("ThreadUpdateActivatedState: application just got deactivated");
                        }
                    }
                }
                catch(Exception e)
                {
                    PluginLog.Error(e.Message + "\n" + e.StackTrace ?? "");
                }
                Thread.Sleep(100);
            }
            PluginLog.Debug("ThreadUpdateActivatedState finished");
        }).Start();
    }

    public void Dispose()
    {
        running = false;
    }
}
