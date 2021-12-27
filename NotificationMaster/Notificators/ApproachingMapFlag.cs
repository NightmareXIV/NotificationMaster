using Dalamud.Game;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NotificationMaster
{
    internal unsafe class ApproachingMapFlag
    {
        NotificationMaster p;
        internal float* flagX;
        internal float* flagY;
        internal int* flagTerritory;
        internal byte* isFlagSet;

        public void Dispose()
        {
            Svc.Framework.Update -= ApproachingMapFlagWatcher;
        }

        public ApproachingMapFlag(NotificationMaster plugin)
        {
            this.p = plugin;
            try
            {
                var agentMap = (IntPtr)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()
                    ->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.Map);
                PluginLog.Information($"AgentMap: {agentMap:X16}");
                isFlagSet = (byte*)(agentMap + 22959);
                flagTerritory = (int*)(agentMap + 14320);
                flagX = (float*)(agentMap + 14328);
                flagY = (float*)(agentMap + 14332);
                Svc.Framework.Update += ApproachingMapFlagWatcher;
            }
            catch(Exception e)
            {
                PluginLog.Error($"{e.Message}\n{e.StackTrace ?? ""}");
                Setup(false, plugin);
            }
        }

        bool IsEnabled = false;
        bool HasTriggered = false;
        bool DirectionX;
        bool DirectionY;
        private void ApproachingMapFlagWatcher(Framework framework)
        {
            if (Native.ApplicationIsActivated() ||
                Svc.ClientState.LocalPlayer == null ||
                *isFlagSet == 0 || *flagTerritory != Svc.ClientState.TerritoryType)
            {
                IsEnabled = false;
                HasTriggered = false;
            }
            else
            {
                if (!IsEnabled)
                {
                    UpdateDirections();
                }
                if(Vector2.Distance(new Vector2(*flagX, *flagY), 
                    new Vector2(Svc.ClientState.LocalPlayer.Position.X, 
                    Svc.ClientState.LocalPlayer.Position.Z)) <= p.cfg.mapFlag_TriggerDistance)
                {
                    if (IsEnabled && !HasTriggered)
                    {
                        DoNotify("You have reached your destination!");
                        Svc.Chat.Print($"{ImGui.GetFrameCount()} Approached distance!");
                    }
                    HasTriggered = true;
                }
                else
                {
                    HasTriggered = false;
                }
                if ((!DirectionX && *flagX > Svc.ClientState.LocalPlayer.Position.X) || (DirectionX && *flagX < Svc.ClientState.LocalPlayer.Position.X))
                {
                    if (IsEnabled && !HasTriggered)
                    {
                        DoNotify("You have crossed your destination border (X)!");
                        Svc.Chat.Print($"{ImGui.GetFrameCount()} Crossed X line!");
                    }
                    UpdateDirections();
                }
                if ((!DirectionY && *flagY > Svc.ClientState.LocalPlayer.Position.Z) || (DirectionY && *flagY < Svc.ClientState.LocalPlayer.Position.Z))
                {
                    if (IsEnabled && !HasTriggered)
                    {
                        DoNotify("You have crossed your destination border (Y)!");
                        Svc.Chat.Print($"{ImGui.GetFrameCount()} Crossed Y line!");
                    }
                    UpdateDirections();
                }
                IsEnabled = true;
            }
        }

        void DoNotify(string s)
        {
            if (p.cfg.mapFlag_FlashTrayIcon)
            {
                Native.Impl.FlashWindow();
            }
            if (p.cfg.mapFlag_AutoActivateWindow) Native.Impl.Activate();
            if (p.cfg.mapFlag_ShowToastNotification)
            {
                TrayIconManager.ShowToast(s, "");
            }
            if (p.cfg.mapFlag_HttpRequestsEnable)
            {
                p.httpMaster.DoRequests(p.cfg.mapFlag_HttpRequests,
                    new string[][]
                    {
                    }
                );
            }
        }

        void UpdateDirections()
        {
            DirectionX = *flagX > Svc.ClientState.LocalPlayer.Position.X;
            DirectionY = *flagY > Svc.ClientState.LocalPlayer.Position.Z;
            Svc.Chat.Print($"Directions: {DirectionX}, {DirectionY}");
        }

        internal static void Setup(bool enable, NotificationMaster p)
        {
            if (enable)
            {
                if (p.mapFlag == null)
                {
                    p.mapFlag = new ApproachingMapFlag(p);
                    PluginLog.Information("Enabling mapFlag module");
                }
                else
                {
                    PluginLog.Information("mapFlag module already enabled");
                }
            }
            else
            {
                if (p.mapFlag != null)
                {
                    p.mapFlag.Dispose();
                    p.mapFlag = null;
                    PluginLog.Information("Disabling mapFlag module");
                }
                else
                {
                    PluginLog.Information("mapFlag module already disabled");
                }
            }
        }
    }
}
