using NotificationMasterAPI;

namespace NotificationMaster;

internal class IPC
{
    internal IPC()
    {
        Svc.PluginInterface.GetIpcProvider<string, string, string, bool>(NMAPINames.DisplayToastNotification).RegisterFunc(DisplayToastNotification);
        Svc.PluginInterface.GetIpcProvider<string, bool>(NMAPINames.FlashTaskbarIcon).RegisterFunc(FlashTaskbarIcon);
        Svc.PluginInterface.GetIpcProvider<string, string, float, bool, bool, bool>(NMAPINames.PlaySound).RegisterFunc(PlaySound);
        Svc.PluginInterface.GetIpcProvider<string, bool>(NMAPINames.StopSound).RegisterFunc(StopSound);
        Svc.PluginInterface.GetIpcProvider<string, bool>(NMAPINames.BringGameForeground).RegisterFunc(BringGameForeground);
        Svc.PluginInterface.GetIpcProvider<object>(NMAPINames.Active).RegisterAction(Active);
    }

    public void Dispose()
    {
        Svc.PluginInterface.GetIpcProvider<string, string, string, bool>(NMAPINames.DisplayToastNotification).UnregisterFunc();
        Svc.PluginInterface.GetIpcProvider<string, bool>(NMAPINames.FlashTaskbarIcon).UnregisterFunc();
        Svc.PluginInterface.GetIpcProvider<string, string, float, bool, bool, bool>(NMAPINames.PlaySound).UnregisterFunc();
        Svc.PluginInterface.GetIpcProvider<string, bool>(NMAPINames.StopSound).UnregisterFunc();
        Svc.PluginInterface.GetIpcProvider<string, bool>(NMAPINames.BringGameForeground).UnregisterFunc();
        Svc.PluginInterface.GetIpcProvider<string, object>(NMAPINames.Active).UnregisterAction();
    }

    private void Active() { }

    private bool BringGameForeground(string requesterPlugin)
    {
        PluginLog.Debug($"{requesterPlugin} requests to bring game foreground");
        try
        {
            Native.Impl.Activate();
            return true;
        }
        catch(Exception ex)
        {
            ex.Log();
            return false;
        }
    }

    private bool StopSound(string requesterPlugin)
    {
        PluginLog.Debug($"{requesterPlugin} requests to stop sound");
        try
        {
            P.audioPlayer.Stop();
            return true;
        }
        catch(Exception ex)
        {
            ex.Log();
            return false;
        }
    }

    private bool PlaySound(string requesterPlugin, string path, float volume, bool repeat, bool stopOnceFocused)
    {
        PluginLog.Debug($"{requesterPlugin} requests to play {path} at vol={volume}, repeat={repeat}, stopOnceFocused={stopOnceFocused}");
        try
        {
            P.audioPlayer.Play(new()
            {
                PlaySound = true,
                Volume = volume,
                SoundPath = path,
                Repeat = repeat,
                StopSoundOnceFocused = stopOnceFocused
            });
            return true;
        }
        catch(Exception ex)
        {
            ex.Log();
            return false;
        }
    }

    private bool FlashTaskbarIcon(string requesterPlugin)
    {
        PluginLog.Debug($"{requesterPlugin} requests to flash taskbar icon");
        try
        {
            Native.Impl.FlashWindow();
            return true;
        }
        catch(Exception ex)
        {
            ex.Log();
            return false;
        }
    }

    private bool DisplayToastNotification(string requesterPlugin, string title, string text)
    {
        PluginLog.Debug($"{requesterPlugin} requests to display notification:\n{title}\n{text}");
        try
        {
            TrayIconManager.ShowToast(text, title);
            return true;
        }
        catch(Exception ex)
        {
            ex.Log();
            return false;
        }
    }
}
