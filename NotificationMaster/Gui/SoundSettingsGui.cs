using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationMaster
{
    internal partial class ConfigGui
    {
        void DrawSoundSettings(ref SoundSettings settings)
        {
            ImGui.Checkbox("Play sound", ref settings.PlaySound);
            if (settings.PlaySound)
            {
                ImGui.Text("Path to file: ");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 100);
                ImGui.InputText("##PathToFile", ref settings.SoundPath, 1000);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                if (ImGui.Button("Browse..."))
                {
                    p.fileSelector.SelectFile(settings);
                }
            }
        }
    }
}
