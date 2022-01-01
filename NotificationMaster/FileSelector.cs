using Dalamud.Interface.Internal.Notifications;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NotificationMaster
{
    internal class FileSelector
    {
        SemaphoreSlim SelectorSemaphore;
        
        internal FileSelector()
        {
            SelectorSemaphore = new(1, 1);
        }

        internal bool IsSelecting()
        {
            return SelectorSemaphore.CurrentCount == 0;
        }

        internal void SelectFile(SoundSettings settings)
        {
            if (SelectorSemaphore.Wait(0))
            {
                new Thread((ThreadStart)delegate
                {
                    PluginLog.Information("Starting file selection");
                    try
                    {
                        OpenFileName ofn = new OpenFileName();

                        if (Native.Impl.TryFindGameWindow(out var hwnd))
                        {
                            PluginLog.Information($"With owner: {hwnd:X16}");
                            ofn.dlgOwner = hwnd;
                        }

                        ofn.structSize = Marshal.SizeOf(ofn);

                        ofn.filter = $"Common audio formats\0{Data.CommonAudioFormats}" +
                        $"\0All files\0*\0";

                        ofn.file = new String(new char[1024]);
                        ofn.maxFile = ofn.file.Length;

                        ofn.fileTitle = new String(new char[256]);
                        ofn.maxFileTitle = ofn.fileTitle.Length;

                        ofn.initialDir = TryGetFolderFromPath(settings.SoundPath);
                        ofn.title = "Select a sound file";

                        PluginLog.Information("Preparing to call winapi");
                        if (LibWrap.GetOpenFileName(ofn))
                        {
                            settings.SoundPath = ofn.file;
                        }
                        PluginLog.Information("Dialog closed");
                    }
                    catch (Exception e)
                    {
                        PluginLog.Error(e.Message + "\n" + e.StackTrace ?? "");
                        new TickScheduler(delegate
                        {
                            Svc.PluginInterface.UiBuilder.AddNotification($"Error: {e.Message}", "NotificationMaster", NotificationType.Error);
                        }, Svc.Framework);
                    }
                    SelectorSemaphore.Release();
                    PluginLog.Information("Ending file selection");
                }).Start();
            }
            else
            {
                Svc.PluginInterface.UiBuilder.AddNotification("Failed to open file dialog", "NotificationMaster", NotificationType.Error);
            }
        }

        string TryGetFolderFromPath(string path)
        {
            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory)) return null;
                return Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            }
            catch(Exception) { }
            return null;
        }

        class Win32Window : IWin32Window
        {
            public IntPtr Handle { get; set; }
        }
    }
}
