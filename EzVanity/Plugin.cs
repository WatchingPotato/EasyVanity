using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EzVanity
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("EasySettings", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Homebrewery", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "ez.vanity";
        private const string modName = "Easy Vanity";
        private const string modVersion = "1.2.5";

        public static ConfigEntry<bool> ConfigEnableFree;
        public static ConfigEntry<bool> ConfigEnableFreeTransmog;
        public static ConfigEntry<bool> ConfigEnableUnlockedTransmog;
        public static ConfigEntry<bool> ConfigEnableDontConsumeDye;

        public static bool HBloaded = false;
        public static Plugin Instance { get; private set; }

        private void Awake()
        {
            // Plugin startup logic
            var harmony = new Harmony("ez.vanity");
            Instance = this;
            Logger.LogInfo($"Plugin {modGUID} is loaded!");
            //new Harmony(modName).PatchAll();

            // Theres probably a better way to do this, but fuck it we ball
            harmony.PatchAll(typeof(FreeTransmogPatch.PatchRemoveIllusionStone));
            harmony.PatchAll(typeof(FreeTransmogPatch.PatchRetrieveIllusionStoneCount));
            harmony.PatchAll(typeof(FreeVanityMirrorPatch.PatchRemoveIllusionStone));
            harmony.PatchAll(typeof(FreeVanityMirrorPatch.PatchRetrieveIllusionStoneCount));
            harmony.PatchAll(typeof(UnlockedTransmogPatch.PatchAbleToTransmogEquip));
            harmony.PatchAll(typeof(UnlockedTransmogPatch.PatchCanTransmogItem));

            ConfigEnableFree = Config.Bind("Free Appearance Change", "EnableFreeAppearance", true, "Whether or not it is free to make changes at the mirror");
            ConfigEnableFreeTransmog = Config.Bind("Free Transmog", "EnableFreeTransmog", true, "Whether or not it is free to transmog armor");
            ConfigEnableUnlockedTransmog = Config.Bind("Unlocked Transmog", "EnableUnlockedTransmog", true, "Whether or not transmog is unrestricted");
            ConfigEnableDontConsumeDye = Config.Bind("Dont Consume Dye", "DontConsumeDye", true, "Whether or not consume dye when used (Not HB Compatiable)");

            // ES check
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("EasySettings"))
            {
                Logger.LogInfo("EasySettings found - Settings tab added");
                ESsetup.Initialize();
            }

            // HB check
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("Homebrewery"))
            {
                Logger.LogInfo("Homebrewery found - Skipping dye patch");
                HBloaded = true;
            }
            else
            {
                harmony.PatchAll(typeof(DyePatch.PatchRemoveDye));
            }
            
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
        public bool DontConsumeDyeEnabled()
        {
            return ConfigEnableDontConsumeDye.Value;
        }
    }
}

