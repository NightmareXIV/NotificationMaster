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
        internal void DrawGpNotify()
        {
            if(ImGui.Checkbox("Enable##gpn", ref p.cfg.gp_Enable))
            {
                GpNotify.Setup(p.cfg.gp_Enable, p);
            }
            if (p.cfg.gp_Enable)
            {
                ImGui.SetNextItemWidth(100f);
                ImGui.DragInt("Notify upon reaching this amount of GP", ref p.cfg.gp_GPTreshold, 1f, 0, 10000);
                ImGui.Text("Use command /gp <number> to quickly change this amount");
                ImGui.SetNextItemWidth(100f);
                ImGui.DragInt("Potion capacity", ref p.cfg.gp_PotionCapacity, 1f, 0, 1000);
                ImGui.SetNextItemWidth(100f);
                ImGui.DragInt("Tolerance", ref p.cfg.gp_Tolerance, 1f, 0, 100);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("If your GP is lower than targeted by not more than this amount, notification will not be sent upon regaining it.");
                }
                ImGui.Text("Notification options:");
                ImGui.Checkbox("Show tray notification", ref p.cfg.gp_ShowToastNotification);
                ImGui.Checkbox("Flash taskbar icon", ref p.cfg.gp_FlashTrayIcon);
                ImGui.Checkbox("Bring FFXIV to foreground", ref p.cfg.gp_AutoActivateWindow);
            }
        }
    }
}
