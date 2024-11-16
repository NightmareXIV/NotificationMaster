using System;

namespace NotificationMaster;

[Serializable]
internal class SoundSettings
{
    public bool PlaySound = false;
    public bool StopSoundOnceFocused = true;
    public string SoundPath = "";
    public float Volume = 1.0f;
    public bool Repeat = false;
}
