namespace NotificationMaster;

internal partial class ConfigGui
{
    internal void DrawFishBiteConfig()
    {
        if (ImGui.Checkbox("Enable", ref p.cfg.fishBite_Enable))
        {
            FishBite.Setup(p.cfg.fishBite_Enable, p);
        }
        if (p.cfg.fishBite_Enable)
        {
            ImGui.Text("When a fish bites, do the following:");
            ImGui.Checkbox("Show tray notification", ref p.cfg.fishBite_ShowToastNotification);
            ImGui.Checkbox("Flash taskbar icon", ref p.cfg.fishBite_FlashTrayIcon);
            ImGui.Checkbox("Bring FFXIV to foreground", ref p.cfg.fishBite_AutoActivateWindow);
            ImGui.Checkbox("Show chat message", ref p.cfg.fishBite_ChatMessage);
            ImGui.Checkbox("Execute actions even if game is active", ref p.cfg.fishBite_AlwaysExecute);
            ForegroundWarning(p.cfg.fishBite_AutoActivateWindow);

            ImGui.Separator();
            ImGui.Text("Bite type settings:");

            ImGui.PushID("fishbite_light");
            if (ImGui.CollapsingHeader("Light bite (!)", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Indent();
                ImGui.Checkbox("Enabled##light", ref p.cfg.fishBite_LightEnabled);
                if (p.cfg.fishBite_LightEnabled)
                {
                    DrawSoundSettings(ref p.cfg.fishBite_LightSoundSettings);
                }
                ImGui.Unindent();
            }
            ImGui.PopID();

            ImGui.PushID("fishbite_medium");
            if (ImGui.CollapsingHeader("Medium bite (!!)", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Indent();
                ImGui.Checkbox("Enabled##medium", ref p.cfg.fishBite_MediumEnabled);
                if (p.cfg.fishBite_MediumEnabled)
                {
                    DrawSoundSettings(ref p.cfg.fishBite_MediumSoundSettings);
                }
                ImGui.Unindent();
            }
            ImGui.PopID();

            ImGui.PushID("fishbite_heavy");
            if (ImGui.CollapsingHeader("Heavy bite (!!!)", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Indent();
                ImGui.Checkbox("Enabled##heavy", ref p.cfg.fishBite_HeavyEnabled);
                if (p.cfg.fishBite_HeavyEnabled)
                {
                    DrawSoundSettings(ref p.cfg.fishBite_HeavySoundSettings);
                }
                ImGui.Unindent();
            }
            ImGui.PopID();

            ImGui.Separator();
            DrawHttpMaster(p.cfg.fishBite_HttpRequests, ref p.cfg.fishBite_HttpRequestsEnable,
                "$B - bite type (light/medium/heavy)");

            ImGui.Separator();
            if (ImGui.Button("Reset to Defaults"))
            {
                FishBite.ResetToDefaults(p);
            }
            ImGui.SameLine();
            ImGui.TextDisabled("(Restores default sounds and settings)");
        }
    }
}
