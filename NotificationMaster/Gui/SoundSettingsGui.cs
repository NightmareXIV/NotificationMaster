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
                if (ImGui.Button("Browse..."))
                {
                    p.fileSelector.SelectFile(settings);
                }
                if (ImGui.Button("Test"))
                {
                    p.audioPlayer.Play(settings.SoundPath, false, settings.Volume, settings.Repeat);
                }
                ImGui.SameLine();
                if (ImGui.Button("Stop"))
                {
                    p.audioPlayer.Stop();
                }
                ImGui.SameLine();
                if(ImGui.Checkbox("Stop playing once game is focused", ref settings.StopSoundOnceFocused))
                {
                    if (!settings.StopSoundOnceFocused) settings.Repeat = false;
                }
                ImGui.SameLine();
                if (ImGui.Checkbox("Repeat", ref settings.Repeat))
                {
                    if (settings.Repeat) settings.StopSoundOnceFocused = true;
                }
                ImGui.SameLine();
                ImGui.Text("|  Volume: ");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                ImGui.SliderFloat("##volume", ref settings.Volume, 0f, 1f);
            }
        }
    }
}
