using ECommons.Reflection;
using System.IO;

namespace NotificationMaster;

internal class TrayIconManager
{
    private static object Icon = null;
    private static TickScheduler HideIconTask = null;

    private static void CreateIcon()
    {
        DestroyIcon();
        var iconType = Utils.GetTypeFromRuntimeAssembly("System.Drawing.Common", "System.Drawing.Icon");
        var iconImage = Activator.CreateInstance(iconType, [Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "nmaster.ico")]);

        var notifyIconType = Utils.GetTypeFromRuntimeAssembly("System.Windows.Forms", "System.Windows.Forms.NotifyIcon");
        var notifyIcon = Activator.CreateInstance(notifyIconType);
        notifyIcon.SetFoP("Icon", iconImage);
        notifyIcon.SetFoP("Text", "FFXIV - NotificationMaster");
        notifyIcon.SetFoP("Visible", true);

        Icon = notifyIcon;
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
            Icon.SetFoP("Visible", false);
            Icon.Call("Dispose", [], true);
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
        if(Icon == null || !Icon.GetFoP<bool>("Visible"))
        {
            PluginLog.Debug("Creating new icon");
            CreateIcon();
        }
        HideIconTask = new TickScheduler(delegate
        {
            PluginLog.Debug("HideIconTask: calling DestroyIcon");
            DestroyIcon();
        }, 60000);
        PluginLog.Debug($"Icon is visible: {Icon.GetFoP<bool>("Visible")}");
        var enumToolTipIconType = Utils.GetTypeFromRuntimeAssembly("System.Windows.Forms", "System.Windows.Forms.ToolTipIcon");
        var enumValue = Enum.ToObject(enumToolTipIconType, 1);
        Icon.Call("ShowBalloonTip", [int.MaxValue, title, str, enumValue], true);
    }
}
