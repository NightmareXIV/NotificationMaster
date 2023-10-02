using ECommons.Logging;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NotificationMaster
{
    internal partial class ConfigGui
    {
        string[] HttpTypes = { "GET", "POST", "JSON POST" };
        void DrawHttpMaster(List<HttpRequestElement> l, ref bool enable, string placeholders = "")
        {
            ImGui.Checkbox("##PerformRequests", ref enable);
            ImGui.SameLine();
            if (ImGui.CollapsingHeader("Perform following HTTP requests:"))
            {
                ImGui.TextUnformatted("You may use following placeholders:\n" + placeholders);
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
                    ImGui.InputTextMultiline("##MultilineContent" + i, ref e.Content, 1000000, new Vector2(ImGui.GetContentRegionAvail().X, Math.Min((e.Content.Split('\n').Length + 1) * ImGui.CalcTextSize("AAAAAAAA").Y, 300f)));
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
                catch (Exception e)
                {
                    PluginLog.Error($"Error: {e.Message}\n{e.StackTrace ?? ""}");
                }
            }
        }
    }
}
