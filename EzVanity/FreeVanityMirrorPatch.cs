using HarmonyLib;
using System.Diagnostics;
using System.Reflection;
using UnityEngine.UI;

namespace EzVanity
{
    class FreeVanityMirrorPatch
    {
        [HarmonyPatch(typeof(VanityMirrorManager), "<Update>g__Retrieve_IllusionStoneCount|57_0")]
        public class PatchRetrieveIllusionStoneCount
        {
            static bool Prefix(ref int __result)
            {
                if (!Plugin.Instance.IsFreeAppearanceEnabled())
                {
                    return true;
                }
                __result = 1;
                return false;
            }
        }
        [HarmonyPatch(typeof(PlayerInventory), "Remove_Item")]
        public class PatchRemoveIllusionStone
        {
            static bool Prefix(PlayerInventory __instance, ItemData _itemData, int _quantity)
            {
                if (!Plugin.Instance.IsFreeAppearanceEnabled())
                {
                    return true;
                }

                if (!VanityMirrorManager._current._isOpen)
                {
                    return true;
                }

                if (_itemData._itemName == "Illusion Stone" && _quantity > 0)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
