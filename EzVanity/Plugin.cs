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
        private const string modVersion = "1.0";

        internal static new ManualLogSource Logger;

        private static ConfigEntry<bool> ConfigEnableFree;
        private static ConfigEntry<bool> ConfigEnableFreeTransmog;
        private static ConfigEntry<bool> ConfigEnableUnlockedTransmog;

        public static Plugin Instance { get; private set; }
        private void Awake()
        {
            // Plugin startup logic
            Instance = this;
            Logger = base.Logger;
            new Harmony(modName).PatchAll();
            Logger.LogInfo($"Plugin {modGUID} is loaded!");

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

            //var MirrorKeyDefinition = new ConfigDefinition("Mirror Key", "ConfigMirrorKey");
            //var MirrorKeyDescription = new ConfigDescription("Set the Mirror Menu key");
            //ConfigMirrorKey = Config.Bind(MirrorKeyDefinition, true, MirrorKeyDescription);
        }

        //Config UI
        private void AddSettings()
        {
            SettingsTab tab = Settings.ModTab;

            tab.AddHeader("Easy Vanity");
            tab.AddToggle("Free Appearance Changes", ConfigEnableFree);
            tab.AddToggle("Free Transmog", ConfigEnableFreeTransmog);
            tab.AddToggle("Unlocked Transmog", ConfigEnableUnlockedTransmog);


            // tab.AddButton("Mirror Menu Button", ConfigMirrorKey);
        }

        // Config bools
        private bool IsFreeAppearanceEnabled()
        {
            return ConfigEnableFree.Value;
        }
        private bool IsFreeTransmogEnabled()
        {
            return ConfigEnableFreeTransmog.Value;
        }
        private bool IsUnlockedTransmogEnabled()
        {
            return ConfigEnableUnlockedTransmog.Value;
        }

        // Sets appearnce cost to 0 and makes confirm button always interactable
        [HarmonyPatch(typeof(VanityMirrorManager), "Update")]
        public class PatchVanityMirrorManagerUpdate
        {

            static void Postfix(VanityMirrorManager __instance)
            {
                if (!Plugin.Instance.IsFreeAppearanceEnabled())
                {
                    return; // Exit if free appearance is not enabled
                }

                FieldInfo costField = typeof(VanityMirrorManager).GetField("_currentAppearanceCost", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo counterField = typeof(VanityMirrorManager).GetField("_appearanceCostCounter", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo buttonField = typeof(VanityMirrorManager).GetField("_applyAppearanceButton", BindingFlags.NonPublic | BindingFlags.Instance);

                if (costField != null && counterField != null && buttonField != null)
                {
                    costField.SetValue(__instance, 0); // Set appearance cost to 0

                    // Update the appearance cost counter - unsure if still needed
                    var counter = counterField.GetValue(__instance) as Text;
                    if (counter != null)
                    {
                        counter.text = string.Format("Cost: x{0}", 0);
                    }

                    // Makes confirm button always interactable
                    var applyButton = buttonField.GetValue(__instance) as UnityEngine.UI.Button;
                    if (applyButton != null)
                    {
                        applyButton.interactable = true;
                    }
                }
            }
        }

        //Sets illusion stone count to always appear as 1 for tranmog
        [HarmonyPatch(typeof(PlayerEquipment), "<Init_TransmogItem>g__Retrieve_IllusionStoneCount|24_0")]
        public class PatchRetrieveIllusionStoneCount
        {
            static bool Prefix(ref int __result)
            {
                if (!Plugin.Instance.IsFreeTransmogEnabled())
                {
                    return true; // Exit if free transmog is not enabled
                }
                __result = 1;
                return false;
            }
        }

        //Prevents illusion stones from being removed from inventory during transmog. I'm sure there are no unintented side effects /s
        [HarmonyPatch(typeof(PlayerInventory), "Remove_Item")]
        public class PatchRemoveItem
        {
            static bool Prefix(PlayerEquipment __instance, ItemData _itemData, int _quantity)
            {
                if (!Plugin.Instance.IsFreeTransmogEnabled())
                {
                    return true; // Exit if free transmog is not enabled
                }
                if (_itemData._itemName == "Illusion Stone" && _quantity > 0) // Prevent the removal of the item
                {
                    return false;
                }
                return true;
            }
        }

        //Makes it so you can always transmog equipment
        [HarmonyPatch(typeof(ItemMenuCell), "AbleToTransmogEquip")]
        public class PatchAbleToTransmogEquip
        {
            static void Postfix(ref bool __result)
            {
                if (!Plugin.Instance.IsUnlockedTransmogEnabled())
                {
                    return; // Exit if unlocked transmog is not enabled
                }
                __result = true;
            }
        }
    }
}

