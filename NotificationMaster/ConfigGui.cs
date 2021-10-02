using Dalamud.Game.Internal.Gui.Toast;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationMaster
{
    class ConfigGui : IDisposable
    {
        internal bool open = false;
        NotificationMaster p;
        internal ConfigGui(NotificationMaster p)
        {
            this.p = p;
            p.pi.UiBuilder.OnBuildUi += Draw;
        }

        public void Dispose()
        {
            p.pi.UiBuilder.OnBuildUi -= Draw;
        }

        internal void Draw()
        {
            if (!open) return;
            if(ImGui.Begin("NotificationMaster configuration", ref open, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("General options:");
                ImGui.SetNextItemWidth(100f);
                ImGui.DragInt("Notify upon reaching this amount of GP", ref p.cfg.GPTreshold, 1f, 0, 10000);
                ImGui.Text("Use command /gp <number> to quickly change this amount");
                ImGui.SetNextItemWidth(100f);
                ImGui.DragInt("Potion capacity", ref p.cfg.PotionCapacity, 1f, 0, 1000);
                ImGui.SetNextItemWidth(100f);
                ImGui.DragInt("Tolerance", ref p.cfg.Tolerance, 1f, 0, 100);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("If your GP is lower than targeted by not more than this amount, notification will not be sent upon regaining it.");
                }
                ImGui.Text("Notification options:");
                ImGui.Checkbox("Show tray notification", ref p.cfg.ShowToastNotification);
                ImGui.Checkbox("Flash taskbar icon", ref p.cfg.FlashTrayIcon);
                ImGui.Checkbox("Bring FFXIV to foreground", ref p.cfg.AutoActivateWindow);
            }
            ImGui.End();
            if (!open)
            {
                p.cfg.Save();
                p.pi.Framework.Gui.Toast.ShowQuest("Configuration saved", new QuestToastOptions() { DisplayCheckmark = true, PlaySound = true });
            }
        }
    }
}
