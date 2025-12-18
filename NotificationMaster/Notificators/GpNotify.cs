using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Command;

namespace NotificationMaster;

internal unsafe class GpNotify : IDisposable
{
    internal int nextTick = 0;
    internal bool needNotification = false;
    private NotificationMaster p;
    public const byte PotionCDGroup = 68;

    public void Dispose()
    {
        Svc.Framework.Update -= Tick;
        Svc.Commands.RemoveHandler("/gp");
    }

    public GpNotify(NotificationMaster plugin)
    {
        p = plugin;
        Svc.Framework.Update += Tick;
        Svc.Commands.AddHandler("/gp", new CommandInfo(OnCommand)
        {
            HelpMessage = "open config\n/gp <number> → set trigger GP amount"
        });
    }

    private void OnCommand(string command, string arguments)
    {
        if(arguments == "")
        {
            p.configGui.open = true;
        }
        else
        {
            try
            {
                var newgp = int.Parse(arguments.Trim());
                if(newgp < 0)
                {
                    Notify.Error("GP can't be negative");
                }
                else
                {
                    p.cfg.gp_GPTreshold = newgp;
                    p.cfg.Save();
                    Notify.Success("Trigger GP amount set to " + p.cfg.gp_GPTreshold);
                }
            }
            catch(Exception e)
            {
                Notify.Error("Error: " + e.Message);
            }
        }
    }

    private void Tick(object _)
    {
        if(Environment.TickCount < nextTick) return;
        nextTick = Environment.TickCount + 5000;
        if(Svc.ClientState?.LocalPlayer == null) return;
        if((Svc.Objects.LocalPlayer.ClassJob.RowId != 16
            && Svc.Objects.LocalPlayer.ClassJob.RowId != 17
            && Svc.Objects.LocalPlayer.ClassJob.RowId != 18)
            || p.PauseUntil > Environment.TickCount64)
        {
            needNotification = false;
            return;
        }
        var gp = Svc.Objects.LocalPlayer.CurrentGp;
        //pi.Framework.Gui.Chat.Print(actMgr.GetCooldown(ActionManager.PotionCDGroup).IsCooldown + "/" + actMgr.GetCooldown(ActionManager.PotionCDGroup).CooldownElapsed + "/" + actMgr.GetCooldown(ActionManager.PotionCDGroup).CooldownTotal);
        if(FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->GetRecastGroupDetail(PotionCDGroup)->IsActive == false) gp += (uint)p.cfg.gp_PotionCapacity;
        //pi.Framework.Gui.Chat.Print(DateTimeOffset.Now + ": " + gp);
        if(gp >= p.cfg.gp_GPTreshold)
        {
            if(needNotification)
            {
                needNotification = false;
                if((!Utils.IsApplicationActivated || p.cfg.gp_AlwaysExecute)
                    && (!p.cfg.gp_SuppressIfNoNodes || Svc.Objects.Any(x => x.ObjectKind == ObjectKind.GatheringPoint)))
                {
                    if(p.cfg.gp_FlashTrayIcon)
                    {
                        Native.Impl.FlashWindow();
                    }
                    if(p.cfg.gp_AutoActivateWindow) Native.Impl.Activate();
                    if(p.cfg.gp_ShowToastNotification)
                    {
                        TrayIconManager.ShowToast(gp + " GP ready!");
                    }

                    if(p.cfg.gp_HttpRequestsEnable)
                    {
                        p.httpMaster.DoRequests(p.cfg.gp_HttpRequests,
                            new string[][]
                            {
                                    new string[] {"$G", gp.ToString()},
                            }
                        );
                    }
                    if(p.cfg.gp_SoundSettings.PlaySound)
                    {
                        p.audioPlayer.Play(p.cfg.gp_SoundSettings);
                    }
                }
            }
        }
        else
        {
            if(gp + p.cfg.gp_Tolerance < p.cfg.gp_GPTreshold)
            {
                needNotification = true;
            }
        }
    }



    internal static void Setup(bool enable, NotificationMaster p)
    {
        if(enable)
        {
            if(p.gpNotify == null)
            {
                p.gpNotify = new GpNotify(p);
                PluginLog.Information("Enabling GP notify");
            }
            else
            {
                PluginLog.Information("GP notify module already enabled");
            }
        }
        else
        {
            if(p.gpNotify != null)
            {
                p.gpNotify.Dispose();
                p.gpNotify = null;
                PluginLog.Information("Disabling GP notify");
            }
            else
            {
                PluginLog.Information("GP notify module already disabled");
            }
        }
    }
}
