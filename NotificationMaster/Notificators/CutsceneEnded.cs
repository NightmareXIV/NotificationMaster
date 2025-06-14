﻿namespace NotificationMaster;

internal class CutsceneEnded : IDisposable
{
    private bool isInCutscene = false;
    private NotificationMaster p;

    public void Dispose()
    {
        Svc.Framework.Update -= HandleFrameworkUpdate;
    }

    public CutsceneEnded(NotificationMaster plugin)
    {
        p = plugin;
        Svc.Framework.Update += HandleFrameworkUpdate;
    }

    private void HandleFrameworkUpdate(object _)
    {
        if(Svc.ClientState == null) return;
        if(p.PauseUntil > Environment.TickCount64) return;
        var c = Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]
            || Svc.Condition[ConditionFlag.WatchingCutscene78];
        if(isInCutscene && !c && (!Utils.IsApplicationActivated || p.cfg.cutscene_AlwaysExecute) &&
            (!p.cfg.cutscene_OnlyMSQ || Svc.ClientState.TerritoryType.EqualsAny<ushort>(1043, 1044, 1048)))
        {
            if(p.cfg.cutscene_FlashTrayIcon)
            {
                Native.Impl.FlashWindow();
            }
            if(p.cfg.cutscene_AutoActivateWindow) Native.Impl.Activate();
            if(p.cfg.cutscene_ShowToastNotification)
            {
                TrayIconManager.ShowToast("Cutscene ended");
            }
            if(p.cfg.cutscene_HttpRequestsEnable)
            {
                p.httpMaster.DoRequests(p.cfg.cutscene_HttpRequests,
                    new string[][] { }
                );
            }
            if(p.cfg.cutscene_SoundSettings.PlaySound)
            {
                p.audioPlayer.Play(p.cfg.cutscene_SoundSettings);
            }
        }
        isInCutscene = c;
    }


    internal static void Setup(bool enable, NotificationMaster p)
    {
        if(enable)
        {
            if(p.cutsceneEnded == null)
            {
                p.cutsceneEnded = new CutsceneEnded(p);
                PluginLog.Information("Enabling cutscene ended module");
            }
            else
            {
                PluginLog.Information("cutscene ended module already enabled");
            }
        }
        else
        {
            if(p.cutsceneEnded != null)
            {
                p.cutsceneEnded.Dispose();
                p.cutsceneEnded = null;
                PluginLog.Information("Disabling cutscene ended module");
            }
            else
            {
                PluginLog.Information("cutscene ended module already disabled");
            }
        }
    }
}
