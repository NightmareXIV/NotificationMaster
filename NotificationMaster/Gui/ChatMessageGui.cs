using Dalamud.Game.Text;
using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NotificationMaster
{
    partial class ConfigGui
    {
        int CustomTypeToAdd = 0;
        internal void DrawChatMessageGui()
        {
            var id = 0;
            int toDelete = -1;
            if (ImGui.Checkbox("Enable", ref p.cfg.chatMessage_Enable))
            {
                ChatMessage.Setup(p.cfg.chatMessage_Enable, p);
            }
            if (p.cfg.chatMessage_Enable)
            {
                //ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(0xff0000ff), "Triggers are paused while configuration is open.");
                ImGui.TextWrapped("When chat message matching any rule received, if FFXIV is running in background:");
                ImGui.Checkbox("Show tray notification", ref p.cfg.chatMessage_ShowToastNotification);
                ImGui.Checkbox("Flash taskbar icon", ref p.cfg.chatMessage_FlashTrayIcon);
                ImGui.Checkbox("Bring FFXIV to foreground", ref p.cfg.chatMessage_AutoActivateWindow);
                DrawHttpMaster(p.cfg.chatMessage_HttpRequests, ref p.cfg.chatMessage_HttpRequestsEnable,
                    "$S - sender\n$M - message\n$T - chat type");
                ImGui.Separator();
                if (ImGui.CollapsingHeader("Triggers"))
                {
                    //ImGui.BeginChild("##trigs");
                    if (ImGui.Button("Add"))
                    {
                        p.cfg.chatMessage_Elements.Add(new ChatMessageElement());
                    }
                    ImGui.Columns(5);
                    ImGui.SetColumnWidth(0, 150f);
                    ImGui.SetColumnWidth(1, 150f);
                    ImGui.SetColumnWidth(2, ImGui.GetWindowContentRegionWidth() - 150 - 150 - 100 - 40);
                    ImGui.SetColumnWidth(3, 100f);
                    ImGui.SetColumnWidth(4, 40f);
                    ImGui.Text("Type");
                    ImGui.NextColumn();
                    ImGui.Text("Sender");
                    ImGui.NextColumn();
                    ImGui.Text("Message");
                    ImGui.NextColumn();
                    ImGui.Text("Search mode");
                    ImGui.NextColumn();
                    ImGui.Text("Del");
                    ImGui.NextColumn();
                    ImGui.Columns(1);
                    for (var i = 0; i < p.cfg.chatMessage_Elements.Count; i++)
                    {
                        var elem = p.cfg.chatMessage_Elements[i];
                        ImGui.Columns(5);
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        if (ImGui.BeginCombo("##fselect"+i, elem.ChatTypes.Count == 0? "Any" : elem.ChatTypes.Count == 1? ((XivChatType)elem.ChatTypes.First()).ToString():$"{elem.ChatTypes.Count} types"))
                        {
                            var customElements = new HashSet<ushort>(elem.ChatTypes);
                            customElements.RemoveWhere(p => Enum.GetValues<XivChatType>().ToHashSet().Contains((XivChatType)p));
                            var elemenets = Enum.GetValues<XivChatType>().Select(e => (ushort)e).ToHashSet().Union(customElements);
                            foreach (var e in elemenets)
                            {
                                var selected = elem.ChatTypes.Contains(e);
                                ImGui.Checkbox(((XivChatType)e).ToString(), ref selected);
                                if (selected)
                                {
                                    elem.ChatTypes.Add(e);
                                }
                                else
                                {
                                    elem.ChatTypes.Remove(e);
                                }
                            }
                            ImGui.SetNextItemWidth(50f);
                            ImGui.InputInt("##typecustom" + i, ref CustomTypeToAdd, 0, 0);
                            ImGui.SameLine();
                            if(ImGui.Button("Add custom type"))
                            {
                                elem.ChatTypes.Add((ushort)CustomTypeToAdd);
                                CustomTypeToAdd = 0;
                            }
                            ImGui.EndCombo();
                        }
                        ImGui.NextColumn();
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        ImGui.InputText("##f2" + i, ref elem.SenderStr, 1000);
                        ImGui.NextColumn();
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        ImGui.InputText("##f3" + i, ref elem.MessageStr, 1000);
                        ImGui.NextColumn();
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        ImGui.Combo("##f4" + i, ref elem.CompareType, ChatMessageElement.CompareTypes, ChatMessageElement.CompareTypes.Length);
                        ImGui.NextColumn();
                        if (ImGui.Button("[X]##del"+i))
                        {
                            toDelete = i;
                        }
                        ImGui.NextColumn();
                        ImGui.Columns(1);
                        ImGui.Text("Exceptions:");
                        ImGui.SameLine();
                        ImGui.Checkbox("No flashing##" + i, ref elem.NoFlash);
                        ImGui.SameLine();
                        ImGui.Checkbox("No bring to foreground##" + i, ref elem.NoForeground);
                        ImGui.SameLine();
                        ImGui.Checkbox("No toast##" + i, ref elem.NoToast);
                        ImGui.SameLine();
                        ImGui.Checkbox("No HTTP##" + i, ref elem.NoHTTP);
                        ImGui.Separator();
                    }
                    //ImGui.EndChild();
                }
                if(toDelete != -1)
                {
                    try
                    {
                        p.cfg.chatMessage_Elements.RemoveAt(toDelete);
                    }
                    catch(Exception e)
                    {
                        PluginLog.Error(e.Message + "\n" + e.StackTrace.NotNull());
                    }
                    toDelete = -1;
                }
                if (ImGui.CollapsingHeader("Message log"))
                {
                    //ImGui.BeginChild("##nm_chatlog");
                    ImGui.Checkbox("Pause log", ref p.chatMessage.pause);
                    if (p.chatMessage != null)
                    {
                        ImGui.Columns(3);
                        ImGui.Text("Type");
                        ImGui.NextColumn();
                        ImGui.Text("Sender");
                        ImGui.NextColumn();
                        ImGui.Text("Message");
                        ImGui.NextColumn();
                        ImGui.Columns(1);
                        foreach (var e in p.chatMessage.ChatLog)
                        {
                            if (e.Type != 0)
                            {
                                var cursor = ImGui.GetCursorPos();
                                ImGui.Columns(3);
                                ImGui.TextWrapped($"{e.Type}/{(XivChatType)e.Type}");
                                ImGui.NextColumn();
                                ImGui.TextWrapped(e.Sender.NotNull());
                                ImGui.NextColumn();
                                ImGui.TextWrapped(e.Message.NotNull());
                                ImGui.NextColumn();
                                ImGui.Columns(1);
                                var cursor2 = ImGui.GetCursorPos();
                                ImGui.SetCursorPos(cursor);
                                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                                if (ImGui.Selectable("##chm" + id++))
                                {
                                    p.cfg.chatMessage_Elements.Add(new ChatMessageElement()
                                    {
                                        ChatTypes = new HashSet<ushort>() { e.Type },
                                        MessageStr = e.Message.Split('\n')[0],
                                        SenderStr = e.Sender.Split('\n')[0]
                                    });
                                }
                                ImGui.SetCursorPos(cursor2);
                                ImGui.Separator();
                            }
                        }
                    }
                    else
                    {
                        ImGui.Text("Error");
                    }
                    //ImGui.EndChild();
                }
            }
        }
    }
}
