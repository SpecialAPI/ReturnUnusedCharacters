using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static UnityEngine.UI.GridLayoutGroup;

namespace ReturnUnusedCharacters.Components
{
    [HarmonyPatch]
    public class PlayerControllerExt : BraveBehaviour
    {
        public void Awake()
        {
            player = GetComponent<PlayerController>();
        }

        public void Rage(float duration)
        {
            if (isRaged)
            {
                if ((bool)RageOverheadVFX && !instanceRageVFX)
                {
                    instanceRageVFX = player.PlayEffectOnActor(RageOverheadVFX, new Vector3(0f, 1.375f, 0f), attached: true, alreadyMiddleCenter: true);
                }

                rageElapsed = Mathf.Max(rageElapsed, duration);
            }
            else
            {
                player.StartCoroutine(HandleRage(duration));
            }
        }

        public IEnumerator HandleRage(float duration)
        {
            var rageColor = new Color(0.5f, 0f, 0f, 0.75f);

            isRaged = true;
            instanceRageVFX = null;

            if ((bool)RageOverheadVFX)
            {
                instanceRageVFX = player.PlayEffectOnActor(RageOverheadVFX, new Vector3(0f, 1.375f, 0f), attached: true, alreadyMiddleCenter: true);
            }

            var damageStat = StatModifier.Create(PlayerStats.StatType.Damage, ModifyMethod.MULTIPLICATIVE, 2f);
            player.ownerlessStatModifiers.Add(damageStat);
            player.stats.RecalculateStats(player);

            if (player.CurrentGun != null)
            {
                player.CurrentGun.ForceImmediateReload();
            }

            rageElapsed = duration;

            var particleCounter = 0f;

            while (rageElapsed > 0f)
            {
                rageElapsed -= BraveTime.DeltaTime;

                player.baseFlatColorOverride = rageColor.WithAlpha(Mathf.Lerp(rageColor.a, 0f, Mathf.Clamp01(1 - rageElapsed)));

                if ((bool)instanceRageVFX && rageElapsed < (duration - 1))
                {
                    instanceRageVFX.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("rage_face_vfx_out");
                    instanceRageVFX = null;
                }

                if (GameManager.Options.ShaderQuality != 0 && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW && (bool)player && player.IsVisible && !player.IsFalling)
                {
                    particleCounter += BraveTime.DeltaTime * 40f;
                    if (particleCounter > 1f)
                    {
                        var num = Mathf.FloorToInt(particleCounter);
                        particleCounter %= 1f;
                        GlobalSparksDoer.DoRandomParticleBurst(num, player.sprite.WorldBottomLeft.ToVector3ZisY(), player.sprite.WorldTopRight.ToVector3ZisY(), Vector3.up, 90f, 0.5f, null, null, null, GlobalSparksDoer.SparksType.BLACK_PHANTOM_SMOKE);
                    }
                }

                yield return null;
            }

            if ((bool)instanceRageVFX)
            {
                instanceRageVFX.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("rage_face_vfx_out");
            }

            player.baseFlatColorOverride = Color.clear;

            player.ownerlessStatModifiers.Remove(damageStat);
            player.stats.RecalculateStats(player);

            isRaged = false;
        }

        [NonSerialized]
        public bool isRaged;
        [NonSerialized]
        public float rageElapsed;
        [NonSerialized]
        public GameObject instanceRageVFX;

        public GameObject RageOverheadVFX
        {
            get
            {
                if (rageOverheadVFXPrefab == null)
                    rageOverheadVFXPrefab = EnragingPhotoObject.OverheadVFX;

                return rageOverheadVFXPrefab;
            }
        }

        public GameObject rageOverheadVFXPrefab;

        public PlayerController player;

        public Action<MinorBreakable, PlayerController> OnMinorBreakableBreak;
        public Action<MajorBreakable, Vector2, PlayerController> OnMajorBreakableBreak;
        public Action<ExplosionData, Vector3, PlayerController> OnExplosion;

        #region Harmony Patches
        [HarmonyPatch(typeof(MinorBreakable), nameof(MinorBreakable.Break), new Type[0])]
        [HarmonyPatch(typeof(MinorBreakable), nameof(MinorBreakable.Break), typeof(Vector2))]
        [HarmonyPostfix]
        public static void MinorBreak(MinorBreakable __instance)
        {
            if (GameManager.HasInstance)
            {
                var players = GameManager.Instance.AllPlayers;
                if (players != null)
                {
                    foreach (var p in players)
                    {
                        if (p == null)
                        {
                            continue;
                        }
                        p.Ext().OnMinorBreakableBreak?.Invoke(__instance, p);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MajorBreakable), nameof(MajorBreakable.Break))]
        [HarmonyPostfix]
        public static void MajorBreak(MajorBreakable __instance, Vector2 sourceDirection)
        {
            if (GameManager.HasInstance)
            {
                var players = GameManager.Instance.AllPlayers;
                if (players != null)
                {
                    foreach (var p in players)
                    {
                        if (p == null)
                        {
                            continue;
                        }
                        p.Ext().OnMajorBreakableBreak?.Invoke(__instance, sourceDirection, p);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Exploder), nameof(Exploder.HandleExplosion), MethodType.Enumerator)]
        [HarmonyILManipulator]
        public static void Explosion(ILContext ctx, MethodBase mthd)
        {
            var crs = new ILCursor(ctx);
            var explosionBegin = mthd.EnumeratorField("onExplosionBegin");

            if (!crs.JumpToNext(x => x.MatchLdfld(explosionBegin)))
                return;

            crs.Emit(OpCodes.Ldarg_0);
            crs.EmitStaticDelegate(InvokeOnExplosion);
        }

        public static Action InvokeOnExplosion(Action _, object cr)
        {
            if (GameManager.HasInstance && GameManager.Instance.AllPlayers != null)
            {
                foreach (var pl in GameManager.Instance.AllPlayers)
                {
                    if (pl == null)
                        continue;

                    pl.Ext().OnExplosion?.Invoke(cr.EnumeratorGetField<ExplosionData>("data"), cr.EnumeratorGetField<Vector3>("position"), pl);
                }
            }

            return _;
        }
        #endregion
    }
}
