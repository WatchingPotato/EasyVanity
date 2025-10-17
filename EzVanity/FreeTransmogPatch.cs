using HarmonyLib;

namespace EzVanity
{
    class FreeTransmogPatch
    {
        //Sets illusion stone count to always appear as 1 for tranmog
        [HarmonyPatch(typeof(PlayerEquipment), "<Init_TransmogItem>g__Retrieve_IllusionStoneCount|23_0")]
        public class PatchRetrieveIllusionStoneCount
        {
            static bool Prefix(ref int __result)
            {
                if (!Plugin.Instance.IsFreeTransmogEnabled())
                {
                    return true;
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
                    return true;
                }

                if (TabMenu._current._itemCell._currentEquipCellTab != EquipCellTab.VANITY_PANEL)
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
