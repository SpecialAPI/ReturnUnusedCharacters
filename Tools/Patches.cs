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
        public static void FixDodgeRollsShittyCodeAkaNoParadox(ILContext ctx)
        {
            var cursor = new ILCursor(ctx);

            foreach(var m in cursor.MatchBefore(x => x.Calls(r)))
            {
                cursor.Emit(OpCodes.Call, c);
            }
        }

        [HarmonyPatch(typeof(PunchoutController), nameof(PunchoutController.Init))]
        [HarmonyPostfix]
        public static void SwapPlayer(PunchoutController __instance)
        {
            if (punchoutSprites.ContainsKey(GameManager.Instance.PrimaryPlayer.characterIdentity))
            {
                __instance.Player.SwapPlayer(punchoutSprites[GameManager.Instance.PrimaryPlayer.characterIdentity]);
            }
        }

        public static int ChangeValue(int _)
        {
            return 7;
        }

        public static MethodInfo r = AccessTools.Method(typeof(Random), nameof(Random.Range), new Type[] { typeof(int), typeof(int) });
        public static MethodInfo c = AccessTools.Method(typeof(Patches), nameof(ChangeValue));
    }
}
