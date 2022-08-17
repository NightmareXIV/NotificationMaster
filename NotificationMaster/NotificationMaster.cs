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
    public class NotificationMaster : IDalamudPlugin
    {
        internal bool IsDisposed = false;
        internal Configuration cfg;
        internal ConfigGui configGui;

        internal GpNotify gpNotify = null;
        internal CutsceneEnded cutsceneEnded = null;
        internal ChatMessage chatMessage = null;
        internal CfPop cfPop = null;
        internal LoginError loginError = null;
        internal ApproachingMapFlag mapFlag = null;
        internal MobPulled mobPulled = null;

        internal HttpMaster httpMaster;
        public ThreadUpdateActivatedState ThreadUpdActivated;
        internal AudioSelector fileSelector = new();
        internal AudioPlayer audioPlayer;

        internal long PauseUntil = 0;

        public string Name => "NotificationMaster";

        public NotificationMaster(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Svc>();
            cfg = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            cfg.Initialize(Svc.PluginInterface);
            httpMaster = new();
            ThreadUpdActivated = new();
            audioPlayer = new(this);

            configGui = new(this);
            Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { configGui.open = true; };

            if (cfg.gp_Enable) GpNotify.Setup(true, this);
            if (cfg.cutscene_Enable) CutsceneEnded.Setup(true, this);
            if (cfg.chatMessage_Enable) ChatMessage.Setup(true, this);
            if (cfg.cfPop_Enable) CfPop.Setup(true, this);
            if (cfg.loginError_Enable) LoginError.Setup(true, this);
            if (cfg.mapFlag_Enable) ApproachingMapFlag.Setup(true, this);
            if (cfg.mobPulled_Enable) MobPulled.Setup(true, this);
            if (Svc.PluginInterface.Reason == PluginLoadReason.Installer)
            {
                configGui.open = true;
                Svc.PluginInterface.UiBuilder.AddNotification(
                    "You have installed NotificationMaster plugin. By default, it has no modules enabled. \n" +
                    "A settings window has been opened: please configure the plugin.", 
                    "Please configure NotificationMaster", NotificationType.Info, 10000);
            }
            Svc.Commands.AddHandler("/pnotify", new CommandInfo(OnCommand)
            {
                HelpMessage = "open/close configuration\n" +
                "/pnotify shutup|s [time in minutes] - pause plugin for specified amount of minutes or until restart if time is not specified\n" +
                "/pnotify resume|r - resume plugin operation"
            });
        }

        private void OnCommand(string command, string arguments)
        {
            if(arguments == "")
            {
                configGui.open = !configGui.open;
            }
            else
            {
                var args = arguments.Split(' ');
                if(args[0].Equals("shutup", StringComparison.OrdinalIgnoreCase) || args[0].Equals("s", StringComparison.OrdinalIgnoreCase))
                {
                    if(args.Length == 1)
                    {
                        PauseUntil = long.MaxValue;
                        Static.Notify("Plugin paused until restart", NotificationType.Success);
                    }
                    else
                    {
                        if(uint.TryParse(args[1], out var minutes))
                        {
                            PauseUntil = Environment.TickCount64 + minutes * 60 * 1000;
                            Static.Notify($"Plugin paused for {minutes} minutes", NotificationType.Success);
                        }
                        else
                        {
                            Static.Notify("Please enter amount of time in minutes");
                        }
                    }
                }
                else if(args[0].Equals("resume", StringComparison.OrdinalIgnoreCase) || args[0].Equals("r", StringComparison.OrdinalIgnoreCase))
                {
                    PauseUntil = 0;
                    Static.Notify("Plugin operation resumed", NotificationType.Success);
                }
                else
                {
                    Static.Notify("Invanid command", NotificationType.Error);
                }
            }
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
            MobPulled.Setup(false, this);
            ThreadUpdActivated.Dispose();
            audioPlayer.Dispose();
            cfg.Save();
            configGui.Dispose();
            Svc.Commands.RemoveHandler("/pnotify");
            IsDisposed = true;
        }
    }
}
