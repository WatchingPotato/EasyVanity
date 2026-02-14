using HarmonyLib;
using System.Diagnostics;
using System.Reflection;
using UnityEngine.UI;

namespace EzVanity
{
    class FreeVanityMirrorPatch
    {
        //Sets illusion stone count to always appear as 1 for vanity mirror
        [HarmonyPatch(typeof(VanityMirrorManager), "<Update>g__Retrieve_IllusionStoneCount|92_0")]
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
        //Prevents illusion stones from being removed from inventory during vanity mirror
        [HarmonyPatch(typeof(PlayerInventory), "Remove_Item")]
        public class PatchRemoveIllusionStone
        {
            static bool Prefix(PlayerInventory __instance, ItemData _itemData, int _quantity)
            {
                if (!Plugin.Instance.IsFreeAppearanceEnabled() || !VanityMirrorManager._current._isOpen)
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
