using Dalamud.Game.ClientState.Conditions;
using Dalamud.Logging;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationMaster
{
    class CfPop : IDisposable
    {
        NotificationMaster p;
        TickScheduler extraNotify = null;
        public void Dispose()
        {
            Svc.ClientState.CfPop -= Pop;
            if(extraNotify != null)
            {
                extraNotify.Dispose();
                extraNotify = null;
            }
        }

        public CfPop(NotificationMaster plugin)
        {
            this.p = plugin;
            Svc.ClientState.CfPop += Pop;
        }

        private void Pop(object sender, ContentFinderCondition e)
        {
            PluginLog.Debug("Cf pop " + e.Name.ToString());
            if (!p.ThreadUpdActivated.IsApplicationActivated)
            {
                DoNotify(e.Name.ToString());
            }
            if (p.cfg.cfPop_NotifyIn30)
            {
                if (extraNotify != null)
                {
                    extraNotify.Dispose();
                }
                extraNotify = new TickScheduler(delegate
                {
                    if (!p.ThreadUpdActivated.IsApplicationActivated && Svc.Condition[ConditionFlag.WaitingForDutyFinder]
                     && !Svc.Condition[ConditionFlag.WaitingForDuty])
                    {
                        DoNotify(e.Name.ToString(), true);
                    }
                }, Svc.Framework, 30 * 1000);
            }
        }

        void DoNotify(string str, bool soonEnd = false)
        {
            if (str == "") str = "Duty roulette";
            if (p.cfg.cfPop_FlashTrayIcon)
            {
                Native.Impl.FlashWindow();
            }
            if (p.cfg.cfPop_AutoActivateWindow) Native.Impl.Activate();
            if (p.cfg.cfPop_ShowToastNotification)
            {
                TrayIconManager.ShowToast(str, soonEnd?"Duty invitation expires in 15 seconds!":"Duty pop");
            }
            if (p.cfg.cfPop_HttpRequestsEnable)
            {
                p.httpMaster.DoRequests(p.cfg.cfPop_HttpRequests,
                    new string[][]
                    {
                        new string[] {"$N", str},
                        new string[] {"$T", soonEnd ? "15":"45"}
                    }
                );
            }
            if (p.cfg.cfPop_SoundSettings.PlaySound)
            {
                p.audioPlayer.Play(p.cfg.cfPop_SoundSettings);
            }
        }

        internal static void Setup(bool enable, NotificationMaster p)
        {
            if (enable)
            {
                if (p.cfPop == null)
                {
                    p.cfPop = new CfPop(p);
                    PluginLog.Information("Enabling cfPop module");
                }
                else
                {
                    PluginLog.Information("cfPop module already enabled");
                }
            }
            else
            {
                if (p.cfPop != null)
                {
                    p.cfPop.Dispose();
                    p.cfPop = null;
                    PluginLog.Information("Disabling cfPop module");
                }
                else
                {
                    PluginLog.Information("cfPop module already disabled");
                }
            }
        }
    }
}
