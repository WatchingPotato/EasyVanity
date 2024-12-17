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

        //private static ConfigEntry<KeyCode> ConfigMirrorKey;
        //private static ConfigEntry<bool> ConfigEnableMirrorKey;

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
            //ConfigMirrorKey = Config.Bind(MirrorKeyDefinition, KeyCode.M, MirrorKeyDescription);

            //var EnableMirrorKeyDefinition = new ConfigDefinition("Enable Mirror Key", "EnableMirrorKey");
            //var EnableMirrorKeyDescription = new ConfigDescription("Enable the Mirror Menu key");
            //ConfigEnableMirrorKey = Config.Bind(EnableMirrorKeyDefinition, true, EnableMirrorKeyDescription);
        }

        //Config UI
        private void AddSettings()
        {
            SettingsTab tab = Settings.ModTab;

            tab.AddHeader("Easy Vanity");
            tab.AddToggle("Free Appearance Changes", ConfigEnableFree);
            tab.AddToggle("Free Transmog", ConfigEnableFreeTransmog);
            tab.AddToggle("Unlocked Transmog", ConfigEnableUnlockedTransmog);

            //tab.AddToggle("Enable Mirror Menu Button", ConfigEnableMirrorKey);
            //tab.AddKeyButton("Mirror Menu Button", ConfigMirrorKey);
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
        //private bool IsMirrorKeyEnabled()
        //{
        //    return ConfigEnableMirrorKey.Value;
        //}


        //

        //Free Appearance Stuff

        //


        // Sets appearnce cost to 0 and makes confirm button always interactable
        [HarmonyPatch(typeof(VanityMirrorManager), "Update")]
        public class VanityMirrorManagerCostPatch
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


        //

        //Free Transmog Stuff

        //


        //Sets illusion stone count to always appear as 1 for tranmog
        [HarmonyPatch(typeof(PlayerEquipment), "<Init_TransmogItem>g__Retrieve_IllusionStoneCount|22_0")]
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

        //Prevents illusion stones from being removed from inventory during transmog
        [HarmonyPatch(typeof(PlayerInventory), "Remove_Item")]
        public class PatchRemoveIllusionStone
        {
            static bool Prefix(PlayerInventory __instance, ItemData _itemData, int _quantity)
            {
                if (!Plugin.Instance.IsFreeTransmogEnabled())
                {
                    return true; // Exit if free transmog is not enabled
                }

                if (TabMenu._current._itemCell._currentEquipCellTab != EquipCellTab.VANITY)
                {
                    return true; // Exit if the current tab is not vanity
                }

                if (_itemData._itemName == "Illusion Stone" && _quantity > 0) // Prevent the removal of Illusion Stones
                {
                    return false;
                }
                return true;
            }
        }


        //

        //Unlocked Transmog Stuff

        //


        //<Handle_ItemData>g__CanTransmogItem|40_7 is weird so things had to be done differently
        private bool isNotWeaponOrRing;
        private ScriptableEquipment currentScriptableEquipment;

        public void CheckEquipmentType(ScriptableEquipment scriptableEquipment)
        {
            currentScriptableEquipment = scriptableEquipment;
            isNotWeaponOrRing = !(scriptableEquipment.GetType() == typeof(ScriptableWeapon)) && !(scriptableEquipment.GetType() == typeof(ScriptableRing));
        }

        public bool IsNotWeaponOrRing()
        {
            //Debug.Log($"IsNotWeaponOrRing: {isNotWeaponOrRing}");
            return isNotWeaponOrRing;
        }

        [HarmonyPatch(typeof(ItemListDataEntry), "CanTransmogItem")]
        public class PatchCanTransmogItem
        {
            static bool Prefix(ItemListDataEntry __instance, ref bool __result)
            {
                if (!Plugin.Instance.IsUnlockedTransmogEnabled())
                {
                    return true; // Exit if unlocked transmog is not enabled
                }

                if (__instance._scriptableItem._itemType == ItemType.GEAR)
                {
                    ScriptableEquipment scriptableEquipment = (ScriptableEquipment)__instance._scriptableItem;
                    Plugin.Instance.CheckEquipmentType(scriptableEquipment);

                    if (Plugin.Instance.IsNotWeaponOrRing())
                    {
                        __result = true;
                        return false;
                    }
                }

                __result = false;
                return false;
            }
        }


        //Makes it so you can always transmog equipment
        [HarmonyPatch(typeof(ItemMenuCell), "AbleToTransmogEquip")]
        public class PatchAbleToTransmogEquip
        {
            static void Postfix(ItemMenuCell __instance, ItemListDataEntry _listEntry, ref bool __result)
            {
                if (!Plugin.Instance.IsUnlockedTransmogEnabled())
                {
                    return; // Exit if unlocked transmog is not enabled
                }

                if (_listEntry._scriptableItem._itemType == ItemType.GEAR)
                {
                    ScriptableEquipment scriptableEquipment = (ScriptableEquipment)_listEntry._scriptableItem;
                    Plugin.Instance.CheckEquipmentType(scriptableEquipment);

                    if (Plugin.Instance.IsNotWeaponOrRing())
                    {
                        __result = true;
                        return;
                    }
                }
            }
        }
    }
}

