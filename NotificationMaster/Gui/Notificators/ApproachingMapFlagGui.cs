namespace NotificationMaster;

internal unsafe partial class ConfigGui
{
    internal void DrawMapFlagConfig()
    {
        /*ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
        ImGui.TextWrapped("Warning: this feature is EXPERIMENTAL!");
        ImGui.PopStyleColor();*/
        var curPosEnable = ImGui.GetCursorPos();
        if(ImGui.Checkbox("Enable", ref p.cfg.mapFlag_Enable))
        {
            ApproachingMapFlag.Setup(p.cfg.mapFlag_Enable, p);
        }
        if(p.cfg.mapFlag_Enable)
        {
            var curPosCont = ImGui.GetCursorPos();
            var distance = 0f;
            ImGui.SetCursorPos(new Vector2(500f, curPosEnable.Y));
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
            ImGui.Text("Debug info: ");
            ImGui.SetCursorPosX(500f);
            ImGui.Text($"Flag state: {p.mapFlag.isFlagSet}");
            ImGui.SetCursorPosX(500f);
            ImGui.Text($"Flag territory: {p.mapFlag.flagTerritory}");
            ImGui.SetCursorPosX(500f);
            ImGui.Text($"Flag X: {p.mapFlag.flagX}");
            ImGui.SetCursorPosX(500f);
            ImGui.Text($"Flag Y: {p.mapFlag.flagY}");
            if(Svc.ClientState.LocalPlayer != null)
            {
                ImGui.SetCursorPosX(500f);
                ImGui.Text($"Player X: {Svc.ClientState.LocalPlayer.Position.X}");
                ImGui.SetCursorPosX(500f);
                ImGui.Text($"Player Y: {Svc.ClientState.LocalPlayer.Position.Z}");
                ImGui.SetCursorPosX(500f);
                ImGui.Text($"Territory: {Svc.ClientState.TerritoryType}");
                ImGui.SetCursorPosX(500f);
                distance = Vector2.Distance(new Vector2(p.mapFlag.flagX, p.mapFlag.flagY),
                    new Vector2(Svc.ClientState.LocalPlayer.Position.X, Svc.ClientState.LocalPlayer.Position.Z));
                ImGui.Text($"Distance: {distance}");
            }
            ImGui.PopStyleColor();
            ImGui.SetCursorPos(curPosCont);
            ImGui.Text("When getting close to map flag if FFXIV is running in background:");
            ImGui.Checkbox("Show tray notification", ref p.cfg.mapFlag_ShowToastNotification);
            ImGui.Checkbox("Flash taskbar icon", ref p.cfg.mapFlag_FlashTrayIcon);
            ImGui.Checkbox("Bring FFXIV to foreground", ref p.cfg.mapFlag_AutoActivateWindow);
            ImGui.Checkbox("Execute actions even if game is active", ref p.cfg.mapFlag_AlwaysExecute);
            ForegroundWarning(p.cfg.mapFlag_AutoActivateWindow);
            ImGui.SetNextItemWidth(100f);
            ImGui.DragInt("Distance to marker", ref p.cfg.mapFlag_TriggerDistance);
            ImGui.Text("Note: this is in-game coordinates distance, not map coordinates distance.");
            if(p.mapFlag.isFlagSet && Svc.ClientState.TerritoryType == p.mapFlag.flagTerritory)
            {
                ImGui.Text($"You are currently {distance:0} yalms away from currently set marker.");
            }
            else
            {
                ImGui.Text("Set flag on your map to see your current distance to it");
            }
            ImGui.Checkbox("Also trigger on crossing X/Y flag axis before reaching set distance", ref p.cfg.mapFlag_TriggerOnCross);
            ImGui.SetNextItemWidth(100f);
            ImGui.DragInt("Axis cross tolerance", ref p.cfg.mapFlag_CrossDelta);
            DrawSoundSettings(ref p.cfg.mapFlag_SoundSettings);
            DrawHttpMaster(p.cfg.mapFlag_HttpRequests, ref p.cfg.mapFlag_HttpRequestsEnable,
                "None available");
        }
    }
}
