using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Nessie.ATLYSS.EasySettings;

namespace EzVanity
{
    class ESsetup
    {
        internal static ESsetup Instance;
        public static void Initialize() => Instance ??= new();
        private ESsetup()
        {
            Settings.OnInitialized.AddListener(ES_AddSettings);
        }

        //EasySettings Config UI
        public void ES_AddSettings()
        {
            SettingsTab tab = Settings.ModTab;
            tab.AddHeader("Easy Vanity");
            tab.AddToggle("Free Appearance Changes", Plugin.ConfigEnableFree);
            tab.AddToggle("Free Transmog", Plugin.ConfigEnableFreeTransmog);
            tab.AddToggle("Unlocked Transmog", Plugin.ConfigEnableUnlockedTransmog);
            if (!Plugin.HBloaded){
                tab.AddToggle("Dont Consume Dye", Plugin.ConfigEnableDontConsumeDye);
            }
        }
    }
}
