using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Logging;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationMaster
{
    class CutsceneEnded : IDisposable
    {
        bool isInCutscene = false;
        private NotificationMaster p;
        const int CastrumZoneId = 217;
        const int PraetoriumZoneId = 224;
        public void Dispose()
        {
            Svc.Framework.Update -= HandleFrameworkUpdate;
        }

        public CutsceneEnded(NotificationMaster plugin)
        {
            this.p = plugin;
            Svc.Framework.Update += HandleFrameworkUpdate;
        }

        private void HandleFrameworkUpdate(Framework framework)
        {
            if (Svc.ClientState == null) return;
            var c = Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]
                || Svc.Condition[ConditionFlag.WatchingCutscene78];
            if (isInCutscene && !c && !Native.ApplicationIsActivated() &&
                (!p.cfg.cutscene_OnlyMSQ || Svc.ClientState.TerritoryType == CastrumZoneId || Svc.ClientState.TerritoryType == PraetoriumZoneId))
            {
                if (p.cfg.cutscene_FlashTrayIcon)
                {
                    Native.Impl.FlashWindow();
                }
                if (p.cfg.cutscene_AutoActivateWindow) Native.Impl.Activate();
                if (p.cfg.cutscene_ShowToastNotification)
                {
                    Native.Impl.ShowToast("Cutscene ended");
                }
            }
            isInCutscene = c;
        }


        internal static void Setup(bool enable, NotificationMaster p)
        {
            if (enable)
            {
                if (p.cutsceneEnded == null)
                {
                    p.cutsceneEnded = new CutsceneEnded(p);
                    PluginLog.Verbose("Enabling cutscene ended module");
                }
                else
                {
                    PluginLog.Verbose("cutscene ended module already enabled");
                }
            }
            else
            {
                if (p.cutsceneEnded != null)
                {
                    p.cutsceneEnded.Dispose();
                    p.cutsceneEnded = null;
                    PluginLog.Verbose("Disabling cutscene ended module");
                }
                else
                {
                    PluginLog.Verbose("cutscene ended module already disabled");
                }
            }
        }
    }
}
