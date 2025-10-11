using HarmonyLib;


namespace EzVanity
{
    public class UnlockedTransmogPatch
    {
        private static UnlockedTransmogPatch _instance;
        public static UnlockedTransmogPatch Instance => _instance ??= new UnlockedTransmogPatch();

        // Checks if the item can normally transmog
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
                    return true;
                }

                if (__instance._scriptableItem._itemType == ItemType.GEAR)
                {
                    ScriptableEquipment scriptableEquipment = (ScriptableEquipment)__instance._scriptableItem;
                    UnlockedTransmogPatch.Instance.CheckEquipmentType(scriptableEquipment);

                    if (UnlockedTransmogPatch.Instance.IsNotWeaponOrRing())
                    {
                        __result = true;
                        return false;
                    }
                }

                __result = false;
                return false;
            }
        }

        // Makes it so you can always transmog equipment
        [HarmonyPatch(typeof(ItemMenuCell), "AbleToTransmogEquip")]
        public class PatchAbleToTransmogEquip
        {
            static void Postfix(ItemMenuCell __instance, ItemListDataEntry _listEntry, ref bool __result)
            {
                if (!Plugin.Instance.IsUnlockedTransmogEnabled())
                {
                    return;
                }

                if (_listEntry._scriptableItem._itemType == ItemType.GEAR)
                {
                    ScriptableEquipment scriptableEquipment = (ScriptableEquipment)_listEntry._scriptableItem;
                    UnlockedTransmogPatch.Instance.CheckEquipmentType(scriptableEquipment);

                    if (UnlockedTransmogPatch.Instance.IsNotWeaponOrRing())
                    {
                        __result = true;
                        return;
                    }
                }
            }
        }
    }
}

