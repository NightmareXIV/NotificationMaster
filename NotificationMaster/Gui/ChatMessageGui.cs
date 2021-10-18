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
                        var chatTypeInt = (int)elem.ChatType;
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        ImGui.DragInt("##f1" + i, ref chatTypeInt, float.Epsilon, 0, ushort.MaxValue, ((XivChatType)elem.ChatType).ToString());
                        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                        {
                            ImGui.OpenPopup("##c1" + i);
                        }
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("Right click - select;\nDouble left click - enter manually.");
                        }
                        if (chatTypeInt >= ushort.MinValue && chatTypeInt <= ushort.MaxValue) elem.ChatType = (ushort)chatTypeInt;
                        if(ImGui.IsPopupOpen("##c1" + i))
                        {
                            ImGui.SetNextWindowSize(new Vector2(200, 400));
                        }
                        if (ImGui.BeginPopup("##c1"+i))
                        {
                            foreach(var e in Enum.GetValues(typeof(XivChatType)))
                            {
                                if(ImGui.Selectable((ushort)e + "/" + e))
                                {
                                    elem.ChatType = (ushort)e;
                                }
                            }
                            ImGui.EndPopup();
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
                                        ChatType = e.Type,
                                        MessageStr = e.Message,
                                        SenderStr = e.Sender
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
