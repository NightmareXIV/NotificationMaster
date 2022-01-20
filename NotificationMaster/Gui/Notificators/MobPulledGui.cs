using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NotificationMaster
{
    partial class ConfigGui
    {
        string mobToDelete = null;
        bool mobAllowDeleting = false;
        string mobsToAdd = "";
        (string filter, bool onlyWorld, bool onlySelected) terrSearchOptions = ("", true, false);
        int tCounter = 0;
        internal void DrawMobPulledConfig()
        {
            if (ImGui.Checkbox("Enable", ref p.cfg.mobPulled_Enable))
            {
                MobPulled.Setup(p.cfg.mobPulled_Enable, p);
            }
            if (p.cfg.mobPulled_Enable)
            {
                tCounter = 0;
                if (mobToDelete != null)
                {
                    p.cfg.mobPulled_Names.Remove(mobToDelete);
                    mobToDelete = null;
                    p.mobPulled.RebuildMobNames();
                    p.mobPulled.ClearIgnoredMobs();
                }
                ImGui.TextColored(ImGuiColors.DalamudRed, "Warning! This function is EXPERIMENTAL!\n" +
                    "Additionally, for the time being, it is required that you CAN SEE the mob for plugin to detect it's pull.\n" +
                    "If your area is extremely congested, A ranks may disappear.\n" +
                    "S/SS should always be visible, however. Solution to this problem will come at a later date.");
                ImGui.Text($"When mob from list in specified zones is pulled, do the following{(p.cfg.mobPulled_AlwaysExecute ? "" : " if FFXIV is running in background")}:");
                ImGui.Checkbox("Show tray notification", ref p.cfg.mobPulled_ShowToastNotification);
                ImGui.Checkbox("Flash taskbar icon", ref p.cfg.mobPulled_FlashTrayIcon);
                ImGui.Checkbox("Bring FFXIV to foreground", ref p.cfg.mobPulled_AutoActivateWindow);
                ForegroundWarning(p.cfg.mobPulled_AutoActivateWindow);
                ImGui.Checkbox("Print warning in chat", ref p.cfg.mobPulled_ChatMessage);
                ImGui.Checkbox("Display an in-game toast", ref p.cfg.mobPulled_Toast);
                DrawSoundSettings(ref p.cfg.mobPulled_SoundSettings);
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudOrange);
                ImGui.Checkbox("Execute actions even if game is active", ref p.cfg.mobPulled_AlwaysExecute);
                ImGui.PopStyleColor();
                DrawHttpMaster(p.cfg.mobPulled_HttpRequests, ref p.cfg.mobPulled_HttpRequestsEnable,
                    "$M - mob name");
                if(ImGui.CollapsingHeader($"List of watched mobs (currently contains {p.cfg.mobPulled_Names.Count})###MPListmobs"))
                {
                    ImGui.Checkbox("Allow deleting entries", ref mobAllowDeleting);
                    if (p.cfg.mobPulled_Names.Count > 0) {
                        ImGui.SameLine();
                        if (ImGui.Button("Export mob names to clipboard"))
                        {
                            ImGui.SetClipboardText(string.Join("\n", p.cfg.mobPulled_Names));
                        }
                    }
                    foreach(var s in p.cfg.mobPulled_Names)
                    {
                        if (mobAllowDeleting)
                        {
                            if (ImGui.SmallButton($"Delete##{s.GetHashCode()}"))
                            {
                                mobToDelete = s;
                            }
                            ImGui.SameLine();
                        }
                        ImGui.Text(s);
                    }
                    ImGui.TextColored(ImGuiColors.DalamudOrange, "Add mobs (one per line; case sensitive; extra spaces will be trimmed; duplicates will be removed)");
                    ImGui.InputTextMultiline("##addMobs", ref mobsToAdd, 100000, 
                        new Vector2(ImGui.GetContentRegionAvail().X, Math.Min((mobsToAdd.Split('\n').Length + 1) * ImGui.CalcTextSize("AAAAAAAA").Y, 300f)));
                    if(ImGui.Button($"Add mobs"))
                    {
                        foreach(var mob in mobsToAdd.Split("\n"))
                        {
                            var trimmed = mob.Trim();
                            if(trimmed.Length > 0)
                            {
                                p.cfg.mobPulled_Names.Add(trimmed);
                            }
                        }
                        mobsToAdd = "";
                        p.mobPulled.RebuildMobNames();
                        p.mobPulled.ClearIgnoredMobs();
                    }
                }

                if(ImGui.CollapsingHeader($"List of territories where module will be enabled, currently has {p.cfg.mobPulled_Territories.Count} entries###MPListOfTerr"))
                {
                    ImGui.SetNextItemWidth(200f);
                    ImGui.InputTextWithHint("##terrSearch", "Filter...", ref terrSearchOptions.filter, 100);
                    ImGui.SameLine();
                    ImGui.Checkbox("Only world zones", ref terrSearchOptions.onlyWorld);
                    ImGui.SameLine();
                    ImGui.Checkbox("Only selected", ref terrSearchOptions.onlySelected);
                    if (p.mobPulled.territories.TryGetValue(Svc.ClientState.TerritoryType, out var v)) {
                        MPPrintZone(Svc.ClientState.TerritoryType, v);
                    }
                    foreach(var k in p.mobPulled.territories)
                    {
                        MPPrintZone(k.Key, k.Value);
                    }
                }
            }
        }

        void MPPrintZone(uint territoryType, (string name, bool isWorld) v)
        {
            tCounter++;
            var cname = $"{territoryType} | {v.name}{(v.isWorld?" (world zone)":"")}";
            if (terrSearchOptions.filter.Length > 0 && !cname.Contains(terrSearchOptions.filter, StringComparison.OrdinalIgnoreCase)) return;
            if (terrSearchOptions.onlyWorld && !v.isWorld) return;
            if (terrSearchOptions.onlySelected && !p.cfg.mobPulled_Territories.Contains(territoryType)) return;
            if (v.isWorld) ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
            var chk = p.cfg.mobPulled_Territories.Contains(territoryType);
            if(ImGui.Checkbox(cname+"##"+tCounter, ref chk))
            {
                if (chk)
                {
                    p.cfg.mobPulled_Territories.Add(territoryType);
                }
                else
                {
                    p.cfg.mobPulled_Territories.Remove(territoryType);
                }
                if(Svc.ClientState.IsLoggedIn) p.mobPulled.TerritoryChanged(null, Svc.ClientState.TerritoryType);
            }
            if (v.isWorld) ImGui.PopStyleColor();
        }
    }
}
