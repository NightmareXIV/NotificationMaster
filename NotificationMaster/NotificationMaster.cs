using Dalamud.Game.Command;
using ECommons.Configuration;
using ECommons.EzIpcManager;
using NotificationMaster.Notificators;
using NotificationMasterAPI;

namespace NotificationMaster;

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
    internal PartyFinder partyFinder = null;

    internal HttpMaster httpMaster;
    public ThreadUpdateActivatedState ThreadUpdActivated;
    internal AudioSelector fileSelector = new();
    internal AudioPlayer audioPlayer;

    internal long PauseUntil = 0;
    internal static NotificationMaster P;

    internal IPC IPC;
    internal NotificationMasterApi NotificationMasterApi;

    [EzIPC("AutoDuty.IsStopped", false)] internal Func<bool> AutoDutyIsStopped;

    public string Name => "NotificationMaster";

    public NotificationMaster(IDalamudPluginInterface pluginInterface)
    {
        P = this;
        ECommonsMain.Init(pluginInterface, this, Module.DalamudReflector);
        EzConfig.PluginConfigDirectoryOverride = "NotificationMaster";
        new TickScheduler(() =>
        {
            EzConfig.Migrate<Configuration>();
            cfg = EzConfig.Init<Configuration>();
            cfg.Initialize(Svc.PluginInterface);
            httpMaster = new();
            ThreadUpdActivated = new();
            audioPlayer = new(this);

            configGui = new(this);
            Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { configGui.open = true; };

            if(cfg.gp_Enable) GpNotify.Setup(true, this);
            if(cfg.cutscene_Enable) CutsceneEnded.Setup(true, this);
            if(cfg.chatMessage_Enable) ChatMessage.Setup(true, this);
            if(cfg.cfPop_Enable) CfPop.Setup(true, this);
            if(cfg.loginError_Enable) LoginError.Setup(true, this);
            if(cfg.mapFlag_Enable) ApproachingMapFlag.Setup(true, this);
            if(cfg.mobPulled_Enable) MobPulled.Setup(true, this);
            if(cfg.partyFinder_Enable) PartyFinder.Setup(true, this);

            if(Svc.PluginInterface.Reason == PluginLoadReason.Installer)
            {
                configGui.open = true;
                Notify.Warning(
                    "You have installed NotificationMaster plugin. By default, it has no modules enabled. \n" +
                    "A settings window has been opened: please configure the plugin.");
            }
            Svc.Commands.AddHandler("/pnotify", new CommandInfo(OnCommand)
            {
                HelpMessage = "open/close configuration\n" +
                "/pnotify shutup|s [time in minutes] - pause plugin for specified amount of minutes or until restart if time is not specified\n" +
                "/pnotify resume|r - resume plugin operation"
            });
            IPC = new();
            NotificationMasterApi = new(Svc.PluginInterface);
            EzIPC.Init(this);
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
                    Notify.Success("Plugin paused until restart");
                }
                else
                {
                    if(uint.TryParse(args[1], out var minutes))
                    {
                        PauseUntil = Environment.TickCount64 + minutes * 60 * 1000;
                        Notify.Success($"Plugin paused for {minutes} minutes");
                    }
                    else
                    {
                        Notify.Error("Please enter amount of time in minutes");
                    }
                }
            }
            else if(args[0].Equals("resume", StringComparison.OrdinalIgnoreCase) || args[0].Equals("r", StringComparison.OrdinalIgnoreCase))
            {
                PauseUntil = 0;
                Notify.Success("Plugin operation resumed");
            }
            else
            {
                Notify.Error("Invanid command");
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
        GenericHelpers.Safe(() => IPC.Dispose());
        IsDisposed = true;
        ECommonsMain.Dispose();
        P = null;
    }
}
