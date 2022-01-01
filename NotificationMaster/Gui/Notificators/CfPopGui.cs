using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationMaster
{
    partial class ConfigGui
    {
        internal void DrawCfPopConfig()
        {
            if (ImGui.Checkbox("Enable", ref p.cfg.cfPop_Enable))
            {
                CfPop.Setup(p.cfg.cfPop_Enable, p);
            }
            if (p.cfg.cfPop_Enable)
            {
                ImGui.Text("When duty pops, do the following if FFXIV is running in background:");
                ImGui.Checkbox("Show tray notification", ref p.cfg.cfPop_ShowToastNotification);
                ImGui.Checkbox("Flash taskbar icon", ref p.cfg.cfPop_FlashTrayIcon);
                ImGui.Checkbox("Bring FFXIV to foreground", ref p.cfg.cfPop_AutoActivateWindow);
                ForegroundWarning(p.cfg.cfPop_AutoActivateWindow);
                DrawSoundSettings(ref p.cfg.cfPop_SoundSettings);
                ImGui.Checkbox("Repeat in 30 seconds if invitation still not accepted", ref p.cfg.cfPop_NotifyIn30);
                DrawHttpMaster(p.cfg.cfPop_HttpRequests, ref p.cfg.cfPop_HttpRequestsEnable, 
                    "$N - name of the duty\n$T - time left to accept the duty");
            }
        }
    }
}
