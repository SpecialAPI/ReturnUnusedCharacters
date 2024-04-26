using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.Characters.Lamey.Items
{
    public class AltLameyGun
    {
        public static void Init()
        {
            var name = "Lamey Gun";
            var shortdesc = "Detective's Weapon";
            var longdesc = "Lamey's trusty gun, its perfect accuracy and high range allows to easily hit enemies even from long distances.";

            var gun = EasyGunInit("altlamey_gun", name, shortdesc, longdesc, 500, 1.3f, WitchPistolObject.muzzleFlashEffects, "heavylaser", PickupObject.ItemQuality.SPECIAL, GunClass.NONE, out var finish, overrideConsoleId: "ccr:lamey_gun_alt");
            gun.InfiniteAmmo = true;

            var wpistolProj = WitchPistolObject.DefaultModule.projectiles[0].GetComponentInChildren<tk2dBaseSprite>();
            var proj = EasyProjectileInit<Projectile>("altlameygunprojectile", 5.5f, 23f, 1000f, 9f, true, false, false, wpistolProj.spriteId, wpistolProj.Collection);

            var light = proj.GetComponentInChildren<Light>();

            var pulser = light.AddComponent<LightPulser>();
            pulser.flicker = false;
            pulser.flickerRange = 2f;
            pulser.normalRange = 2.8f;
            pulser.pulseSpeed = 20f;
            pulser.waitTime = 0.05f;
            pulser.enabled = false;

            var heightController = light.AddComponent<ObjectHeightController>();
            heightController.heightOffGround = -0.8f;

            var lightController = light.AddComponent<BundleOfWandsLightController>();
            lightController.baseColor = new(0.9961f, 0f, 0.9961f);

            proj.hitEffects = WitchPistolObject.DefaultModule.projectiles[0].hitEffects;

            gun.RawSourceVolley.projectiles.Add(new()
            {
                shootStyle = ProjectileModule.ShootStyle.SemiAutomatic,
                projectiles = new()
                {
                    proj
                },
                cooldownTime = 0.2f,
                numberOfShotsInClip = 15,
                ammoType = GameUIAmmoType.AmmoType.CUSTOM,
                angleVariance = 0f,
                customAmmoType = AddCustomAmmoType("ccr_altlameygun", "altlameygunammotype", "altlameygun_clip")
            });

            finish();

            gun.encounterTrackable.journalData.SuppressInAmmonomicon =  EncounterDatabase.GetEntry(gun.encounterTrackable.EncounterGuid).journalData.SuppressInAmmonomicon =    true;
            gun.encounterTrackable.journalData.AmmonomiconSprite =      EncounterDatabase.GetEntry(gun.encounterTrackable.EncounterGuid).journalData.AmmonomiconSprite =        "lamey_gun_idle_01";

            // apply changes to the original lamey gun
            LameyGunObject.gunSwitchGroup = gun.gunSwitchGroup;

            LameyGunObject.quality = PickupObject.ItemQuality.SPECIAL;
            LameyGunObject.gunClass = gun.gunClass;

            LameyGunObject.encounterTrackable.journalData.PrimaryDisplayName =              EncounterDatabase.GetEntry(LameyGunObject.encounterTrackable.EncounterGuid).journalData.PrimaryDisplayName =                gun.encounterTrackable.journalData.PrimaryDisplayName;
            LameyGunObject.encounterTrackable.journalData.NotificationPanelDescription =    EncounterDatabase.GetEntry(LameyGunObject.encounterTrackable.EncounterGuid).journalData.NotificationPanelDescription =      gun.encounterTrackable.journalData.NotificationPanelDescription;
            LameyGunObject.encounterTrackable.journalData.AmmonomiconFullEntry =            EncounterDatabase.GetEntry(LameyGunObject.encounterTrackable.EncounterGuid).journalData.AmmonomiconFullEntry =              gun.encounterTrackable.journalData.AmmonomiconFullEntry;
            LameyGunObject.encounterTrackable.journalData.AmmonomiconSprite =               EncounterDatabase.GetEntry(LameyGunObject.encounterTrackable.EncounterGuid).journalData.AmmonomiconSprite =                 "lamey_gun_idle_01";
            LameyGunObject.encounterTrackable.journalData.SuppressInAmmonomicon =           EncounterDatabase.GetEntry(LameyGunObject.encounterTrackable.EncounterGuid).journalData.SuppressInAmmonomicon =             false;

            AddSpriteToCollection(LameyGunObject.sprite.CurrentSprite, AmmonomiconController.ForceInstance.EncounterIconCollection);
        }
    }
}
