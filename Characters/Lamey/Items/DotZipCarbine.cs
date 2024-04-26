using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.Characters.Lamey.Items
{
    public class DotZipCarbine : GunBehaviour
    {
        public static void Init()
        {
            var srifleproj = ShockRifleObject.DefaultModule.projectiles[0];

            void SetupLightningProjectile(Projectile proj)
            {
                var anim = proj.transform.Find("Sprite").GetComponent<tk2dSpriteAnimator>();
                anim.Library = srifleproj.transform.Find("Sprite").GetComponent<tk2dSpriteAnimator>().Library;
                anim.DefaultClipId = srifleproj.transform.Find("Sprite").GetComponent<tk2dSpriteAnimator>().DefaultClipId;

                var light = proj.transform.Find("light");

                var height = light.AddComponent<ObjectHeightController>();
                height.heightOffGround = -0.8f;

                var killer = light.AddComponent<TimedObjectKiller>();
                killer.lifeTime = 0.06f;
                killer.m_light = null;
                killer.m_poolType = TimedObjectKiller.PoolType.Pooled;
                killer.m_renderer = null;

                var trailtransform = proj.transform.Find("Trail 1");

                var trailsprite = trailtransform.GetComponent<tk2dTiledSprite>();
                var srifletrailsprite = srifleproj.GetComponentInChildren<tk2dTiledSprite>();
                trailsprite.SetSprite(srifletrailsprite.Collection, srifletrailsprite.spriteId);

                trailsprite.spriteAnimator.Library = srifletrailsprite.spriteAnimator.Library;
                trailsprite.spriteAnimator.DefaultClipId = srifletrailsprite.spriteAnimator.DefaultClipId;

                var trail = trailtransform.AddComponent<TrailController>();
                trail.animation = "electric_lightning_beam_middle";
                trail.boneSpawnOffset = Vector2.zero;
                trail.cascadeTimer = 0.1f;
                trail.destroyOnEmpty = true;
                trail.DispersalDensity = 7f;
                trail.DispersalMaxCoherency = 1f;
                trail.DispersalMinCoherency = 0.8f;
                trail.DispersalParticleSystemPrefab = srifleproj.GetComponentInChildren<TrailController>().DispersalParticleSystemPrefab;
                trail.FlipUvsY = false;
                trail.globalTimer = 0f;
                trail.rampHeight = false;
                trail.rampStartHeight = 2f;
                trail.rampTime = 1f;
                trail.softMaxLength = 0f;
                trail.startAnimation = "";
                trail.usesAnimation = true;
                trail.usesCascadeTimer = false;
                trail.UsesDispersalParticles = true;
                trail.usesGlobalTimer = true;
                trail.usesSoftMaxLength = false;
                trail.usesStartAnimation = false;

                proj.hitEffects = srifleproj.hitEffects;
            }

            var name = "Dot Zip Carbine";
            var shortdesc = "Bankrupted, creatively";
            var longdesc = "Has low stats, but very high maximum ammo and clip size. Having this weapon in your inventory allows you to hold an extra active item, and killing enemies with it slowly increases the maximum ammo and clip capacity of all guns in a linear, percentage-based manner.\n\nThe result of a catastrophically disastrous business decision from a once-beloved firearms and computing company, created after its sole owner became obssessed with the idea of a hyper-compact, cheap computer-gun. The Dot Zip Carbine turned out to be janky and unreliable, and its computerized stock was unable to do anything other than compressing files. Although it is responsible for its company going bankrupt, the Gungeon appears to have pitied this weapon, and has endowed its frame with the ability to compress and store much more than simple computer files.";

            var gun = EasyGunInit("dot_zip_carbine", name, shortdesc, longdesc, 600, 1.5f, HegemonyRifleObject.muzzleFlashEffects, "Battery", PickupObject.ItemQuality.C, GunClass.RIFLE, out var finish);

            var proj = EasyProjectileInit<Projectile>("dotzipcarbineprojectile", 4f, 140f, 1000f, 20f, true, false, true, srifleproj.GetComponentInChildren<tk2dSprite>().spriteId, srifleproj.GetComponentInChildren<tk2dSprite>().Collection);
            SetupLightningProjectile(proj);

            gun.RawSourceVolley.projectiles.Add(new()
            {
                shootStyle = ProjectileModule.ShootStyle.Automatic,
                projectiles = new()
                {
                    proj
                },
                cooldownTime = 0.2f,
                numberOfShotsInClip = 20,
                ammoType = GameUIAmmoType.AmmoType.CUSTOM,
                angleVariance = 10f,
                customAmmoType = AddCustomAmmoType("ccr_dotzipcarbine", "dotzipcarbineammotype", "dot_zip_carbine_clip")
            });

            var controller = gun.AddComponent<DotZipCarbine>();

            controller.statsPerKill = 0.0005f;

            controller.zipBombSynergyProjectile = EasyProjectileInit<Projectile>("dotzipcarbinesynergyprojectile", 9f, 140f, 1000f, 20f, true, false, true, srifleproj.GetComponentInChildren<tk2dSprite>().spriteId, srifleproj.GetComponentInChildren<tk2dSprite>().Collection);
            SetupLightningProjectile(controller.zipBombSynergyProjectile);

            controller.zipBombSynergyExplodeChance = 0.25f;
            controller.zipBombSynergyExplosionData = ExplosiveRoundsObject.ExplosionData;

            controller.fileCompressionSynergyClipMult = 0.75f;
            controller.fileCompressionSynergyDamageMult = 2f;

            finish();

            var transform = controller.AddComponent<TransformGunSynergyProcessor>();

            transform.SynergyToCheck = CustomSynergyTypeE.YOU_WIN_RAR;
            transform.NonSynergyGunId = gun.PickupObjectId;
            transform.SynergyGunId = InitWinRar();
        }

        public static int InitWinRar()
        {
            var gun = EasyGunInit("winrar", "WinRar", "", "", 600, 1.5f, HegemonyRifleObject.muzzleFlashEffects, "Battery", PickupObject.ItemQuality.EXCLUDED, GunClass.RIFLE, out var finish, overrideConsoleId: $"ccr:dot_zip_carbine+you_win_rar");

            var ammotype = AddCustomAmmoType("ccr_winrar", "winrarammotype", "winrar_clip");
            var proj = Plugin.bundle.LoadAsset<GameObject>("dotzipcarbineprojectile").GetComponent<Projectile>();
            var numProjectiles = 4;

            for (int i = 0; i < numProjectiles; i++)
            {
                gun.RawSourceVolley.projectiles.Add(new()
                {
                    shootStyle = ProjectileModule.ShootStyle.Automatic,
                    projectiles = new()
                    {
                        proj
                    },
                    cooldownTime = 0.35f,
                    numberOfShotsInClip = 20,
                    ammoType = GameUIAmmoType.AmmoType.CUSTOM,
                    angleVariance = 20f,
                    customAmmoType = ammotype,
                    ammoCost = i == 0 ? 1 : 0
                });
            }

            gun.RawSourceVolley.UsesShotgunStyleVelocityRandomizer = true;

            gun.RawSourceVolley.IncreaseFinalSpeedPercentMax = 15f;
            gun.RawSourceVolley.DecreaseFinalSpeedPercentMin = 15f;

            finish();

            return gun.PickupObjectId;
        }

        public override void OnPlayerPickup(PlayerController playerOwner)
        {
            base.OnPlayerPickup(playerOwner);

            ammoMod = StatModifier.Create(PlayerStats.StatType.AmmoCapacityMultiplier, ModifyMethod.MULTIPLICATIVE, stats);
            clipMod = StatModifier.Create(PlayerStats.StatType.AdditionalClipCapacityMultiplier, ModifyMethod.MULTIPLICATIVE, stats);

            playerOwner.ownerlessStatModifiers.Add(ammoMod);
            playerOwner.ownerlessStatModifiers.Add(clipMod);

            playerOwner.stats.RecalculateStats(playerOwner, false, false);

            playerOwner.Ext().OnExplosion += ZipBombSynergy;
        }

        public override void PostProcessProjectile(Projectile projectile)
        {
            base.PostProcessProjectile(projectile);

            projectile.OnHitEnemy += AddStats;

            if (!PlayerOwner)
                return;

            if (PlayerOwner.HasActiveBonusSynergy(CustomSynergyTypeE.FILE_COMPRESSION))
            {
                projectile.baseData.damage *= fileCompressionSynergyDamageMult;
            }

            if(PlayerOwner.HasActiveBonusSynergy(CustomSynergyTypeE.ZIP_BOMB) && Random.value < zipBombSynergyExplodeChance)
            {
                var kaboom = projectile.AddComponent<ExplosiveModifier>();

                kaboom.doExplosion = true;
                kaboom.explosionData = zipBombSynergyExplosionData;
                kaboom.IgnoreQueues = true;
            }

            if (PlayerOwner.HasActiveBonusSynergy(CustomSynergyTypeE.KNOT_A_TYPO))
            {
                var proj = OwnedShootProjectile(projectile, projectile.transform.position, Random.Range(0f, 360f), GenericOwner);

                if(proj != null)
                {
                    proj.OnHitEnemy += AddStats;

                    if(PlayerOwner)
                        PlayerOwner.DoPostProcessProjectile(proj);
                }
            }
        }

        public void AddStats(Projectile proj, SpeculativeRigidbody body, bool killed)
        {
            if (killed && body != null && body.aiActor != null && body.aiActor.CanDropCurrency)
            {
                stats += statsPerKill;

                if (ammoMod != null)
                    ammoMod.amount = stats;

                if(clipMod != null)
                    clipMod.amount = stats;

                if(PlayerOwner != null)
                    PlayerOwner.stats.RecalculateStats(PlayerOwner, false, false);
            }
        }

        public void ZipBombSynergy(ExplosionData explosion, Vector3 pos, PlayerController pl)
        {
            if (pl.HasActiveBonusSynergy(CustomSynergyTypeE.ZIP_BOMB) && pl.CurrentRoom != null && pl.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All) != null)
            {
                var enm = pl.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All).Where(x => x != null);

                var enemiesToShootAt = enm.Where(x => x.IsWorthShootingAt).OrderBy(x => Vector2.Distance(x.CenterPosition, pos)).Concat(enm.Where(x => !x.IsWorthShootingAt).OrderBy(x => Vector2.Distance(x.CenterPosition, pos))).Take(8);

                foreach (var en in enemiesToShootAt)
                {
                    var proj = OwnedShootProjectile(zipBombSynergyProjectile, pos, (en.CenterPosition - pos.XY()).ToAngle(), pl);

                    if (proj != null)
                    {
                        proj.OnHitEnemy += AddStats;

                        pl.DoPostProcessProjectile(proj);
                    }
                }
            }
        }

        public override void DisableEffectPlayer(PlayerController player)
        {
            base.DisableEffectPlayer(player);

            if(ammoMod != null)
                player.ownerlessStatModifiers.Remove(ammoMod);

            if(clipMod != null)
                player.ownerlessStatModifiers.Remove(clipMod);

            ammoMod = null;
            clipMod = null;

            player.stats.RecalculateStats(player, false, false);

            player.Ext().OnExplosion -= ZipBombSynergy;
        }

        public override void PostProcessVolley(ProjectileVolleyData volley)
        {
            base.PostProcessVolley(volley);

            if (!PlayerOwner || !PlayerOwner.HasActiveBonusSynergy(CustomSynergyTypeE.FILE_COMPRESSION))
                return;

            foreach(var mod in volley.projectiles)
            {
                if (mod.numberOfShotsInClip <= 1)
                    continue;

                mod.numberOfShotsInClip = Mathf.Max(Mathf.RoundToInt(mod.numberOfShotsInClip * fileCompressionSynergyClipMult / 1.5f), 1);
            }
        }

        public float statsPerKill;
        public float stats = 1f;

        private StatModifier ammoMod;
        private StatModifier clipMod;

        public Projectile zipBombSynergyProjectile;

        public float zipBombSynergyExplodeChance;
        public ExplosionData zipBombSynergyExplosionData;

        public float fileCompressionSynergyClipMult;
        public float fileCompressionSynergyDamageMult;
    }
}
