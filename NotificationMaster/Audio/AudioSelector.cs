using System.IO;
using System.Threading;

namespace NotificationMaster;

internal class AudioSelector
{
    private SemaphoreSlim SelectorSemaphore;

    internal AudioSelector()
    {
        SelectorSemaphore = new(1, 1);
    }

    internal bool IsSelecting()
    {
        return SelectorSemaphore.CurrentCount == 0;
    }

    internal void SelectFile(SoundSettings settings)
    {
        if(SelectorSemaphore.Wait(0))
        {
            new Thread((ThreadStart)delegate
            {
                PluginLog.Information("Starting file selection");
                try
                {
                    var ofn = new OpenFileName();

                    if(Native.Impl.TryFindGameWindow(out var hwnd))
                    {
                        PluginLog.Information($"With owner: {hwnd:X16}");
                        ofn.dlgOwner = hwnd;
                    }

                    ofn.structSize = Marshal.SizeOf(ofn);

                    ofn.filter = $"Media foundation formats\0{NotificationMasterAPI.Data.MFAudioFormats}" +
                    $"\0All files\0*\0";

                    ofn.file = new String(new char[1024]);
                    ofn.maxFile = ofn.file.Length;

                    ofn.fileTitle = new String(new char[256]);
                    ofn.maxFileTitle = ofn.fileTitle.Length;

                    ofn.initialDir = TryGetFolderFromPath(settings.SoundPath);
                    ofn.title = "Select a sound file";

                    PluginLog.Information("Preparing to call winapi");
                    if(Native.GetOpenFileName(ofn))
                    {
                        new TickScheduler(delegate
                        {
                            settings.SoundPath = ofn.file;
                        });
                    }
                    PluginLog.Information("Dialog closed");
                }
                catch(Exception e)
                {
                    PluginLog.Error(e.Message + "\n" + e.StackTrace ?? "");
                    new TickScheduler(delegate
                    {
                        Notify.Error($"Error: {e.Message}");
                    });
                }
                SelectorSemaphore.Release();
                PluginLog.Information("Ending file selection");
            }).Start();
        }
        else
        {
            Notify.Error("Failed to open file dialog");
        }
    }

    private string TryGetFolderFromPath(string path)
    {
        try
        {
            var directory = Path.GetDirectoryName(path);
            if(!string.IsNullOrEmpty(directory)) return null;
            return Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        }
        catch(Exception) { }
        return null;
    }

    private class Win32Window
    {
        public IntPtr Handle { get; set; }
    }
}
