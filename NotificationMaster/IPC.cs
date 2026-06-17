using ECommons.EzIpcManager;
using NotificationMasterAPI;
using static System.Net.Mime.MediaTypeNames;

namespace NotificationMaster;

internal class IPC
{
    internal IPC()
    {
        EzIPC.Init(this);
    }

    [EzIPC(NMAPINames.Active, false)]
    private void Active() { }

    [EzIPC(NMAPINames.BringGameForeground, false)]
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

    [EzIPC(NMAPINames.StopSound, false)]
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

    [EzIPC(NMAPINames.PlaySound, false)]
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

    [EzIPC(NMAPINames.FlashTaskbarIcon, false)]
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

    [EzIPC(NMAPINames.DisplayToastNotification, false)]
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

    [EzIPC(NMAPINames.IsGameWindowActivated, false)]
    private bool IsGameWindowActivated()
    {
        return P.ThreadUpdActivated.IsApplicationActivated;
    }

    [EzIPC(NMAPINames.SendHttpRequest, false)]
    private bool SendHttpRequests(string requesterPlugin, List<HttpRequestElement> elements, string[][] replacements)
    {
        PluginLog.Debug($"{requesterPlugin} requests to send HTTP requests:\n{elements.Print("\n")}");
        replacements ??= [];
        try
        {
            P.httpMaster.DoRequests(elements, replacements);
            return true;
        }
        catch(Exception e)
        {
            e.Log();
            return false;
        }
    }
}
