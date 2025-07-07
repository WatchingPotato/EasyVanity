using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using Nessie.ATLYSS.EasySettings;

namespace EzVanity
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "ez.vanity";
        private const string modName = "Easy Vanity";
        private const string modVersion = "1.1.9";

        private static ConfigEntry<bool> ConfigEnableFree;
        private static ConfigEntry<bool> ConfigEnableFreeTransmog;
        private static ConfigEntry<bool> ConfigEnableUnlockedTransmog;

        public static Plugin Instance { get; private set; }
        private void Awake()
        {
            // Plugin startup logic
            Instance = this;

            new Harmony(modName).PatchAll();
            Logger.LogInfo($"Plugin {modGUID} is loaded!");

            //// Temp manual configs
            //ConfigEnableFree = Config.Bind("Free Appearance Change", "EnableFreeAppearance", true, "Whether or not it is free to make changes at the mirror");
            //ConfigEnableFreeTransmog = Config.Bind("Free Transmog", "EnableFreeTransmog", true, "Whether or not it is free to transmog armor");
            //ConfigEnableUnlockedTransmog = Config.Bind("Unlocked Transmog", "EnableUnlockedTransmog", true, "Whether or not Transmog is unrestricted");

            //EasySettings Init
            InitConfig();
            Settings.OnInitialized.AddListener(AddSettings);
            Settings.OnApplySettings.AddListener(() => { Config.Save(); });
        }

        //EasySettings Config Init
        private void InitConfig()
        {
            var FreeAppearanceDefinition = new ConfigDefinition("Free Appearance Change", "EnableFreeAppearance");
            var FreeAppearanceDescription = new ConfigDescription("Enable or disable changing appearance for free.");
            ConfigEnableFree = Config.Bind(FreeAppearanceDefinition, true, FreeAppearanceDescription);

            var FreeTransmogDefinition = new ConfigDefinition("Free Transmog", "EnableFreeTransmog");
            var FreeTransmogDescription = new ConfigDescription("Enable or disable transmogging for free.");
            ConfigEnableFreeTransmog = Config.Bind(FreeTransmogDefinition, true, FreeTransmogDescription);

            var UnlockedTransmogDefinition = new ConfigDefinition("Unlocked Transmog", "EnableUnlockedTransmog");
            var UnlockedTransmogDescription = new ConfigDescription("Enable or disable transmogging for free.");
            ConfigEnableUnlockedTransmog = Config.Bind(UnlockedTransmogDefinition, true, UnlockedTransmogDescription);
        }

        //EasySettings Config UI
        private void AddSettings()
        {
            SettingsTab tab = Settings.ModTab;

            tab.AddHeader("Easy Vanity");
            tab.AddToggle("Free Appearance Changes", ConfigEnableFree);
            tab.AddToggle("Free Transmog", ConfigEnableFreeTransmog);
            tab.AddToggle("Unlocked Transmog", ConfigEnableUnlockedTransmog);
        }

        // Config bools
        public bool IsFreeAppearanceEnabled()
        {
            return ConfigEnableFree.Value;
        }
        public bool IsFreeTransmogEnabled()
        {
            return ConfigEnableFreeTransmog.Value;
        }
        public bool IsUnlockedTransmogEnabled()
        {
            return ConfigEnableUnlockedTransmog.Value;
        }

    }
}

