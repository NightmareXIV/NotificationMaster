using System.IO;
using System.Reflection;

namespace NotificationMaster;

public enum FishBiteType : byte
{
    Unknown = 0,
    Light = 36,
    Medium = 37,
    Heavy = 38,
    None = 255,
}

internal class FishBite : IDisposable
{
    private NotificationMaster p;
    private IntPtr _tugTypeAddress = IntPtr.Zero;
    private FishBiteType _lastBite = FishBiteType.None;

    // Signature from AutoHook plugin
    private const string TugTypeSignature = "48 8D 35 ?? ?? ?? ?? 4C 8B CE";

    public FishBite(NotificationMaster plugin)
    {
        p = plugin;
        try
        {
            _tugTypeAddress = Svc.SigScanner.GetStaticAddressFromSig(TugTypeSignature);
            PluginLog.Debug($"FishBite: Found TugType address: {_tugTypeAddress:X}");
        }
        catch (Exception e)
        {
            PluginLog.Error($"FishBite: Could not find TugType signature: {e.Message}\n{e.StackTrace ?? ""}");
        }

        Svc.Framework.Update += OnFrameworkUpdate;
    }

    public void Dispose()
    {
        Svc.Framework.Update -= OnFrameworkUpdate;
    }

    private unsafe void OnFrameworkUpdate(object _)
    {
        if (_tugTypeAddress == IntPtr.Zero)
            return;

        var currentBite = *(FishBiteType*)_tugTypeAddress;

        // Only trigger on state change to a valid bite
        if (currentBite != _lastBite && currentBite != FishBiteType.None && currentBite != FishBiteType.Unknown)
        {
            _lastBite = currentBite;
            OnBite(currentBite);
        }
        else if (currentBite == FishBiteType.None || currentBite == FishBiteType.Unknown)
        {
            _lastBite = currentBite;
        }
    }

    private void OnBite(FishBiteType bite)
    {
        if (p.PauseUntil > Environment.TickCount64) return;
        if (Utils.IsApplicationActivated && !p.cfg.fishBite_AlwaysExecute) return;

        // Debug: show raw bite value
        PluginLog.Debug($"FishBite: OnBite called with value {(byte)bite} ({bite})");

        var biteName = bite switch
        {
            FishBiteType.Light => "light",
            FishBiteType.Medium => "medium",
            FishBiteType.Heavy => "heavy",
            _ => $"unknown({(byte)bite})"
        };

        var isEnabled = bite switch
        {
            FishBiteType.Light => p.cfg.fishBite_LightEnabled,
            FishBiteType.Medium => p.cfg.fishBite_MediumEnabled,
            FishBiteType.Heavy => p.cfg.fishBite_HeavyEnabled,
            _ => false
        };

        if (!isEnabled) return;

        PluginLog.Debug($"FishBite: {biteName} bite detected");

        if (p.cfg.fishBite_FlashTrayIcon)
        {
            Native.Impl.FlashWindow();
        }

        if (p.cfg.fishBite_AutoActivateWindow)
        {
            Native.Impl.Activate();
        }

        if (p.cfg.fishBite_ShowToastNotification)
        {
            TrayIconManager.ShowToast($"{char.ToUpper(biteName[0]) + biteName[1..]} bite!", "Fish hooked");
        }

        if (p.cfg.fishBite_ChatMessage)
        {
            Svc.Chat.Print($"[FishNotify] You hook a fish with a {biteName} bite.");
        }

        var soundSettings = bite switch
        {
            FishBiteType.Light => p.cfg.fishBite_LightSoundSettings,
            FishBiteType.Medium => p.cfg.fishBite_MediumSoundSettings,
            FishBiteType.Heavy => p.cfg.fishBite_HeavySoundSettings,
            _ => null
        };

        PluginLog.Debug($"FishBite: soundSettings for {biteName}: PlaySound={soundSettings?.PlaySound}, Path={soundSettings?.SoundPath}");

        if (soundSettings?.PlaySound == true)
        {
            PluginLog.Debug($"FishBite: Playing sound for {biteName}");
            p.audioPlayer.Play(soundSettings);
        }

        if (p.cfg.fishBite_HttpRequestsEnable)
        {
            p.httpMaster.DoRequests(p.cfg.fishBite_HttpRequests,
                new string[][]
                {
                    new string[] { "$B", biteName }
                }
            );
        }
    }

    internal static void ResetToDefaults(NotificationMaster p)
    {
        try
        {
            var configDir = Svc.PluginInterface.ConfigDirectory.FullName;
            var soundsDir = Path.Combine(configDir, "Sounds");
            Directory.CreateDirectory(soundsDir);

            var infoPath = Path.Combine(soundsDir, "Info.wav");
            var alertPath = Path.Combine(soundsDir, "Alert.wav");
            var alarmPath = Path.Combine(soundsDir, "Alarm.wav");

            // Force re-extract sounds
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = new Dictionary<string, string>
            {
                { "NotificationMaster.Sounds.Info.wav", "Info.wav" },
                { "NotificationMaster.Sounds.Alert.wav", "Alert.wav" },
                { "NotificationMaster.Sounds.Alarm.wav", "Alarm.wav" }
            };

            foreach (var (resourceName, fileName) in resourceNames)
            {
                var filePath = Path.Combine(soundsDir, fileName);
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var fileStream = File.Create(filePath);
                    stream.CopyTo(fileStream);
                }
            }

            // Reset light bite settings
            p.cfg.fishBite_LightEnabled = true;
            p.cfg.fishBite_LightSoundSettings.SoundPath = infoPath;
            p.cfg.fishBite_LightSoundSettings.PlaySound = true;
            p.cfg.fishBite_LightSoundSettings.StopSoundOnceFocused = false;
            p.cfg.fishBite_LightSoundSettings.Volume = 1.0f;
            p.cfg.fishBite_LightSoundSettings.Repeat = false;

            // Reset medium bite settings
            p.cfg.fishBite_MediumEnabled = true;
            p.cfg.fishBite_MediumSoundSettings.SoundPath = alertPath;
            p.cfg.fishBite_MediumSoundSettings.PlaySound = true;
            p.cfg.fishBite_MediumSoundSettings.StopSoundOnceFocused = false;
            p.cfg.fishBite_MediumSoundSettings.Volume = 1.0f;
            p.cfg.fishBite_MediumSoundSettings.Repeat = false;

            // Reset heavy bite settings
            p.cfg.fishBite_HeavyEnabled = true;
            p.cfg.fishBite_HeavySoundSettings.SoundPath = alarmPath;
            p.cfg.fishBite_HeavySoundSettings.PlaySound = true;
            p.cfg.fishBite_HeavySoundSettings.StopSoundOnceFocused = false;
            p.cfg.fishBite_HeavySoundSettings.Volume = 1.0f;
            p.cfg.fishBite_HeavySoundSettings.Repeat = false;

            // Reset other settings
            p.cfg.fishBite_ChatMessage = true;
            p.cfg.fishBite_AlwaysExecute = true;
            p.cfg.fishBite_FlashTrayIcon = true;
            p.cfg.fishBite_ShowToastNotification = false;
            p.cfg.fishBite_AutoActivateWindow = false;

            p.cfg.Save();
            Notify.Success("Fish Notify settings reset to defaults");
        }
        catch (Exception e)
        {
            PluginLog.Error($"FishBite: Failed to reset to defaults: {e.Message}\n{e.StackTrace ?? ""}");
            Notify.Error("Failed to reset settings");
        }
    }

    internal static void ExtractDefaultSounds(NotificationMaster p)
    {
        try
        {
            var configDir = Svc.PluginInterface.ConfigDirectory.FullName;
            var soundsDir = Path.Combine(configDir, "Sounds");
            Directory.CreateDirectory(soundsDir);

            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = new Dictionary<string, string>
            {
                { "NotificationMaster.Sounds.Info.wav", "Info.wav" },
                { "NotificationMaster.Sounds.Alert.wav", "Alert.wav" },
                { "NotificationMaster.Sounds.Alarm.wav", "Alarm.wav" }
            };

            foreach (var (resourceName, fileName) in resourceNames)
            {
                var filePath = Path.Combine(soundsDir, fileName);
                if (!File.Exists(filePath))
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using var fileStream = File.Create(filePath);
                        stream.CopyTo(fileStream);
                        PluginLog.Debug($"FishBite: Extracted {fileName} to {filePath}");
                    }
                }
            }

            // Set default sound paths if not already set
            var infoPath = Path.Combine(soundsDir, "Info.wav");
            var alertPath = Path.Combine(soundsDir, "Alert.wav");
            var alarmPath = Path.Combine(soundsDir, "Alarm.wav");

            if (string.IsNullOrEmpty(p.cfg.fishBite_LightSoundSettings.SoundPath) && File.Exists(infoPath))
            {
                p.cfg.fishBite_LightSoundSettings.SoundPath = infoPath;
                p.cfg.fishBite_LightSoundSettings.PlaySound = true;
                p.cfg.fishBite_LightSoundSettings.StopSoundOnceFocused = false;
            }

            if (string.IsNullOrEmpty(p.cfg.fishBite_MediumSoundSettings.SoundPath) && File.Exists(alertPath))
            {
                p.cfg.fishBite_MediumSoundSettings.SoundPath = alertPath;
                p.cfg.fishBite_MediumSoundSettings.PlaySound = true;
                p.cfg.fishBite_MediumSoundSettings.StopSoundOnceFocused = false;
            }

            if (string.IsNullOrEmpty(p.cfg.fishBite_HeavySoundSettings.SoundPath) && File.Exists(alarmPath))
            {
                p.cfg.fishBite_HeavySoundSettings.SoundPath = alarmPath;
                p.cfg.fishBite_HeavySoundSettings.PlaySound = true;
                p.cfg.fishBite_HeavySoundSettings.StopSoundOnceFocused = false;
            }

            p.cfg.Save();
        }
        catch (Exception e)
        {
            PluginLog.Error($"FishBite: Failed to extract default sounds: {e.Message}\n{e.StackTrace ?? ""}");
        }
    }

    internal static void Setup(bool enable, NotificationMaster p)
    {
        if (enable)
        {
            if (p.fishBite == null)
            {
                ExtractDefaultSounds(p);
                p.fishBite = new FishBite(p);
                PluginLog.Information("Enabling fishBite module");
            }
            else
            {
                PluginLog.Information("fishBite module already enabled");
            }
        }
        else
        {
            if (p.fishBite != null)
            {
                p.fishBite.Dispose();
                p.fishBite = null;
                PluginLog.Information("Disabling fishBite module");
            }
            else
            {
                PluginLog.Information("fishBite module already disabled");
            }
        }
    }
}
