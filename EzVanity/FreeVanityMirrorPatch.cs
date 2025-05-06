using HarmonyLib;
using System.Reflection;
using UnityEngine.UI;

namespace EzVanity
{
    class FreeVanityMirrorPatch
    {
        [HarmonyPatch(typeof(VanityMirrorManager), "Update")]
        public class VanityMirrorManagerCostPatch
        {
            static void Postfix(VanityMirrorManager __instance)
            {
                if (!Plugin.Instance.IsFreeAppearanceEnabled())
                {
                    return;
                }

                FieldInfo costField = typeof(VanityMirrorManager).GetField("_currentAppearanceCost", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo counterField = typeof(VanityMirrorManager).GetField("_appearanceCostCounter", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo buttonField = typeof(VanityMirrorManager).GetField("_applyAppearanceButton", BindingFlags.NonPublic | BindingFlags.Instance);

                if (costField != null && counterField != null && buttonField != null)
                {
                    costField.SetValue(__instance, 0);

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
    }
}
