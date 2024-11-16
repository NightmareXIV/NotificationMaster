using ECommons;
using ECommons.ImGuiMethods;
using ECommons.Logging;
using ECommons.Reflection;
using ECommons.Schedulers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationMaster;

internal class AudioPlayer : IDisposable
{
    private BlockingCollection<(string path, bool stopOnFocus, float volume, bool repeat)> Playlist;
    private bool StopAudio = false;
    private bool threadStarted = false;
    private Dictionary<string, Assembly> NAudio = [];
    internal AudioPlayer(NotificationMaster plugin)
    {
        try
        {
            if(DalamudReflector.TryGetLocalPlugin(out _, out var context, out _))
            {
                var resources = P.GetType().Assembly.GetManifestResourceNames();
                //PluginLog.Debug($"Res: {resources.Print("\n")}");
                foreach(var r in resources)
                {
                    if(r.EndsWith(".dll"))
                    {
                        var key = r[..^4].Split('.').Skip(2).Join("."); 
                        using var stream = P.GetType().Assembly.GetManifestResourceStream(r);
                        NAudio[key] = context.LoadFromStream(stream);
                        PluginLog.Debug($"Loading {key}");
                    }
                }
            }
            else
            {
                PluginLog.Warning($"Could not find LocalPlugin");
            }
        }
        catch(Exception e)
        {
            e.Log();
        }
        Playlist = [];
    }

    private void BeginThread()
    {
        if(threadStarted) return;
        threadStarted = true;
        PluginLog.Information("Starting audio player thread");
        new Thread((ThreadStart)delegate
        {
            PluginLog.Information("Audio player thread begins");
            try
            {
                while(!Playlist.IsCompleted)
                {
                    var audio = Playlist.Take();
                    if(Playlist.Count != 0)
                    {
                        PluginLog.Warning("Playlist count was not 0, skipping current item...");
                        continue;
                    }
                    StopAudio = false;
                    PluginLog.Debug($"Beginning playing {audio.path}");
                    try
                    {
                        var audioFileReaderType = NAudio["NAudio"].GetType("NAudio.Wave.AudioFileReader");
                        var waveOutType = NAudio["NAudio.WinForms"].GetType("NAudio.Wave.WaveOut");
                        var audioFile = Activator.CreateInstance(audioFileReaderType, [audio.path]);
                        var outputDevice = Activator.CreateInstance(waveOutType);
                        {
                            audioFile.SetFoP("Volume", audio.volume);
                            outputDevice.Call("Init", [audioFile], true);
                            outputDevice.Call("Play", [], true);
                            while(Playlist.Count == 0
                            && !StopAudio
                            && !P.IsDisposed
                            && !(audio.stopOnFocus && Utils.IsApplicationActivated))
                            {
                                if(outputDevice.GetFoP<int>("PlaybackState") == 1)
                                {
                                    Thread.Sleep(100);
                                }
                                else
                                {
                                    if(audio.repeat)
                                    {
                                        audioFile.SetFoP("Position", 0);
                                        outputDevice.Call("Play", [], true);
                                        Thread.Sleep(100);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            PluginLog.Debug($"Stopping device {audio.path}");
                            outputDevice.Call("Stop", [], true);
                        }
                        outputDevice.Call("Dispose", [], true);
                        audioFile.Call("Dispose", [], true);
                    }
                    catch(Exception e)
                    {
                        PluginLog.Error(e.Message + "\n" + e.StackTrace ?? "");
                        new TickScheduler(delegate
                        {
                            Notify.Error(
                                $"Error during playing audio file:\n{e.Message}");
                        });
                    }
                    PluginLog.Debug($"Stopping playing {audio.path}");
                }
            }
            catch(InvalidOperationException e)
            {
                PluginLog.Information("Not an error: " + e.Message + "\n" + e.StackTrace ?? "");
            }
            catch(Exception e)
            {
                PluginLog.Error(e.Message + "\n" + e.StackTrace ?? "");
            }
            PluginLog.Information("Stopping audio player thread");
        }).Start();
    }

    public void Play(string path, bool stopOnFocus, float volume, bool repeat)
    {
        if(!threadStarted) BeginThread();
        if(!Playlist.TryAdd((path, stopOnFocus, volume, repeat)))
        {
            var timeBegin = Environment.TickCount64;
            Task.Run(delegate
            {
                Playlist.Add((path, stopOnFocus, volume, repeat));
                PluginLog.Warning($"Took extra {Environment.TickCount64 - timeBegin}ms to add audio into playlist");
            });
        }
    }

    public void Play(SoundSettings s)
    {
        Play(s.SoundPath, s.StopSoundOnceFocused, s.Volume, s.Repeat);
    }

    public void Stop()
    {
        StopAudio = true;
    }

    public void Dispose()
    {
        StopAudio = true;
        Playlist.CompleteAdding();
    }
}
