using ECommons.Logging;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NotificationMaster;

internal class TrayIconManager
{
    private static NotifyIcon Icon = null;
    private static TickScheduler HideIconTask = null;

    private static void CreateIcon()
    {
        DestroyIcon();
        Icon = new NotifyIcon
        {
            Icon = new Icon(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "nmaster.ico")),
            Text = "FFXIV - NotificationMaster",
            Visible = true
        };
    }

    public static void DestroyIcon()
    {
        if(HideIconTask != null)
        {
            HideIconTask.Dispose();
            HideIconTask = null;
        }
        if(Icon != null)
        {
            Icon.Visible = false;
            Icon.Dispose();
            Icon = null;
        }
    }

    public static void ShowToast(string str, string title = "")
    {
        PluginLog.Debug($"Preparing to show toast notification: title={title}, str={str}");
        if(HideIconTask != null)
        {
            PluginLog.Debug("Disposing old HideIconTask");
            HideIconTask.Dispose();
        }
        if(Icon == null || !Icon.Visible)
        {
            PluginLog.Debug("Creating new icon");
            CreateIcon();
        }
        HideIconTask = new TickScheduler(delegate
        {
            PluginLog.Debug("HideIconTask: calling DestroyIcon");
            DestroyIcon();
        }, Svc.Framework, 60000);
        PluginLog.Debug($"Icon is visible: {Icon.Visible}");
        Icon.ShowBalloonTip(int.MaxValue, title, str, ToolTipIcon.Info);
    }
}
