﻿using Dalamud.Interface.Internal.Notifications;
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

        public string Name => "NotificationMaster";

        public NotificationMaster(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Svc>();
            cfg = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            cfg.Initialize(Svc.PluginInterface);
            actMgr = new ActionManager(this);

            configGui = new ConfigGui(this);
            Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { configGui.open = true; };

            if (cfg.gp_Enable) GpNotify.Setup(true, this);
            if (cfg.cutscene_Enable) CutsceneEnded.Setup(true, this);
            if (cfg.chatMessage_Enable) ChatMessage.Setup(true, this);
            if (Svc.PluginInterface.Reason == PluginLoadReason.Installer)
            {
                configGui.open = true;
                Svc.PluginInterface.UiBuilder.AddNotification(
                    "You have installed NotificationMaster plugin. By default, it has no modules enabled. \n" +
                    "A settings window has been opened: please configure the plugin.", 
                    "Please configure NotificationMaster", NotificationType.Info, 10000);
            }
        }

        public void Dispose()
        {
            GpNotify.Setup(false, this);
            CutsceneEnded.Setup(false, this);
            ChatMessage.Setup(false, this);
            cfg.Save();
            configGui.Dispose();
        }
    }
}