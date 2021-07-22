using Dalamud.Game.Command;
using Dalamud.Game.Internal;
using Dalamud.Game.Internal.Gui.Toast;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GPNotify
{
    class GPNotify : IDalamudPlugin
    {
        internal DalamudPluginInterface pi;
        internal ActionManager actMgr;
        internal Configuration cfg;
        internal int nextTick = 0;
        internal bool needNotification = false;
        internal ConfigGui configGui;

        public string Name => "GPNotify";

        public void Dispose()
        {
            configGui.Dispose();
            pi.Framework.OnUpdateEvent -= Tick;
            pi.CommandManager.RemoveHandler("/gp");
            pi.Dispose();
        }

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pi = pluginInterface;
            cfg = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            cfg.Initialize(pi);
            actMgr = new ActionManager(this);
            pi.Framework.OnUpdateEvent += Tick;
            pi.CommandManager.AddHandler("/gp", new CommandInfo(OnCommand));
            configGui = new ConfigGui(this);
            pi.UiBuilder.OnOpenConfigUi += delegate { configGui.open = true; };
        }

        private void OnCommand(string command, string arguments)
        {
            if(arguments == "")
            {
                configGui.open = true;
            }
            else
            {
                try
                {
                    var newgp = int.Parse(arguments.Trim());
                    if(newgp < 0)
                    {
                        pi.Framework.Gui.Toast.ShowError("GP can't be negative");
                    }
                    else
                    {
                        cfg.GPTreshold = newgp;
                        cfg.Save();
                        pi.Framework.Gui.Toast.ShowQuest("Trigger GP amount set to " + cfg.GPTreshold,
                            new QuestToastOptions() { DisplayCheckmark = true, PlaySound = true });
                    }
                }
                catch(Exception e)
                {
                    pi.Framework.Gui.Toast.ShowError("Error: " + e.Message);
                }
            }
        }

        void Tick(Framework framework)
        {
            if (Environment.TickCount < nextTick) return;
            nextTick = Environment.TickCount + 5000;
            if (pi.ClientState?.LocalPlayer == null) return;
            if (pi.ClientState.LocalPlayer.ClassJob.Id != 16 && pi.ClientState.LocalPlayer.ClassJob.Id != 17) return;
            var gp = pi.ClientState.LocalPlayer.CurrentGp;
            //pi.Framework.Gui.Chat.Print(actMgr.GetCooldown(ActionManager.PotionCDGroup).IsCooldown + "/" + actMgr.GetCooldown(ActionManager.PotionCDGroup).CooldownElapsed + "/" + actMgr.GetCooldown(ActionManager.PotionCDGroup).CooldownTotal);
            if (!actMgr.GetCooldown(ActionManager.PotionCDGroup).IsCooldown) gp += cfg.PotionCapacity;
            //pi.Framework.Gui.Chat.Print(DateTimeOffset.Now + ": " + gp);
            if(gp >= cfg.GPTreshold)
            {
                if (needNotification) 
                {
                    needNotification = false;
                    if (!Native.ApplicationIsActivated())
                    {
                        if (cfg.FlashTrayIcon)
                        {
                            var flashInfo = new Native.FLASHWINFO
                            {
                                cbSize = (uint)Marshal.SizeOf<Native.FLASHWINFO>(),
                                uCount = uint.MaxValue,
                                dwTimeout = 0,
                                dwFlags = Native.FlashWindow.FLASHW_ALL |
                                            Native.FlashWindow.FLASHW_TIMERNOFG,
                                hwnd = Process.GetCurrentProcess().MainWindowHandle
                            };
                            Native.FlashWindowEx(ref flashInfo);
                        }
                        if (cfg.AutoActivateWindow) Native.SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
                        if (cfg.ShowToastNotification)
                        {
                            var n = new NotifyIcon
                            {
                                Icon = SystemIcons.Application,
                                Visible = true
                            };
                            n.ShowBalloonTip(int.MaxValue, "", gp+" GP ready!", ToolTipIcon.Info);
                            n.BalloonTipClosed += delegate
                            {
                                n.Visible = false;
                                n.Dispose();
                            };
                            n.BalloonTipClicked += delegate
                            {
                                n.Visible = false;
                                n.Dispose();
                            };
                        }
                    }
                }
            }
            else
            {
                needNotification = true;
            }
        }
    }
}
