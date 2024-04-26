using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.Characters.Lamey.Items
{
    public class VegasSMG : GunBehaviour
    {
        public static void Init()
        {
            var name = "Vegas SMG";
            var shortdesc = "No Wedding Bells for Today";
            var longdesc = "The Vegas SMG is a unique 12.7 mm submachine gun that never entered mass use due to its overly small quick, in spite of its large and expensive magazines. Although it was a commercial failure, its actual effectiveness is impressive for a manufacturer with more experience producing printers as opposed to actual guns.";

            var gun = EasyGunInit("vegas_smg", name, shortdesc, longdesc, 500, 0.5f, Mac10Object.muzzleFlashEffects, "CCR_VegasSMG", PickupObject.ItemQuality.B, GunClass.FULLAUTO, out var finish);

            var mac10proj = Mac10Object.DefaultModule.projectiles[0].GetComponentInChildren<tk2dBaseSprite>();
            var proj = EasyProjectileInit<Projectile>("vegassmgprojectile", 8f, 23f, 1000f, 5f, false, false, false, mac10proj.spriteId, mac10proj.Collection);

            proj.hitEffects = Mac10Object.DefaultModule.projectiles[0].hitEffects;

            gun.RawSourceVolley.projectiles.Add(new()
            {
                shootStyle = ProjectileModule.ShootStyle.Automatic,
                projectiles = new()
                {
                    proj
                },
                cooldownTime = 0.1f,
                numberOfShotsInClip = 8,
                angleVariance = 35f
            });

            SoundManager.AddCustomSwitchData("WPN_Guns", "CCR_VegasSMG", "Play_WPN_Gun_Shot_01", new SwitchedEvent("Play_WPN_Gun_Shot_01", "WPN_Guns", "Mac10"));
            SoundManager.AddCustomSwitchData("WPN_Guns", "CCR_VegasSMG", "Play_WPN_Gun_Reload_01", new SwitchedEvent("Play_WPN_Gun_Reload_01", "WPN_Guns", "Uzi"));

            var controller = gun.AddComponent<VegasSMG>();

            controller.rageChance = 0.3f;
            controller.rageDuration = 4f;

            controller.pewpewBoySynergyCooldown = 10f;
            controller.pewpewBoySynergyDuration = 5f;

            var ashesProj = GunslingersAshesObject.DefaultModule.projectiles[0];
            var ashesSprite = ashesProj.GetComponentInChildren<tk2dBaseSprite>();
            var strangerProj = EasyProjectileInit<Projectile>("vegassmgstrangerprojectile", 12f, 0f, 1000f, 0f, false, false, true, ashesSprite.spriteId, ashesSprite.Collection);

            strangerProj.hitEffects = ashesProj.hitEffects;

            var anim = strangerProj.GetComponentInChildren<tk2dBaseSprite>().spriteAnimator;
            anim.Library = ashesSprite.spriteAnimator.Library;
            anim.DefaultClipId = ashesSprite.spriteAnimator.DefaultClipId;
            anim.playAutomatically = ashesSprite.spriteAnimator.playAutomatically;

            var projSpawnHandler = strangerProj.AddComponent<SpawnProjModifier>();
            projSpawnHandler.alignToSurfaceNormal = false;
            projSpawnHandler.collisionSpawnProjectiles = new Projectile[0];
            projSpawnHandler.collisionSpawnStyle = SpawnProjModifier.CollisionSpawnStyle.FLAK_BURST;
            projSpawnHandler.doOverrideObjectCollisionSpawnStyle = false;
            projSpawnHandler.elapsed = 0f;
            projSpawnHandler.fireRandomlyInAngle = false;
            projSpawnHandler.inFlightAimAtEnemies = true;
            projSpawnHandler.InFlightSourceTransform = null;
            projSpawnHandler.inFlightSpawnAngle = 360f;
            projSpawnHandler.inFlightSpawnAnimation = "gunslinger_projectile_shoot";
            projSpawnHandler.inFlightSpawnCooldown = 2f;
            projSpawnHandler.numberToSpawnOnCollison = 0;
            projSpawnHandler.numToSpawnInFlight = 1;
            projSpawnHandler.overrideObjectSpawnStyle = SpawnProjModifier.CollisionSpawnStyle.RADIAL;
            projSpawnHandler.PostprocessSpawnedProjectiles = false;
            projSpawnHandler.projectileToSpawnOnCollision = null;
            projSpawnHandler.randomRadialStartAngle = false;
            projSpawnHandler.spawnAudioEvent = "";
            projSpawnHandler.spawnCollisionProjectilesOnBounce = false;
            projSpawnHandler.SpawnedProjectileScaleModifier = 1f;
            projSpawnHandler.SpawnedProjectilesInheritAppearance = false;
            projSpawnHandler.SpawnedProjectilesInheritData = false;
            projSpawnHandler.spawnOnObjectCollisions = false;
            projSpawnHandler.spawnProjecitlesOnDieInAir = false;
            projSpawnHandler.spawnProjectilesInFlight = true;
            projSpawnHandler.spawnProjectilesOnCollision = false;
            projSpawnHandler.startAngle = 180;
            projSpawnHandler.usesComplexSpawnInFlight = true;
            projSpawnHandler.UsesMultipleCollisionSpawnProjectiles = false;

            var projKiller = strangerProj.AddComponent<DieInAirWithDelayProjectileHandler>();
            projKiller.delay = 2.5f;

            var ashesSmallProj = ashesProj.GetComponent<SpawnProjModifier>().projectileToSpawnInFlight;
            var ashesSmallSprite = ashesSmallProj.GetComponentInChildren<tk2dBaseSprite>();
            var strangerSmallProj = EasyProjectileInit<Projectile>("vegassmgstrangershotprojectile", 50f, 23f, 1000f, 9f, false, false, true, ashesSmallSprite.spriteId, ashesSmallSprite.Collection);

            strangerSmallProj.hitEffects = ashesSmallProj.hitEffects;

            var smallAnim = ashesSmallProj.transform.GetComponentInChildren<tk2dBaseSprite>().spriteAnimator;
            smallAnim.Library = ashesSmallSprite.spriteAnimator.Library;
            smallAnim.DefaultClipId = ashesSmallSprite.spriteAnimator.DefaultClipId;
            smallAnim.playAutomatically = ashesSmallSprite.spriteAnimator.playAutomatically;

            var pierce = strangerSmallProj.GetOrAddComponent<PierceProjModifier>();
            pierce.penetration = 1;

            projSpawnHandler.projectileToSpawnInFlight = strangerSmallProj;

            controller.mysteriousStrangerSynergyProjectile = strangerProj;
            controller.mysteriousStrangerChance = 0.05f;

            finish();
        }

        public override void PostProcessProjectile(Projectile projectile)
        {
            base.PostProcessProjectile(projectile);

            projectile.OnHitEnemy += ChanceToRageOnKill;
        }

        public void ChanceToRageOnKill(Projectile proj, SpeculativeRigidbody body, bool killed)
        {
            if (killed && PlayerOwner && PlayerOwner.HasActiveBonusSynergy(CustomSynergyTypeE.PSYCHOBUFF) && Random.value < rageChance)
            {
                PlayerOwner.Ext().Rage(rageDuration);
            }
        }

        public override void OnPlayerPickup(PlayerController playerOwner)
        {
            base.OnPlayerPickup(playerOwner);

            playerOwner.OnUsedPlayerItem += PewpewBoySynergy;
        }

        public void PewpewBoySynergy(PlayerController player, PlayerItem item)
        {
            if(player.HasActiveBonusSynergy(CustomSynergyTypeE.PEWPEW_BOY) && Time.time - lastTimeUsedPewpewBoySynergy >= pewpewBoySynergyCooldown)
            {
                player.StartCoroutine(HandlePewpewBoySynergyCR(player));
            }
        }

        public IEnumerator HandlePewpewBoySynergyCR(PlayerController player)
        {
            if (!player)
                yield break;

            var dmgMult = StatModifier.Create(PlayerStats.StatType.Damage, ModifyMethod.MULTIPLICATIVE, 1.5f);
            var firerateMult = StatModifier.Create(PlayerStats.StatType.RateOfFire, ModifyMethod.MULTIPLICATIVE, 1.5f);

            player.ownerlessStatModifiers.Add(dmgMult);
            player.ownerlessStatModifiers.Add(firerateMult);

            player.stats.RecalculateStats(player, false, false);

            AkSoundEngine.PostEvent("Play_ITM_Macho_Brace_Active_01", player.gameObject);

            SpriteOutlineManager.AddOutlineToSprite(player.sprite, new(0f, 1000f, 1000f));

            for(float i = 0f; this && player && PlayerOwner && (i < pewpewBoySynergyDuration); i += BraveTime.DeltaTime)
            {
                if(this)
                    lastTimeUsedPewpewBoySynergy = Time.time;

                yield return null;
            }

            if (this)
                lastTimeUsedPewpewBoySynergy = Time.time;

            if (player)
            {
                SpriteOutlineManager.AddOutlineToSprite(player.sprite, player.outlineColor);

                player.ownerlessStatModifiers.Remove(dmgMult);
                player.ownerlessStatModifiers.Remove(firerateMult);

                player.stats.RecalculateStats(player, false, false);

                AkSoundEngine.PostEvent("Play_ITM_Macho_Brace_Fade_01", player.gameObject);
            }

            if (!this)
                yield break;

            while (this && player && PlayerOwner && (Time.time - lastTimeUsedPewpewBoySynergy <= pewpewBoySynergyCooldown))
            {
                yield return null;
            }

            if(this && player && PlayerOwner)
            {
                AkSoundEngine.PostEvent("Play_ITM_Macho_Brace_Active_01", player.gameObject);
            }
        }

        public override void OnDroppedByPlayer(PlayerController player)
        {
            base.OnDroppedByPlayer(player);

            player.OnUsedPlayerItem -= PewpewBoySynergy;
        }

        public override void OnPostFired(PlayerController player, Gun gun)
        {
            base.OnPostFired(player, gun);

            if(player && player.CurrentRoom != null && player.HasActiveBonusSynergy(CustomSynergyTypeE.MYSTERIOUS_STRANGER) && Random.value < mysteriousStrangerChance)
            {
                var randomPos = player.CurrentRoom.GetRandomAvailableCell(IntVector2.One, null, false, null);

                if (randomPos.HasValue)
                {
                    OwnedShootProjectile(mysteriousStrangerSynergyProjectile, randomPos.Value.ToCenterVector2(), Random.Range(0f, 360f), player);
                }
            }
        }

        public float rageChance;
        public float rageDuration;

        public float lastTimeUsedPewpewBoySynergy = -999f;
        public float pewpewBoySynergyCooldown;
        public float pewpewBoySynergyDuration;

        public Projectile mysteriousStrangerSynergyProjectile;
        public float mysteriousStrangerChance;
    }
}
