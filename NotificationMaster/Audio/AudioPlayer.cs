using ECommons.ImGuiMethods;
using ECommons.Logging;
using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationMaster;

internal class AudioPlayer : IDisposable
{
    private BlockingCollection<(string path, bool stopOnFocus, float volume, bool repeat)> Playlist;
    private bool StopAudio = false;
    private bool threadStarted = false;
    private NotificationMaster p;
    internal AudioPlayer(NotificationMaster plugin)
    {
        Playlist = [];
        p = plugin;
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
                        using(var audioFile = new AudioFileReader(audio.path))
                        using(var outputDevice = new WasapiOut())
                        {
                            audioFile.Volume = audio.volume;
                            outputDevice.Init(audioFile);
                            outputDevice.Play();
                            while(Playlist.Count == 0
                            && !StopAudio
                            && !p.IsDisposed
                            && !(audio.stopOnFocus && p.ThreadUpdActivated.IsApplicationActivated))
                            {
                                if(outputDevice.PlaybackState == PlaybackState.Playing)
                                {
                                    Thread.Sleep(100);
                                }
                                else
                                {
                                    if(audio.repeat)
                                    {
                                        audioFile.Position = 0;
                                        outputDevice.Play();
                                        Thread.Sleep(100);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            PluginLog.Debug($"Stopping device {audio.path}");
                            outputDevice.Stop();
                        }
                    }
                    catch(Exception e)
                    {
                        PluginLog.Error(e.Message + "\n" + e.StackTrace ?? "");
                        new TickScheduler(delegate
                        {
                            Notify.Error(
                                $"Error during playing audio file:\n{e.Message}");
                        }, Svc.Framework);
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
