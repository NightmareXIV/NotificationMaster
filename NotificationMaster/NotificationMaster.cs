using Dalamud.Logging;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationMaster
{
    class NotificationMaster : IDalamudPlugin
    {
        internal ActionManager actMgr;
        internal Configuration cfg;
        internal ConfigGui configGui;
        internal GpNotify gpNotify = null;

        public string Name => "NotificationMaster";

        public NotificationMaster(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Svc>();
            cfg = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            cfg.Initialize(Svc.PluginInterface);
            actMgr = new ActionManager(this);

            configGui = new ConfigGui(this);
            Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { configGui.open = true; };

            if (cfg.gp_Enable) SetupGpNotify(true);
        }

        public void Dispose()
        {
            SetupGpNotify(false);
            cfg.Save();
            configGui.Dispose();
        }

        internal void SetupGpNotify(bool enable)
        {
            if (enable)
            {
                if(gpNotify == null)
                {
                    gpNotify = new GpNotify(this);
                    PluginLog.Verbose("Enabling GP notify");
                }
                else
                {
                    PluginLog.Verbose("GP notify module already enabled");
                }
            }
            else
            {
                if (gpNotify != null)
                {
                    gpNotify.Dispose();
                    gpNotify = null;
                    PluginLog.Verbose("Disabling GP notify");
                }
                else
                {
                    PluginLog.Verbose("GP notify module already disabled");
                }
            }
        }
    }
}
