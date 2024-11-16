using Dalamud.Game.ClientState.Objects.Enums;

namespace NotificationMaster;

internal unsafe partial class ConfigGui
{
    internal void DrawGpNotify()
    {
        var curPosEnable = ImGui.GetCursorPos();
        if(ImGui.Checkbox("Enable##gpn", ref p.cfg.gp_Enable))
        {
            GpNotify.Setup(p.cfg.gp_Enable, p);
        }
        if(p.cfg.gp_Enable)
        {
            var curPosCont = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(500f, curPosEnable.Y));
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
            ImGui.Text("Debug info: ");
            ImGui.SetCursorPosX(500f);
            ImGui.Text($"Nodes around: {Svc.Objects.Count(x => x.ObjectKind == ObjectKind.GatheringPoint)}");
            ImGui.SetCursorPosX(500f);
            ImGui.Text($"Potion cooldown: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->GetRecastGroupDetail(GpNotify.PotionCDGroup)->IsActive}");
            ImGui.PopStyleColor();
            ImGui.SetCursorPos(curPosCont);
            ImGui.SetNextItemWidth(100f);
            ImGui.DragInt("Notify upon reaching this amount of GP", ref p.cfg.gp_GPTreshold, 1f, 0, 10000);
            ImGui.Text("Use command /gp <number> to quickly change this amount");
            ImGui.SetNextItemWidth(100f);
            ImGui.DragInt("Potion capacity", ref p.cfg.gp_PotionCapacity, 1f, 0, 1000);
            ImGui.SetNextItemWidth(100f);
            ImGui.DragInt("Tolerance", ref p.cfg.gp_Tolerance, 1f, 0, 100);
            ImGui.Checkbox("Suppress notification if no potential gathering places are around", ref p.cfg.gp_SuppressIfNoNodes);
            if(ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("If your GP is lower than targeted by not more than this amount, notification will not be sent upon regaining it.");
            }
            ImGui.Text("Notification options:");
            ImGui.Checkbox("Show tray notification", ref p.cfg.gp_ShowToastNotification);
            ImGui.Checkbox("Flash taskbar icon", ref p.cfg.gp_FlashTrayIcon);
            ImGui.Checkbox("Bring FFXIV to foreground", ref p.cfg.gp_AutoActivateWindow);
            ForegroundWarning(p.cfg.gp_AutoActivateWindow);
            DrawSoundSettings(ref p.cfg.gp_SoundSettings);
            DrawHttpMaster(p.cfg.gp_HttpRequests, ref p.cfg.gp_HttpRequestsEnable,
                "$G - available GP");
        }
    }
}
