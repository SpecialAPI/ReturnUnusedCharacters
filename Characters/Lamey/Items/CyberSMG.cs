using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.Characters.Lamey.Items
{
    public class CyberSMG
    {
        public static void Init()
        {
            var name = "Cyber SMG";
            var shortdesc = "Poking where you should not";
            var longdesc = "A newer Cybergun design, heavily marketed as a weapon to be used by lone detectives when surrounded by nameless goons. After just a few bursts shot mindlessly around oneself, it is nigh-guaranteed that none (other than the weapon's manufacturers) will remain standing.";

            var gun = EasyGunInit("cybersmg", name, shortdesc, longdesc, 500, 1f, HegemonyRifleObject.muzzleFlashEffects, "CCR_CyberSMG", PickupObject.ItemQuality.B, GunClass.FULLAUTO, out var finish);

            var hrifleproj = RogueSpecialObject.DefaultModule.projectiles[0].GetComponentInChildren<tk2dBaseSprite>();
            var proj = EasyProjectileInit<Projectile>("cybersmgprojectile", 3.5f, 23f, 1000f, 5f, true, false, false, hrifleproj.spriteId, hrifleproj.Collection);

            var homing = proj.AddComponent<HomingModifier>();
            homing.HomingRadius = 10f;
            homing.AngularVelocity = 420f;

            proj.hitEffects = RogueSpecialObject.DefaultModule.projectiles[0].hitEffects;
            gun.RawSourceVolley.projectiles.Add(new()
            {
                shootStyle = ProjectileModule.ShootStyle.Automatic,
                projectiles = new()
                {
                    proj
                },
                cooldownTime = 0.1f,
                numberOfShotsInClip = 18,
                ammoType = GameUIAmmoType.AmmoType.CUSTOM,
                angleVariance = 35f,
                customAmmoType = AddCustomAmmoType("ccr_cybersmg", "cybersmgammotype", "cybersmg_clip")
            });

            SoundManager.AddCustomSwitchData("WPN_Guns", "CCR_CyberSMG", "Play_WPN_Gun_Shot_01", new SwitchedEvent("Play_WPN_Gun_Shot_01", "WPN_Guns", "heavylaser"));
            SoundManager.AddCustomSwitchData("WPN_Guns", "CCR_CyberSMG", "Play_WPN_Gun_Reload_01", new SwitchedEvent("Play_WPN_Gun_Reload_01", "WPN_Guns", "Claw"));

            finish();
        }
    }
}
