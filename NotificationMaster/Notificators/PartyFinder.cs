using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace NotificationMaster.Notificators;

internal unsafe class PartyFinder : IDisposable
{
    private NotificationMaster p;
    private int memberCount = -1;

    private delegate void ShowLogMessageDelegate(RaptureLogModule* module, uint id);
    [Signature("48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 55 48 8D AC 24 ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 85 ?? ?? ?? ?? 33 F6 48 8D B9", DetourName = nameof(ShowLogMessageDetour), Fallibility = Fallibility.Fallible)]
    private Hook<ShowLogMessageDelegate> ShowLogMessageHook { get; init; }


    public void Dispose()
    {
        Svc.Framework.Update -= Tick;

        ShowLogMessageHook?.Disable();
        ShowLogMessageHook?.Dispose();
    }

    public PartyFinder(NotificationMaster plugin)
    {
        p = plugin;
        Svc.Framework.Update += Tick;

        Svc.Hook.InitializeFromAttributes(this);
        ShowLogMessageHook.Enable();
    }

    private void Tick(object _)
    {
        var addon = (AddonPartyList*)Svc.GameGui.GetAddonByName("_PartyList", 1).Address;
        if(addon == null)
        {
            return;
        }

        var oldCount = memberCount;
        memberCount = addon->MemberCount;

        if(oldCount > 1 && memberCount == 1)
        {
            Notify("Party disbanded.");
            return;
        }

        if(!Svc.Condition[ConditionFlag.UsingPartyFinder] ||
            p.cfg.partyFinder_OnlyWhenFilled)
        {
            return;
        }

        if(oldCount != -1 && memberCount > 0 && oldCount != memberCount)
        {
            if(oldCount > memberCount)
            {
                Notify("A player has left the party.");
            }
            else
            {
                Notify("A player has joined the party.");
            }
        }
    }

    private void ShowLogMessageDetour(RaptureLogModule* module, uint id)
    {
        ShowLogMessageHook.Original(module, id);

        if(p.cfg.partyFinder_Delisted && (id == 981 || id == 982 || id == 985 || id == 986 || id == 7448))
        {
            Notify("Party recruitment ended.");
        }
        else if(id == 983 || id == 984 || id == 7451 || id == 7452)
        {
            Notify("Party recruitment ended. All places have been filled.");
        }
    }

    private void Notify(string message)
    {
        if(p.cfg.partyFinder_FlashTrayIcon)
        {
            Native.Impl.FlashWindow();
        }

        if(p.cfg.partyFinder_AutoActivateWindow)
        {
            Native.Impl.Activate();
        }

        if(p.cfg.partyFinder_ShowToastNotification)
        {
            TrayIconManager.ShowToast(message);
        }

        if(p.cfg.partyFinder_SoundSettings.PlaySound)
        {
            p.audioPlayer.Play(p.cfg.partyFinder_SoundSettings);
        }
    }

    internal static void Setup(bool enable, NotificationMaster p)
    {
        if(enable)
        {
            if(p.partyFinder == null)
            {
                p.partyFinder = new PartyFinder(p);
                PluginLog.Information("Enabling partyFinder module");
            }
            else
            {
                PluginLog.Information("partyFinder module already enabled");
            }
        }
        else
        {
            if(p.partyFinder != null)
            {
                p.partyFinder.Dispose();
                p.partyFinder = null;
                PluginLog.Information("Disabling partyFinder module");
            }
            else
            {
                PluginLog.Information("partyFinder module already disabled");
            }
        }
    }
}
