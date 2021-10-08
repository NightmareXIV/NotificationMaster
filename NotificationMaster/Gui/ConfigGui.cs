using Dalamud.Game.Gui.Toast;
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
            if (!open) return;
            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(450f, 200f));
            if(ImGui.Begin("NotificationMaster configuration", ref open))
            {
                ImGui.BeginTabBar("##NMtabs");
                DrawTab("GP notify", DrawGpNotify);
                DrawTab("Cutscene ending notify", DrawCutsceneConfig);
                ImGui.EndTabBar();
            }
            ImGui.End();
            if (!open)
            {
                p.cfg.Save();
                Svc.Toasts.ShowQuest("Configuration saved", new QuestToastOptions() { DisplayCheckmark = true, PlaySound = true });
            }
            ImGui.PopStyleVar();
        }

        void DrawTab(string name, Action function)
        {
            if (ImGui.BeginTabItem($"{name}"))
            {
                ImGui.BeginChild($"##{name}-child");
                function();
                ImGui.EndChild();
                ImGui.EndTabItem();
            }
        }
    }
}
