using ImGuiNET;
using NotificationMaster.Notificators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationMaster
{
    partial class ConfigGui
    {
        internal void DrawPartyFinderConfig()
        {
            if (ImGui.Checkbox("Enable", ref p.cfg.partyFinder_Enable))
            {
                PartyFinder.Setup(p.cfg.partyFinder_Enable, p);
            }
            if (p.cfg.partyFinder_Enable)
            {
                ImGui.Checkbox("Only when the party fills up", ref p.cfg.partyFinder_OnlyWhenFilled);
                ImGui.Checkbox("Notify if the party is delisted", ref p.cfg.partyFinder_Delisted);

                if (p.cfg.partyFinder_OnlyWhenFilled)
                {
                    if (p.cfg.partyFinder_Delisted)
                    {
                        ImGui.Text("When the party fills or is delisted");
                    }
                    else
                    {
                        ImGui.Text("When the party fills");
                    }
                }
                else
                {
                    if (p.cfg.partyFinder_Delisted)
                    {
                        ImGui.Text("When someone joins or leaves the party, or the party is delisted");
                    }
                    else
                    {
                        ImGui.Text("When someone joins or leaves the party");
                    }
                }

                ImGui.Text("do the following if FFXIV is running in background: ");

                ImGui.Checkbox("Show tray notification", ref p.cfg.partyFinder_ShowToastNotification);
                ImGui.Checkbox("Flash taskbar icon", ref p.cfg.partyFinder_FlashTrayIcon);
                ImGui.Checkbox("Bring FFXIV to foreground", ref p.cfg.partyFinder_AutoActivateWindow);
                ForegroundWarning(p.cfg.partyFinder_AutoActivateWindow);
                DrawSoundSettings(ref p.cfg.partyFinder_SoundSettings);
            }
        }
    }
}
