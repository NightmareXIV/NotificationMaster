﻿namespace NotificationMaster;

internal partial class ConfigGui
{
    internal void DrawLoginErrorConfig()
    {
        if(ImGui.Checkbox("Enable", ref p.cfg.loginError_Enable))
        {
            LoginError.Setup(p.cfg.loginError_Enable, p);
        }
        if(p.cfg.loginError_Enable)
        {
            //ImGui.TextColored(ImGuiColors.DalamudOrange, "Please note that this function is in testing. ");
            ImGui.Text($"When server connection error occurs, do the following{(p.cfg.loginError_AlwaysExecute ? "" : " if FFXIV is running in background")}:");
            ImGui.Checkbox("Show tray notification", ref p.cfg.loginError_ShowToastNotification);
            ImGui.Checkbox("Flash taskbar icon", ref p.cfg.loginError_FlashTrayIcon);
            ImGui.Checkbox("Bring FFXIV to foreground", ref p.cfg.loginError_AutoActivateWindow);
            ForegroundWarning(p.cfg.loginError_AutoActivateWindow);
            DrawSoundSettings(ref p.cfg.loginError_SoundSettings);
            ImGui.Checkbox("Execute actions even if game is active", ref p.cfg.loginError_AlwaysExecute);
            DrawHttpMaster(p.cfg.loginError_HttpRequests, ref p.cfg.loginError_HttpRequestsEnable,
                "None");
        }
    }
}
