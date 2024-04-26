using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.Characters.Lamey.Items
{
    public class CyberPistol
    {
        public static void Init()
        {
            var name = "Cyber Pistol";
            var shortdesc = "Investigative Spirit";
            var longdesc = "One of the earliest models of Cybergun-branded weapons to be manufactured, the Cyber Pistol is much rougher than its newer counterparts, yet it remains popular amongst the detectives of the flashier planets in the Hegemony of Man's domain.\n\nIt is from this gun that the homing shots, a staple of Cyberguns, originated; simply because the designers thought it'd be cooler than a gun with reasonable spread. They were right.";

            var gun = EasyGunInit("cyberpistol", name, shortdesc, longdesc, 250, 1f, HegemonyRifleObject.muzzleFlashEffects, "heavylaser", PickupObject.ItemQuality.B, GunClass.PISTOL, out var finish);

            var hrifleproj = HegemonyRifleObject.DefaultModule.projectiles[0].GetComponentInChildren<tk2dBaseSprite>();
            var proj = EasyProjectileInit<Projectile>("cyberpistolprojectile", 12f, 23f, 1000f, 30f, true, false, true, hrifleproj.spriteId, hrifleproj.Collection);

            var homing = proj.AddComponent<HomingModifier>();
            homing.HomingRadius = 10f;
            homing.AngularVelocity = 420f;

            proj.hitEffects = HegemonyRifleObject.DefaultModule.projectiles[0].hitEffects;

            gun.RawSourceVolley.projectiles.Add(new()
            {
                shootStyle = ProjectileModule.ShootStyle.SemiAutomatic,
                projectiles = new()
                {
                    proj
                },
                cooldownTime = 0.15f,
                numberOfShotsInClip = 4,
                ammoType = GameUIAmmoType.AmmoType.CUSTOM,
                angleVariance = 35f,
                customAmmoType = AddCustomAmmoType("ccr_cyberpistol", "cyberpistolammotype", "cyberpistol_clip")
            });

            gun.carryPixelOffset = new(3, 0);

            finish();
        }
    }
}
