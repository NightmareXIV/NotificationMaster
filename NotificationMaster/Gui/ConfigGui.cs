using Dalamud.Game.Gui.Toast;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NotificationMaster
{
    partial class ConfigGui : IDisposable
    {
        internal bool open = false;
        internal NotificationMaster p;
        internal ConfigGui(NotificationMaster p)
        {
            this.p = p;
            Svc.PluginInterface.UiBuilder.Draw += Draw;
        }

        public void Dispose()
        {
            Svc.PluginInterface.UiBuilder.Draw -= Draw;
        }

        internal void Draw()
        {
            if(p.PauseUntil > Environment.TickCount64)
            {
                ImGuiHelpers.ForceNextWindowMainViewport();
                var sb = new StringBuilder("NotificationMaster is paused");
                if(p.PauseUntil != long.MaxValue)
                {
                    var ts = TimeSpan.FromMilliseconds(p.PauseUntil - Environment.TickCount64);
                    sb.Append($" for {(ts.Days * 60 + ts.Hours):D2}:{ts.Minutes:D2}:{ts.Seconds:D2}");
                }
                var text = sb.ToString();
                var dims = ImGui.CalcTextSize(text);
                ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(ImGuiHelpers.MainViewport.Size.X / 2 - dims.X / 2, 20f));
                ImGui.Begin("NotificationMasterPauseWarning", ImGuiWindowFlags.NoNav
                | ImGuiWindowFlags.NoFocusOnAppearing
                | ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.TextColored(ImGuiColors.DalamudOrange, text);
                ImGui.End();
            }
            if (open)
            {
                
                ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(650f, 200f));
                if (ImGui.Begin("NotificationMaster configuration", ref open))
                {
                    if (p.fileSelector.IsSelecting())
                    {
                        ImGui.Text("Awaiting file selection...");
                    }
                    else
                    {
                        ImGui.BeginTabBar("##NMtabs");
                        DrawTab("GP replenish", DrawGpNotify, p.cfg.gp_Enable);
                        DrawTab("Cutscene ending", DrawCutsceneConfig, p.cfg.cutscene_Enable);
                        DrawTab("Chat message", DrawChatMessageGui, p.cfg.chatMessage_Enable);
                        DrawTab("Duty pop", DrawCfPopConfig, p.cfg.cfPop_Enable);
                        DrawTab("Connection error", DrawLoginErrorConfig, p.cfg.loginError_Enable);
                        DrawTab("Approaching map flag", DrawMapFlagConfig, p.cfg.mapFlag_Enable);
                        DrawTab("Mob pulled", DrawMobPulledConfig, p.cfg.mobPulled_Enable);
                        ImGui.EndTabBar();
                    }
                }
                ImGui.End();
                if (!open)
                {
                    p.cfg.Save();
                    Svc.PluginInterface.UiBuilder.AddNotification("Configuration saved", "NotificationMaster", NotificationType.Success);
                }
                ImGui.PopStyleVar();
            }
        }

        void DrawTab(string name, Action function, bool enabled)
        {
            var colored = false;
            if (enabled)
            {
                colored = true;
                ImGui.PushStyleColor(ImGuiCol.Text, 0xff00ff00);
            }
            if (ImGui.BeginTabItem($"{name}"))
            {
                if (colored) ImGui.PopStyleColor();
                ImGui.BeginChild($"##{name}-child");
                function();
                ImGui.EndChild();
                ImGui.EndTabItem();
            }
            else
            {
                if (colored) ImGui.PopStyleColor();
            }
        }
        
        void ForegroundWarning(bool display)
        {
            if (display)
            {
                ImGui.TextColored(ImGuiColors.DalamudRed, "Unfortunately bringing FFXIV to foreground isn't very reliable function.\nIf it fails to work for you - not much can be done.");
            }
        }
    }
}
