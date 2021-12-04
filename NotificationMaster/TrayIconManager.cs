using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NotificationMaster
{
    internal class TrayIconManager
    {
        private static NotifyIcon Icon = null;
        private static TickScheduler HideIconTask = null;

        static void CreateIcon()
        {
            DestroyIcon();
            Icon = new NotifyIcon
            {
                Icon = new Icon(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "nmaster.ico")),
                Text = "FFXIV - NotificationMaster",
                Visible = true
            };
        }

        static public void DestroyIcon()
        {
            if (HideIconTask != null)
            {
                HideIconTask.Dispose();
                HideIconTask = null;
            }
            if (Icon != null)
            {
                Icon.Visible = false;
                Icon.Dispose();
                Icon = null;
            }
        }

        public static void ShowToast(string str, string title = "")
        {
            if (HideIconTask != null)
            {
                HideIconTask.Dispose();
            }
            if (Icon == null || !Icon.Visible) CreateIcon();
            HideIconTask = new TickScheduler(delegate
            {
                DestroyIcon();
            }, Svc.Framework, 60000);
            PluginLog.Debug($"Icon is visible: {Icon.Visible}");
            Icon.ShowBalloonTip(int.MaxValue, title, str, ToolTipIcon.Info);
        }
    }
}
