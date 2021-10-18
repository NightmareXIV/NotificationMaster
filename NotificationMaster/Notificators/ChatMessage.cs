using Dalamud.Game.Network;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NotificationMaster
{
    class ChatMessage : IDisposable
    {
        private NotificationMaster p;
        internal (ushort Type, string Sender, string Message)[] ChatLog = new (ushort, string, string)[100];
        internal bool pause = false;

        public void Dispose()
        {
            Svc.Chat.ChatMessage -= Chat_ChatMessage;
        }

        public ChatMessage(NotificationMaster plugin)
        {
            this.p = plugin;
            Svc.Chat.ChatMessage += Chat_ChatMessage;
        }

        private void Chat_ChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (p.configGui.open)
            {
                if(!pause) ChatLog.AddShifting(((ushort)type, sender.ToString(), message.ToString()));
            }
            //else
            {
                if (!Native.ApplicationIsActivated())
                {
                    var senderFullStr = sender.ToString();
                    var messageFullStr = message.ToString();
                    foreach (var e in p.cfg.chatMessage_Elements)
                    {
                        if (
                            (e.ChatType == 0 || e.ChatType == (ushort)type)
                            && (e.MessageStr == ""
                                || (e.MessageStr == "" && messageFullStr == "")
                                || (e.CompareType == 0 && messageFullStr.Contains(e.MessageStr))
                                || (e.CompareType == 1 && messageFullStr.Contains(e.MessageStr, StringComparison.InvariantCultureIgnoreCase))
                                || (e.CompareType == 2 && Regex.IsMatch(messageFullStr, e.MessageStr))
                            )
                            && (e.SenderStr == ""
                                || (e.SenderStr == "" && senderFullStr == "")
                                || (e.CompareType == 0 && senderFullStr.Contains(e.SenderStr))
                                || (e.CompareType == 1 && senderFullStr.Contains(e.SenderStr, StringComparison.InvariantCultureIgnoreCase))
                                || (e.CompareType == 2 && Regex.IsMatch(senderFullStr, e.SenderStr))
                            )
                        )
                        {
                            if (p.cfg.chatMessage_FlashTrayIcon)
                            {
                                Native.Impl.FlashWindow();
                            }
                            if (p.cfg.chatMessage_AutoActivateWindow) Native.Impl.Activate();
                            if (p.cfg.chatMessage_ShowToastNotification)
                            {
                                Native.Impl.ShowToast(messageFullStr, "Message" + (senderFullStr == "" ? "" : $" from {senderFullStr}"));
                            }
                        }
                    }
                }
            }
        }

        internal static void Setup(bool enable, NotificationMaster p)
        {
            if (enable)
            {
                if (p.chatMessage == null)
                {
                    p.chatMessage = new ChatMessage(p);
                    PluginLog.Verbose("Enabling chat message module");
                }
                else
                {
                    PluginLog.Verbose("chat message module already enabled");
                }
            }
            else
            {
                if (p.chatMessage != null)
                {
                    p.chatMessage.Dispose();
                    p.chatMessage = null;
                    PluginLog.Verbose("Disabling chat message module");
                }
                else
                {
                    PluginLog.Verbose("chat message module already disabled");
                }
            }
        }
    }

    [Serializable]
    public class ChatMessageElement
    {
        [NonSerialized] internal readonly static string[] CompareTypes = { "Exact", "Case insensitive", "Regexp" };

        public ushort ChatType = 0;
        /// <summary>
        /// 0: Exact; 1: Case insensitive; 2: Regexp
        /// </summary>
        public int CompareType = 0;
        public string SenderStr = "";
        public string MessageStr = "";
    }
}
