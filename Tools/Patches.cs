using HarmonyLib;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ReturnUnusedCharacters.Tools
{
    [HarmonyPatch]
    public static class Patches
    {
        [HarmonyPatch(typeof(GameUIAmmoController), nameof(GameUIAmmoController.Initialize))]
        [HarmonyPostfix]
        public static void AddMissingAmmotypes(GameUIAmmoController __instance)
        {
            __instance.ammoTypes = __instance.ammoTypes.AddRangeToArray(addedAmmoTypes.ToArray());
        }

        [HarmonyPatch(typeof(Foyer), nameof(Foyer.SetUpCharacterCallbacks))]
        [HarmonyPrefix]
        public static void PlaceBreachCharacters()
        {
            foreach (var kvp in breachCharacters)
            {
                var room = GameManager.Instance.Dungeon.data.GetRoomFromPosition(kvp.Value.IntXY(VectorConversions.Floor));
                if (room != null)
                {
                    var obj = Object.Instantiate(kvp.Key, room.hierarchyParent);
                    obj.transform.position = kvp.Value;
                    var interactables = obj.GetComponentsInChildren<IPlayerInteractable>();
                    foreach (var interactable in interactables)
                    {
                        room.RegisterInteractable(interactable);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PunchoutController), nameof(PunchoutController.Init))]
        [HarmonyILManipulator]
        public static void SwapToCustomPunchoutCharacter_Transpiler(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            foreach(var m in cursor.MatchAfter(x => x.MatchCallOrCallvirt(randomRange_IntInt)))
            {
                cursor.Emit(OpCodes.Call, stcpc_cv);
            }
        }

        public static int SwapToCustomPunchoutCharacter_ChangeValue(int curr)
        {
            if (punchoutSprites.ContainsKey(GameManager.Instance.PrimaryPlayer.characterIdentity))
                return punchoutSprites[GameManager.Instance.PrimaryPlayer.characterIdentity];

            return curr;
        }

        public static MethodInfo randomRange_IntInt = AccessTools.Method(typeof(Random), nameof(Random.Range), new Type[] { typeof(int), typeof(int) });
        public static MethodInfo stcpc_cv = AccessTools.Method(typeof(Patches), nameof(SwapToCustomPunchoutCharacter_ChangeValue));
    }
}
