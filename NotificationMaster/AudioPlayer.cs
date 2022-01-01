using Dalamud.Logging;
using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationMaster
{
    internal class AudioPlayer : IDisposable
    {
        BlockingCollection<(string path, bool stopOnFocus, float volume)> Playlist;
        bool StopAudio = false;
        bool threadStarted = false;
        NotificationMaster p;
        internal AudioPlayer(NotificationMaster plugin)
        {
            Playlist = new();
            this.p = plugin;
        }

        void BeginThread()
        {
            if (threadStarted) return;
            threadStarted = true;
            PluginLog.Information("Starting audio player thread");
            new Thread((ThreadStart)delegate
            {
                PluginLog.Information("Audio player thread begins");
                try
                {
                    while (!Playlist.IsCompleted)
                    {
                        var audio = Playlist.Take();
                        if (Playlist.Count != 0)
                        {
                            PluginLog.Warning("Playlist count was not 0, skipping current item...");
                            continue;
                        }
                        StopAudio = false;
                        PluginLog.Information($"Beginning playing {audio.path}");
                        using (var audioFile = new AudioFileReader(audio.path))
                        using (var outputDevice = new WaveOutEvent())
                        {
                            audioFile.Volume = audio.volume;
                            outputDevice.Init(audioFile);
                            outputDevice.Play();
                            while (Playlist.Count == 0
                            && !StopAudio
                            && !p.IsDisposed
                            && outputDevice.PlaybackState == PlaybackState.Playing
                            && !(audio.stopOnFocus && p.ThreadUpdActivated.IsApplicationActivated))
                            {
                                //PluginLog.Information($"{plugin.ThreadUpdActivated.IsApplicationActivated}");
                                Thread.Sleep(100);
                            }
                            outputDevice.Stop();
                        }
                        PluginLog.Information($"Stopping playing {audio.path}");
                    }
                }
                catch(InvalidOperationException e)
                {
                    PluginLog.Information("Not an error: " + e.Message + "\n" + e.StackTrace ?? "");
                }
                catch (Exception e)
                {
                    PluginLog.Error(e.Message + "\n" + e.StackTrace ?? "");
                }
                PluginLog.Information("Stopping audio player thread");
            }).Start();
        }

        public void Play(string path, bool stopOnFocus, float volume)
        {
            if (!threadStarted) BeginThread();
            if(!Playlist.TryAdd((path, stopOnFocus, volume)))
            {
                var timeBegin = Environment.TickCount64;
                Task.Run(delegate
                {
                    Playlist.Add((path, stopOnFocus, volume));
                    PluginLog.Warning($"Took extra {Environment.TickCount64 - timeBegin}ms to add audio into playlist");
                });
            }
        }

        public void Play(SoundSettings s)
        {
            Play(s.SoundPath, s.StopSoundOnceFocused, s.Volume);
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
}
