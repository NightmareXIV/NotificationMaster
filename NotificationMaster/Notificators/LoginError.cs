using FFXIVClientStructs.FFXIV.Component.GUI;

namespace NotificationMaster;

internal unsafe class LoginError : IDisposable
{
    private NotificationMaster p;
    private bool seenErrorWindow = false;
    public void Dispose()
    {
        Svc.Framework.Update -= LoginErrorWatcher;
    }

    public LoginError(NotificationMaster plugin)
    {
        p = plugin;
        Svc.Framework.Update += LoginErrorWatcher;
    }

    private void LoginErrorWatcher(object framework)
    {
        if(p.PauseUntil > Environment.TickCount64) return;
        if(!Svc.ClientState.IsLoggedIn)
        {
            var addonPtr = Svc.GameGui.GetAddonByName("Dialogue", 1);
            if(addonPtr != IntPtr.Zero && ((AtkUnitBase*)addonPtr)->IsVisible)
            {
                if(!seenErrorWindow)
                {
                    seenErrorWindow = true;
                    if(p.cfg.loginError_AlwaysExecute || !Utils.IsApplicationActivated)
                    {
                        if(p.cfg.loginError_FlashTrayIcon)
                        {
                            Native.Impl.FlashWindow();
                        }
                        if(p.cfg.loginError_AutoActivateWindow) Native.Impl.Activate();
                        if(p.cfg.loginError_ShowToastNotification)
                        {
                            TrayIconManager.ShowToast("Server connection error occurred!", "");
                        }
                        if(p.cfg.loginError_HttpRequestsEnable)
                        {
                            p.httpMaster.DoRequests(p.cfg.loginError_HttpRequests,
                                new string[][]
                                {
                                }
                            );
                        }
                        if(p.cfg.loginError_SoundSettings.PlaySound)
                        {
                            p.audioPlayer.Play(p.cfg.loginError_SoundSettings);
                        }
                    }
                }
            }
            else
            {
                seenErrorWindow = false;
            }
        }
        else
        {
            seenErrorWindow = false;
        }
    }

    internal static void Setup(bool enable, NotificationMaster p)
    {
        if(enable)
        {
            if(p.loginError == null)
            {
                p.loginError = new LoginError(p);
                PluginLog.Information("Enabling loginError module");
            }
            else
            {
                PluginLog.Information("loginError module already enabled");
            }
        }
        else
        {
            if(p.loginError != null)
            {
                p.loginError.Dispose();
                p.loginError = null;
                PluginLog.Information("Disabling loginError module");
            }
            else
            {
                PluginLog.Information("loginError module already disabled");
            }
        }
    }
}
