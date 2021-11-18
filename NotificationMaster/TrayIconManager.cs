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

        static public NotifyIcon GetIcon()
        {
            if (Icon == null) CreateIcon();
            return Icon;
        }

        static public void DestroyIcon()
        {
            if(Icon != null)
            {
                Icon.Visible = false;
                Icon.Dispose();
                Icon = null;
            }
        }
    }
}
