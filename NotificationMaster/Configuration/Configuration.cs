using Dalamud.Plugin;
using ECommons.Configuration;
using System;
using System.Collections.Generic;

namespace NotificationMaster;

[Serializable]
internal class Configuration : IEzConfig
{
    [NonSerialized]
    private IDalamudPluginInterface pluginInterface;
    public int Version { get; set; } = 1;

    public bool gp_Enable = false;
    public bool gp_ShowToastNotification = true;
    public bool gp_FlashTrayIcon = true;
    public bool gp_AutoActivateWindow = false;
    public int gp_PotionCapacity = 400;
    public int gp_GPTreshold = 800;
    public int gp_Tolerance = 50;
    public bool gp_SuppressIfNoNodes = false;
    public bool gp_HttpRequestsEnable = false;
    public List<HttpRequestElement> gp_HttpRequests = [];
    public SoundSettings gp_SoundSettings = new();

    public bool cutscene_Enable = false;
    public bool cutscene_ShowToastNotification = true;
    public bool cutscene_FlashTrayIcon = true;
    public bool cutscene_AutoActivateWindow = false;
    public bool cutscene_OnlyMSQ = false;
    public bool cutscene_HttpRequestsEnable = false;
    public List<HttpRequestElement> cutscene_HttpRequests = [];
    public SoundSettings cutscene_SoundSettings = new();

    public bool chatMessage_Enable = false;
    public bool chatMessage_ShowToastNotification = true;
    public bool chatMessage_FlashTrayIcon = true;
    public bool chatMessage_AutoActivateWindow = false;
    public List<ChatMessageElement> chatMessage_Elements = [];
    public bool chatMessage_HttpRequestsEnable = false;
    public List<HttpRequestElement> chatMessage_HttpRequests = [];
    public SoundSettings chatMessage_SoundSettings = new();

    public bool cfPop_Enable = false;
    public bool cfPop_ShowToastNotification = true;
    public bool cfPop_FlashTrayIcon = true;
    public bool cfPop_AutoActivateWindow = false;
    public bool cfPop_NotifyIn30 = false;
    public bool cfPop_NotifyOnlyIn30 = false;
    public bool cfPop_HttpRequestsEnable = false;
    public List<HttpRequestElement> cfPop_HttpRequests = [];
    public SoundSettings cfPop_SoundSettings = new();

    public bool loginError_Enable = false;
    public bool loginError_AlwaysExecute = true;
    public bool loginError_FlashTrayIcon = true;
    public bool loginError_AutoActivateWindow = false;
    public bool loginError_ShowToastNotification = true;
    public bool loginError_HttpRequestsEnable;
    public List<HttpRequestElement> loginError_HttpRequests = [];
    public SoundSettings loginError_SoundSettings = new();

    public bool mapFlag_Enable = false;
    public bool mapFlag_FlashTrayIcon = true;
    public bool mapFlag_AutoActivateWindow = false;
    public bool mapFlag_ShowToastNotification = true;
    public bool mapFlag_HttpRequestsEnable;
    public int mapFlag_TriggerDistance = 200;
    public bool mapFlag_TriggerOnCross = true;
    public int mapFlag_CrossDelta = 100;
    public List<HttpRequestElement> mapFlag_HttpRequests = [];
    public SoundSettings mapFlag_SoundSettings = new();

    public bool mobPulled_Enable = false;
    public bool mobPulled_FlashTrayIcon = true;
    public bool mobPulled_AutoActivateWindow = false;
    public bool mobPulled_ShowToastNotification = true;
    public bool mobPulled_HttpRequestsEnable;
    public List<HttpRequestElement> mobPulled_HttpRequests = [];
    public SoundSettings mobPulled_SoundSettings = new();
    public HashSet<string> mobPulled_Names = [];
    public HashSet<uint> mobPulled_Territories = [];
    public bool mobPulled_AlwaysExecute = true;
    public bool mobPulled_ChatMessage = true;
    public bool mobPulled_Toast = true;

    public bool partyFinder_Enable = false;
    public bool partyFinder_OnlyWhenFilled = false;
    public bool partyFinder_Delisted = false;
    public bool partyFinder_ShowToastNotification = true;
    public bool partyFinder_FlashTrayIcon = true;
    public bool partyFinder_AutoActivateWindow = false;
    public SoundSettings partyFinder_SoundSettings = new();

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
        foreach(var e in chatMessage_Elements)
        {
            if(e.ChatType != 0)
            {
                e.ChatTypes.Add(e.ChatType);
            }
            e.ChatType = 0;
        }
    }

    public void Save()
    {
        EzConfig.Save();
    }
}
