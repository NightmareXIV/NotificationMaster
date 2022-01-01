using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Internal;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Logging;
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

namespace NotificationMaster
{
    class GpNotify : IDisposable
    {
        internal int nextTick = 0;
        internal bool needNotification = false;
        private NotificationMaster p;
        public const byte PotionCDGroup = 69;

        public void Dispose()
        {
            Svc.Framework.Update -= Tick;
            Svc.Commands.RemoveHandler("/gp");
        }

        public GpNotify(NotificationMaster plugin)
        {
            this.p = plugin;
            Svc.Framework.Update += Tick;
            Svc.Commands.AddHandler("/gp", new CommandInfo(OnCommand) 
            { 
                HelpMessage = "open config\n/gp <number> → set trigger GP amount"    
            });
        }

        private void OnCommand(string command, string arguments)
        {
            if(arguments == "")
            {
                p.configGui.open = true;
            }
            else
            {
                try
                {
                    var newgp = int.Parse(arguments.Trim());
                    if(newgp < 0)
                    {
                        Svc.PluginInterface.UiBuilder.AddNotification("GP can't be negative", "NotificationMaster", NotificationType.Error);
                    }
                    else
                    {
                        p.cfg.gp_GPTreshold = newgp;
                        p.cfg.Save();
                        Svc.PluginInterface.UiBuilder.AddNotification("Trigger GP amount set to " + p.cfg.gp_GPTreshold,
                            "NotificationMaster", NotificationType.Success);
                    }
                }
                catch(Exception e)
                {
                    Svc.PluginInterface.UiBuilder.AddNotification("Error: " + e.Message, "NotificationMaster", NotificationType.Error);
                }
            }
        }

        void Tick(Framework framework)
        {
            if (Environment.TickCount < nextTick) return;
            nextTick = Environment.TickCount + 5000;
            if (Svc.ClientState?.LocalPlayer == null) return;
            if (Svc.ClientState.LocalPlayer.ClassJob.Id != 16
                && Svc.ClientState.LocalPlayer.ClassJob.Id != 17
                && Svc.ClientState.LocalPlayer.ClassJob.Id != 18)
            {
                needNotification = false;
                return;
            }
            var gp = Svc.ClientState.LocalPlayer.CurrentGp;
            //pi.Framework.Gui.Chat.Print(actMgr.GetCooldown(ActionManager.PotionCDGroup).IsCooldown + "/" + actMgr.GetCooldown(ActionManager.PotionCDGroup).CooldownElapsed + "/" + actMgr.GetCooldown(ActionManager.PotionCDGroup).CooldownTotal);
            if (!p.actMgr.GetCooldown(PotionCDGroup).IsCooldown) gp += (uint)p.cfg.gp_PotionCapacity;
            //pi.Framework.Gui.Chat.Print(DateTimeOffset.Now + ": " + gp);
            if(gp >= p.cfg.gp_GPTreshold)
            {
                if (needNotification) 
                {
                    needNotification = false;
                    if (!p.ThreadUpdActivated.IsApplicationActivated)
                    {
                        if (p.cfg.gp_FlashTrayIcon)
                        {
                            Native.Impl.FlashWindow();
                        }
                        if (p.cfg.gp_AutoActivateWindow) Native.Impl.Activate();
                        if (p.cfg.gp_ShowToastNotification)
                        {
                            TrayIconManager.ShowToast(gp + " GP ready!");
                        }

                        if (p.cfg.gp_HttpRequestsEnable)
                        {
                            p.httpMaster.DoRequests(p.cfg.gp_HttpRequests,
                                new string[][]
                                {
                                        new string[] {"$G", gp.ToString()},
                                }
                            );
                        }
                    }
                }
            }
            else
            {
                if (gp + p.cfg.gp_Tolerance < p.cfg.gp_GPTreshold)
                {
                    needNotification = true;
                }
            }
        }



        internal static void Setup(bool enable, NotificationMaster p)
        {
            if (enable)
            {
                if (p.gpNotify == null)
                {
                    p.gpNotify = new GpNotify(p);
                    PluginLog.Information("Enabling GP notify");
                }
                else
                {
                    PluginLog.Information("GP notify module already enabled");
                }
            }
            else
            {
                if (p.gpNotify != null)
                {
                    p.gpNotify.Dispose();
                    p.gpNotify = null;
                    PluginLog.Information("Disabling GP notify");
                }
                else
                {
                    PluginLog.Information("GP notify module already disabled");
                }
            }
        }
    }
}
