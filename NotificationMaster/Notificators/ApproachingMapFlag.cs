using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace NotificationMaster;

internal unsafe class ApproachingMapFlag
{
    private NotificationMaster p;
    internal float flagX => AgentMap.Instance()->FlagMapMarker.XFloat;
    internal float flagY => AgentMap.Instance()->FlagMapMarker.YFloat;
    internal uint flagTerritory => AgentMap.Instance()->FlagMapMarker.TerritoryId;
    internal bool isFlagSet => AgentMap.Instance()->IsFlagMarkerSet;

    public void Dispose()
    {
        Svc.Framework.Update -= ApproachingMapFlagWatcher;
    }

    public ApproachingMapFlag(NotificationMaster plugin)
    {
        p = plugin;
        try
        {
            Svc.Framework.Update += ApproachingMapFlagWatcher;
        }
        catch(Exception e)
        {
            PluginLog.Error($"{e.Message}\n{e.StackTrace ?? ""}");
            Setup(false, plugin);
        }
    }

    private bool IsEnabled = false;
    private bool HasTriggered = false;
    private bool DirectionX;
    private bool DirectionY;
    private void ApproachingMapFlagWatcher(object _)
    {
        if(p.PauseUntil > Environment.TickCount64 || Utils.IsApplicationActivated || Svc.ClientState.LocalPlayer == null ||
            Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51] ||
             isFlagSet == false || flagTerritory != Svc.ClientState.TerritoryType)
        {
            IsEnabled = false;
            HasTriggered = false;
        }
        else
        {
            if(!IsEnabled)
            {
                UpdateDirections();
            }
            if(Vector2.Distance(new Vector2(flagX, flagY),
                new Vector2(Svc.ClientState.LocalPlayer.Position.X,
                Svc.ClientState.LocalPlayer.Position.Z)) <= p.cfg.mapFlag_TriggerDistance)
            {
                if(IsEnabled && !HasTriggered)
                {
                    PluginLog.Debug($"{ImGui.GetFrameCount()} Distance reached, notification fired");
                    DoNotify("You have reached your destination!");
                }
                HasTriggered = true;
            }
            else
            {
                HasTriggered = false;
            }
            if((!DirectionX && flagX > Svc.ClientState.LocalPlayer.Position.X + p.cfg.mapFlag_CrossDelta)
                || (DirectionX && flagX < Svc.ClientState.LocalPlayer.Position.X - p.cfg.mapFlag_CrossDelta))
            {
                if(IsEnabled && !HasTriggered && p.cfg.mapFlag_TriggerOnCross)
                {
                    PluginLog.Debug($"{ImGui.GetFrameCount()} Crossed X line, notification fired");
                    DoNotify("You have crossed your destination border (X)!");
                }
                UpdateDirections();
            }
            if((!DirectionY && flagY > Svc.ClientState.LocalPlayer.Position.Z + p.cfg.mapFlag_CrossDelta)
                || (DirectionY && flagY < Svc.ClientState.LocalPlayer.Position.Z - p.cfg.mapFlag_CrossDelta))
            {
                if(IsEnabled && !HasTriggered && p.cfg.mapFlag_TriggerOnCross)
                {
                    PluginLog.Debug($"{ImGui.GetFrameCount()} Crossed Y line, notification fired");
                    DoNotify("You have crossed your destination border (Y)!");
                }
                UpdateDirections();
            }
            IsEnabled = true;
        }
    }

    private void DoNotify(string s)
    {
        if(p.cfg.mapFlag_FlashTrayIcon)
        {
            Native.Impl.FlashWindow();
        }
        if(p.cfg.mapFlag_AutoActivateWindow) Native.Impl.Activate();
        if(p.cfg.mapFlag_ShowToastNotification)
        {
            TrayIconManager.ShowToast(s, "");
        }
        if(p.cfg.mapFlag_HttpRequestsEnable)
        {
            p.httpMaster.DoRequests(p.cfg.mapFlag_HttpRequests,
                new string[][]
                {
                }
            );
        }
        if(p.cfg.mapFlag_SoundSettings.PlaySound)
        {
            p.audioPlayer.Play(p.cfg.mapFlag_SoundSettings);
        }
    }

    private void UpdateDirections()
    {
        DirectionX = flagX > Svc.ClientState.LocalPlayer.Position.X;
        DirectionY = flagY > Svc.ClientState.LocalPlayer.Position.Z;
        //Svc.Chat.Print($"Directions: {DirectionX}, {DirectionY}");
    }

    internal static void Setup(bool enable, NotificationMaster p)
    {
        if(enable)
        {
            if(p.mapFlag == null)
            {
                p.mapFlag = new ApproachingMapFlag(p);
                PluginLog.Information("Enabling mapFlag module");
            }
            else
            {
                PluginLog.Information("mapFlag module already enabled");
            }
        }
        else
        {
            if(p.mapFlag != null)
            {
                p.mapFlag.Dispose();
                p.mapFlag = null;
                PluginLog.Information("Disabling mapFlag module");
            }
            else
            {
                PluginLog.Information("mapFlag module already disabled");
            }
        }
    }
}
