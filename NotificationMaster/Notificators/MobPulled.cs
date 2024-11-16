using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text.SeStringHandling;
using ECommons.Logging;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NotificationMaster;

internal class MobPulled : IDisposable
{
    private NotificationMaster p;
    private HashSet<uint> ignoreMobIds = [];
    private HashSet<string> watchedMobNamesHashes = [];
    internal Dictionary<uint, (string name, bool isWorld)> territories;
    public void Dispose()
    {
        Svc.ClientState.TerritoryChanged -= TerritoryChanged;
        Svc.Framework.Update -= MobPulledWatcher;
    }

    public MobPulled(NotificationMaster plugin)
    {
        p = plugin;
        territories = [];
        foreach(var terr in Svc.Data.GetExcelSheet<TerritoryType>())
        {
            territories.Add(terr.RowId, (terr.PlaceName.Value.Name.ToString(), terr.Mount));
        }
        PluginLog.Debug($"Territories added: {territories.Count}, world zones={territories.Where(p => p.Value.isWorld).Count()}");
        if(p.cfg.mobPulled_Territories.Count == 0)
        {
            PluginLog.Information("Config mob pulled territories count was 0, populating it with world zones");
            foreach(var e in territories)
            {
                if(e.Value.isWorld) p.cfg.mobPulled_Territories.Add(e.Key);
            }
        }
        RebuildMobNames();
        TerritoryChanged(Svc.ClientState.TerritoryType);
        Svc.ClientState.TerritoryChanged += TerritoryChanged;
    }

    internal void RebuildMobNames()
    {
        watchedMobNamesHashes.Clear();
        foreach(var s in p.cfg.mobPulled_Names)
        {
            watchedMobNamesHashes.Add(s);
        }
        PluginLog.Debug($"Mob names hash table rebuilt, config entries={string.Join(",", p.cfg.mobPulled_Names)}; hashes={string.Join(",", watchedMobNamesHashes)}");
    }

    internal void ClearIgnoredMobs()
    {
        ignoreMobIds.Clear();
        PluginLog.Debug("Cleared ignored mobs ids cache");
    }

    internal void TerritoryChanged(ushort newTerritory)
    {
        Svc.Framework.Update -= MobPulledWatcher;
        PluginLog.Debug("MobPulledWatcher unregistered.");
        ClearIgnoredMobs();
        if(p.cfg.mobPulled_Territories.Contains(newTerritory))
        {
            Svc.Framework.Update += MobPulledWatcher;
            PluginLog.Debug($"MobPulledWatcher registered, territory type={newTerritory}");
        }
        else
        {
            PluginLog.Debug($"MobPulledWatcher was not registered for this territory, territory type={newTerritory}");
        }
    }

    private void MobPulledWatcher(object framework)
    {
        if(p.PauseUntil > Environment.TickCount64) return;
        if(Svc.ClientState.LocalPlayer != null)
        {
            foreach(var o in Svc.Objects)
            {
                if(o is IBattleNpc bnpc && !ignoreMobIds.Contains(o.EntityId))
                {
                    var bnpcName = bnpc.Name.ToString();
                    if(bnpcName.Length == 0) continue;
                    if(!watchedMobNamesHashes.Contains(bnpcName))
                    {
                        ignoreMobIds.Add(o.EntityId);
                    }
                    else
                    {
                        if(bnpc.MaxHp != bnpc.CurrentHp)
                        {
                            PluginLog.Debug($"Detected pulled mob: {bnpc.Name} with id={o.EntityId}");
                            ignoreMobIds.Add(o.EntityId);
                            if(p.cfg.mobPulled_AlwaysExecute || !Utils.IsApplicationActivated)
                            {
                                PluginLog.Debug($"Notifying; app activated = {Utils.IsApplicationActivated}");
                                if(p.cfg.mobPulled_FlashTrayIcon && !Utils.IsApplicationActivated)
                                {
                                    Native.Impl.FlashWindow();
                                }
                                if(p.cfg.mobPulled_AutoActivateWindow && !Utils.IsApplicationActivated) Native.Impl.Activate();
                                if(p.cfg.mobPulled_ShowToastNotification)
                                {
                                    TrayIconManager.ShowToast($"{bnpc.Name} has been pulled!", "");
                                }
                                if(p.cfg.mobPulled_HttpRequestsEnable)
                                {
                                    p.httpMaster.DoRequests(p.cfg.mobPulled_HttpRequests,
                                        new string[][]
                                        {
                                            new string[] {"$M", bnpc.Name.ToString()},
                                        }
                                    );
                                }
                                if(p.cfg.mobPulled_SoundSettings.PlaySound)
                                {
                                    p.audioPlayer.Play(p.cfg.mobPulled_SoundSettings);
                                }
                                if(p.cfg.mobPulled_ChatMessage)
                                {
                                    Svc.Chat.Print(
                                        new SeStringBuilder()
                                        .AddUiForeground(16)
                                        .AddText($"{bnpc.Name} has been pulled!")
                                        .AddUiForegroundOff()
                                        .Build());
                                }
                                if(p.cfg.mobPulled_Toast)
                                {
                                    Svc.Toasts.ShowQuest(
                                        new SeStringBuilder()
                                        .AddUiForeground(16)
                                        .AddText($"{bnpc.Name} has been pulled!")
                                        .AddUiForegroundOff()
                                        .Build()
                                        , new QuestToastOptions()
                                        {
                                            DisplayCheckmark = true,
                                            PlaySound = true
                                        });
                                }
                            }
                            continue;
                        }
                    }
                }
            }
        }
    }

    internal static void Setup(bool enable, NotificationMaster p)
    {
        if(enable)
        {
            if(p.mobPulled == null)
            {
                p.mobPulled = new MobPulled(p);
                PluginLog.Information("Enabling mobPulled module");
            }
            else
            {
                PluginLog.Information("mobPulled module already enabled");
            }
        }
        else
        {
            if(p.mobPulled != null)
            {
                p.mobPulled.Dispose();
                p.mobPulled = null;
                PluginLog.Information("Disabling mobPulled module");
            }
            else
            {
                PluginLog.Information("mobPulled module already disabled");
            }
        }
    }
}
