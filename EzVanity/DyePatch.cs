using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace EzVanity
{
    class DyePatch
    {
        [HarmonyPatch(typeof(PlayerInventory), "Init_RemoveConsumableItemData")]
        public class PatchRemoveDye
        {
            static bool Prefix(PlayerInventory __instance, ItemData _itemData)
            {
                if (Plugin.Instance.DontConsumeDyeEnabled() && TabMenu._current._itemCell._currentEquipCellTab == EquipCellTab.EQUIPMENT_PANEL &&
                    _itemData._itemName.EndsWith(" Dye", StringComparison.Ordinal))
                {
                    return false;
                }
                return true;
            }

            static void Postfix(PlayerInventory __instance, ItemData _itemData)
            {
                if (Plugin.Instance.DontConsumeDyeEnabled() && TabMenu._current._itemCell._currentEquipCellTab == EquipCellTab.EQUIPMENT_PANEL &&
                    _itemData._itemName.EndsWith(" Dye", StringComparison.Ordinal))
                {
                    ProfileDataManager._current.Save_ProfileData();
                }
            }
        }
    }
}
