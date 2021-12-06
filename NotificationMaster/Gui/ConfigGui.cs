using Dalamud.Game.Gui.Toast;
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
            if (open)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(650f, 200f));
                if (ImGui.Begin("NotificationMaster configuration", ref open))
                {
                    ImGui.BeginTabBar("##NMtabs");
                    DrawTab("GP notify", DrawGpNotify, p.cfg.gp_Enable);
                    DrawTab("Cutscene ending notify", DrawCutsceneConfig, p.cfg.cutscene_Enable);
                    DrawTab("Chat message notify", DrawChatMessageGui, p.cfg.chatMessage_Enable);
                    DrawTab("Duty pop notify", DrawCfPopConfig, p.cfg.cfPop_Enable);
                    DrawTab("Server connection error notify", DrawLoginErrorConfig, p.cfg.loginError_Enable);
                    ImGui.EndTabBar();
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

        string[] HttpTypes = { "GET", "POST", "JSON POST" };
        void DrawHttpMaster(List<HttpRequestElement> l, ref bool enable, string placeholders = "")
        {
            ImGui.Checkbox("##PerformRequests", ref enable);
            ImGui.SameLine();
            if(ImGui.CollapsingHeader("Perform following HTTP requests:"))
            {
                ImGui.TextUnformatted("You may use following placeholders:\n"+placeholders);
                if (ImGui.Button("-  Add  -"))
                {
                    l.Add(new HttpRequestElement());
                }
                var i = 0;
                var toDelete = -1;
                foreach (var e in l)
                {
                    i++;
                    if (ImGui.Button("Delete##" + i) && ImGui.GetIO().KeyCtrl)
                    {
                        toDelete = i - 1;
                    }
                    if (ImGui.IsItemHovered()) ImGui.SetTooltip("Hold CTRL + click to delete");
                    ImGui.SameLine();
                    ImGui.Text("URL:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100f);
                    ImGui.Combo("##type" + i, ref e.type, HttpTypes, HttpTypes.Length);
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    ImGui.InputText("##url" + i, ref e.URI, 100000);
                    ImGui.Text("Content:");
                    ImGui.InputTextMultiline("##MultilineContent"+i, ref e.Content, 1000000, new Vector2(ImGui.GetContentRegionAvail().X, Math.Min((e.Content.Split('\n').Length+1)*ImGui.CalcTextSize("AAAAAAAA").Y, 300f)));
                    ImGui.Separator();
                }
                try
                {
                    if (toDelete >= 0)
                    {
                        l.RemoveAt(toDelete);
                        toDelete = -1;
                    }
                }
                catch(Exception e)
                {
                    PluginLog.Error($"Error: {e.Message}\n{e.StackTrace ?? ""}");
                }
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
