using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationMaster
{
    [Serializable]
    internal class SoundSettings
    {
        public bool PlaySound = false;
        public bool StopSoundOnceFocused = true;
        public string SoundPath = "";
        public float Volume = 1.0f;
    }
}
