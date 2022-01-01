using Dalamud.Game.Command;
using Dalamud.Interface.Internal.Notifications;
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
        internal CutsceneEnded cutsceneEnded = null;
        internal ChatMessage chatMessage = null;
        internal CfPop cfPop = null;
        internal LoginError loginError = null;
        internal ApproachingMapFlag mapFlag = null;
        internal HttpMaster httpMaster;
        internal ThreadUpdateActivatedState ThreadUpdActivated;
        internal FileSelector fileSelector = new();

        public string Name => "NotificationMaster";

        public NotificationMaster(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Svc>();
            cfg = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            cfg.Initialize(Svc.PluginInterface);
            actMgr = new(this);
            httpMaster = new();
            ThreadUpdActivated = new();

            configGui = new(this);
            Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { configGui.open = true; };

            if (cfg.gp_Enable) GpNotify.Setup(true, this);
            if (cfg.cutscene_Enable) CutsceneEnded.Setup(true, this);
            if (cfg.chatMessage_Enable) ChatMessage.Setup(true, this);
            if (cfg.cfPop_Enable) CfPop.Setup(true, this);
            if (cfg.loginError_Enable) LoginError.Setup(true, this);
            if (cfg.mapFlag_Enable) ApproachingMapFlag.Setup(true, this);
            if (Svc.PluginInterface.Reason == PluginLoadReason.Installer)
            {
                configGui.open = true;
                Svc.PluginInterface.UiBuilder.AddNotification(
                    "You have installed NotificationMaster plugin. By default, it has no modules enabled. \n" +
                    "A settings window has been opened: please configure the plugin.", 
                    "Please configure NotificationMaster", NotificationType.Info, 10000);
            }
            Svc.Commands.AddHandler("/pnotify", new CommandInfo(delegate
            {
                configGui.open = !configGui.open;
            })
            {
                HelpMessage = "open/close configuration"
            });
        }

        public void Dispose()
        {
            TrayIconManager.DestroyIcon();
            GpNotify.Setup(false, this);
            CutsceneEnded.Setup(false, this);
            ChatMessage.Setup(false, this);
            CfPop.Setup(false, this);
            LoginError.Setup(false, this);
            ApproachingMapFlag.Setup(false, this);
            ThreadUpdActivated.Dispose();
            cfg.Save();
            configGui.Dispose();
            Svc.Commands.RemoveHandler("/pnotify");
        }
    }
}
