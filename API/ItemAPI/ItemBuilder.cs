global using static ReturnUnusedCharacters.API.ItemAPI.ItemBuilder;
using Gungeon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.API.ItemAPI
{
    public static class ItemBuilder
    {
        /// <summary>
        /// Sets the base assembly of the ResourceExtractor, so 
        /// resources can be accessed
        /// </summary>
        public static void InitItemBuilder()
        {
            try
            {
                sharedAssets = ResourceManager.LoadAssetBundle("shared_auto_001");
                Shop_Key_Items_01 = sharedAssets.LoadAsset<GenericLootTable>("Shop_Key_Items_01");
                Shop_Truck_Items_01 = sharedAssets.LoadAsset<GenericLootTable>("Shop_Truck_Items_01");
                Shop_Curse_Items_01 = sharedAssets.LoadAsset<GenericLootTable>("Shop_Curse_Items_01");
                Shop_Goop_Items_01 = sharedAssets.LoadAsset<GenericLootTable>("Shop_Goop_Items_01");
                Shop_Blank_Items_01 = sharedAssets.LoadAsset<GenericLootTable>("Shop_Blank_Items_01");
                ForgeDungeonPrefab = DungeonDatabase.GetOrLoadByName("Base_Forge");
                BlacksmithShop = ForgeDungeonPrefab.PatternSettings.flows[0].AllNodes[10].overrideExactRoom;
                BlackSmith_Items_01 = (BlacksmithShop.placedObjects[8].nonenemyBehaviour as BaseShopController).shopItemsGroup2;
                ForgeDungeonPrefab = null;
                ItemIds = new();
                Passive = new();
                Active = new();
                Guns = new();
                Item = new();
            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.Message);
                ETGModConsole.Log(e.StackTrace);
            }
        }

        public enum CooldownType
        {
            Timed, Damage, PerRoom, None
        }

        public static PlayerOrbital EasyGuonInit(string objectPath, IntVector2 hitboxSize, float orbitRadius = 2.5f, float degreesPerSecond = 120f, int tier = 0, bool shouldRotate = false,
            IntVector2? additionalHitboxPixelOffset = null, CollisionLayer? overrideCollisionLayer = null)
        {
            var go = Plugin.bundle.LoadAsset<GameObject>(objectPath);

            go.transform.Find("Sprite").GetComponent<tk2dSprite>().CurrentSprite.ReplaceShader("tk2d/CutoutVertexColorTintableTilted");

            var rigidbody = go.AddComponent<SpeculativeRigidbody>();
            var hitboxOffset = additionalHitboxPixelOffset ?? IntVector2.Zero;

            rigidbody.PixelColliders = new()
            {
                new()
                {
                    CollisionLayer = overrideCollisionLayer ?? CollisionLayer.EnemyBulletBlocker,
                    ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual,
                    ManualOffsetX = -hitboxSize.x / 2 + hitboxOffset.x,
                    ManualOffsetY = -hitboxSize.y / 2 + hitboxOffset.y,
                    ManualWidth = hitboxSize.x,
                    ManualHeight = hitboxSize.y
                }
            };

            var guon = go.AddComponent<PlayerOrbital>();
            guon.orbitRadius = orbitRadius;
            guon.orbitDegreesPerSecond = degreesPerSecond;
            guon.SetOrbitalTier(tier);
            guon.shouldRotate = shouldRotate;

            SpecialAssets.assets.Add(go);

            return guon;
        }

        public static T EasyItemInit<T>(string objectPath, string itemName, string itemShortDesc, string itemLongDesc, PickupObject.ItemQuality quality, int? ammonomiconPlacement = null, string overrideConsoleID = null)
            where T : PickupObject
        {
            GameObject go = Plugin.bundle.LoadAsset<GameObject>(objectPath);
            T item = go.AddComponent<T>();
            item.GetComponent<tk2dSprite>().CurrentSprite.ReplaceShader("tk2d/CutoutVertexColorTintableTilted");
            SetupItem(item, itemName, itemShortDesc, itemLongDesc, "ccr", overrideConsoleID);
            item.quality = quality;
            if (ammonomiconPlacement != null)
            {
                item.PlaceItemInAmmonomiconAfterItemById(ammonomiconPlacement.Value);
            }
            if (quality == PickupObject.ItemQuality.SPECIAL || quality == PickupObject.ItemQuality.EXCLUDED)
            {
                item.RemovePickupFromLootTables();
            }
            return item;
        }

        public static PickupObject RemovePickupFromLootTables(this PickupObject po)
        {
            WeightedGameObject go1 = GameManager.Instance.RewardManager.GunsLootTable.defaultItemDrops.FindWeightedGameObjectInCollection(po);
            if (go1 != null)
            {
                GameManager.Instance.RewardManager.GunsLootTable.defaultItemDrops.elements.Remove(go1);
            }
            WeightedGameObject go2 = GameManager.Instance.RewardManager.ItemsLootTable.defaultItemDrops.FindWeightedGameObjectInCollection(po);
            if (go2 != null)
            {
                GameManager.Instance.RewardManager.ItemsLootTable.defaultItemDrops.elements.Remove(go2);
            }
            return po;
        }

        public static WeightedGameObject FindWeightedGameObjectInCollection(this WeightedGameObjectCollection collection, PickupObject po)
        {
            WeightedGameObject go = collection.FindWeightedGameObjectInCollection(po.PickupObjectId);
            if (go == null)
            {
                go = collection.FindWeightedGameObjectInCollection(po.gameObject);
            }
            return go;
        }

        public static WeightedGameObject FindWeightedGameObjectInCollection(this WeightedGameObjectCollection collection, int id)
        {
            foreach (WeightedGameObject go in collection.elements)
            {
                if (go.pickupId == id)
                {
                    return go;
                }
            }
            return null;
        }

        public static WeightedGameObject FindWeightedGameObjectInCollection(this WeightedGameObjectCollection collection, GameObject obj)
        {
            foreach (WeightedGameObject go in collection.elements)
            {
                if (go.gameObject == obj)
                {
                    return go;
                }
            }
            return null;
        }

        public static PickupObject PlaceItemInAmmonomiconAfterItemById(this PickupObject item, int id)
        {
            item.ForcedPositionInAmmonomicon = PickupObjectDatabase.GetById(id).ForcedPositionInAmmonomicon;
            return item;
        }

        public static int AddToAmmonomicon(tk2dSpriteDefinition spriteDefinition)
        {
            return AddSpriteToCollection(spriteDefinition, ammonomiconColl);
        }

        public static tk2dSpriteCollectionData ammonomiconColl = AmmonomiconController.ForceInstance.EncounterIconCollection;

        public static int AddSpriteToCollection(tk2dSpriteDefinition spriteDefinition, tk2dSpriteCollectionData collection)
        {
            //Add definition to collection
            var defs = collection.spriteDefinitions;
            var newDefs = defs.Concat(new tk2dSpriteDefinition[] { spriteDefinition }).ToArray();
            collection.spriteDefinitions = newDefs;

            //Reset lookup dictionary
            collection.spriteNameLookupDict = null;
            collection.InitDictionary(); //InitDictionary only runs if the dictionary is null
            return newDefs.Length - 1;
        }

        /// <summary>
        /// Finishes the item setup, adds it to the item databases, adds an encounter trackable 
        /// blah, blah, blah
        /// </summary>
        public static void SetupItem(PickupObject item, string name, string shortDesc, string longDesc, string idPool, string overrideConsoleId = null)
        {
            try
            {
                item.encounterTrackable = null;
                var notSpapiName = item.name;
                item.gameObject.name = $"ccr_{item.gameObject.name}";
                ETGMod.Databases.Items.SetupItem(item, item.name);
                AddToAmmonomicon(item.sprite.GetCurrentSpriteDef());
                item.encounterTrackable.journalData.AmmonomiconSprite = item.sprite.GetCurrentSpriteDef().name;
                item.SetName(name);
                item.SetShortDescription(shortDesc);
                item.SetLongDescription(longDesc);
                if (item is PlayerItem)
                    (item as PlayerItem).consumable = false;
                if (!string.IsNullOrEmpty(overrideConsoleId))
                {
                    Game.Items.Add(overrideConsoleId, item);
                }
                else
                {
                    Game.Items.Add(idPool + ":" + name.ToID(), item);
                }
                ETGMod.Databases.Items.AddSpecific(false, item);
                ItemIds.Add(notSpapiName.ToLowerInvariant(), item.PickupObjectId);
                if (item is PassiveItem passive)
                {
                    Passive.Add(notSpapiName.ToLowerInvariant(), passive);
                }
                else if (item is PlayerItem active)
                {
                    Active.Add(notSpapiName.ToLowerInvariant(), active);
                }
                Item.Add(notSpapiName.ToLowerInvariant(), item);
            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.Message);
                ETGModConsole.Log(e.StackTrace);
            }
        }

        public static Gun SetupBasicGunComponents(GameObject obj)
        {
            Gun gun = obj.AddComponent<Gun>();
            gun.gunName = "gun";
            gun.gunSwitchGroup = string.Empty;
            gun.isAudioLoop = false;
            gun.lowersAudioWhileFiring = false;
            gun.gunClass = GunClass.NONE;
            gun.currentGunStatModifiers = new StatModifier[0];
            gun.passiveStatModifiers = new StatModifier[0];
            gun.currentGunDamageTypeModifiers = new DamageTypeModifier[0];
            gun.barrelOffset = obj.transform.Find("Barrel");
            gun.muzzleOffset = null;
            gun.chargeOffset = null;
            gun.reloadOffset = null;
            gun.carryPixelOffset = new IntVector2(2, 0);
            gun.carryPixelUpOffset = new IntVector2(0, 0);
            gun.carryPixelDownOffset = new IntVector2(0, 0);
            gun.UsesPerCharacterCarryPixelOffsets = false;
            gun.PerCharacterPixelOffsets = new CharacterCarryPixelOffset[0];
            gun.leftFacingPixelOffset = new IntVector2(0, 0);
            gun.overrideOutOfAmmoHandedness = GunHandedness.AutoDetect;
            gun.additionalHandState = AdditionalHandState.None;
            gun.gunPosition = GunPositionOverride.AutoDetect;
            gun.forceFlat = false;
            gun.RawSourceVolley = ScriptableObject.CreateInstance<ProjectileVolleyData>();
            gun.RawSourceVolley.projectiles = new List<ProjectileModule>();
            gun.singleModule = null;
            gun.rawOptionalReloadVolley = null;
            gun.OverrideFinaleAudio = false;
            gun.HasFiredHolsterShot = false;
            gun.HasFiredReloadSynergy = false;
            gun.modifiedVolley = null;
            gun.modifiedFinalVolley = null;
            gun.modifiedOptionalReloadVolley = null;
            gun.DuctTapeMergedGunIDs = null;
            gun.PreventNormalFireAudio = false;
            gun.OverrideNormalFireAudioEvent = string.Empty;
            gun.ammo = 100;
            gun.CanGainAmmo = true;
            gun.InfiniteAmmo = false;
            gun.UsesBossDamageModifier = false;
            gun.CustomBossDamageModifier = 1f;
            gun.reloadTime = 1f;
            gun.CanReloadNoMatterAmmo = false;
            gun.blankDuringReload = false;
            gun.blankReloadRadius = 0f;
            gun.reflectDuringReload = false;
            gun.blankKnockbackPower = 0f;
            gun.blankDamageToEnemies = 0f;
            gun.blankDamageScalingOnEmptyClip = 1f;
            gun.doesScreenShake = false;
            gun.gunScreenShake = new ScreenShakeSettings();
            gun.directionlessScreenShake = false;
            gun.damageModifier = 0;
            gun.thrownObject = null;
            gun.procGunData = null;
            gun.activeReloadData = null;
            gun.ClearsCooldownsLikeAWP = false;
            gun.AppliesHoming = false;
            gun.AppliedHomingAngularVelocity = 0f;
            gun.AppliedHomingDetectRadius = 0f;
            gun.shootAnimation = string.Empty;
            gun.usesContinuousFireAnimation = false;
            gun.reloadAnimation = string.Empty;
            gun.emptyReloadAnimation = string.Empty;
            gun.idleAnimation = string.Empty;
            gun.chargeAnimation = string.Empty;
            gun.dischargeAnimation = string.Empty;
            gun.emptyAnimation = string.Empty;
            gun.introAnimation = string.Empty;
            gun.finalShootAnimation = string.Empty;
            gun.enemyPreFireAnimation = string.Empty;
            gun.outOfAmmoAnimation = string.Empty;
            gun.criticalFireAnimation = string.Empty;
            gun.dodgeAnimation = string.Empty;
            gun.usesDirectionalIdleAnimations = false;
            gun.usesDirectionalAnimator = false;
            gun.preventRotation = false;
            gun.muzzleFlashEffects = new VFXPool { type = VFXPoolType.None, effects = new VFXComplex[0] };
            gun.usesContinuousMuzzleFlash = false;
            gun.finalMuzzleFlashEffects = new VFXPool { type = VFXPoolType.None, effects = new VFXComplex[0] };
            gun.reloadEffects = new VFXPool { type = VFXPoolType.None, effects = new VFXComplex[0] };
            gun.emptyReloadEffects = new VFXPool { type = VFXPoolType.None, effects = new VFXComplex[0] };
            gun.activeReloadSuccessEffects = new VFXPool { type = VFXPoolType.None, effects = new VFXComplex[0] };
            gun.activeReloadFailedEffects = new VFXPool { type = VFXPoolType.None, effects = new VFXComplex[0] };
            gun.light = null;
            gun.baseLightIntensity = 0f;
            gun.shellCasing = null;
            gun.shellsToLaunchOnFire = 0;
            gun.shellCasingOnFireFrameDelay = 0;
            gun.shellsToLaunchOnReload = 0;
            gun.reloadShellLaunchFrame = 0;
            gun.clipObject = null;
            gun.clipsToLaunchOnReload = 0;
            gun.reloadClipLaunchFrame = 0;
            gun.prefabName = string.Empty;
            gun.rampBullets = false;
            gun.rampStartHeight = 0f;
            gun.rampTime = 0f;
            gun.IgnoresAngleQuantization = false;
            gun.IsTrickGun = false;
            gun.TrickGunAlternatesHandedness = false;
            gun.PreventOutlines = false;
            gun.alternateVolley = null;
            gun.alternateShootAnimation = string.Empty;
            gun.alternateReloadAnimation = string.Empty;
            gun.alternateIdleAnimation = string.Empty;
            gun.alternateSwitchGroup = string.Empty;
            gun.IsHeroSword = false;
            gun.HeroSwordDoesntBlank = false;
            gun.StarterGunForAchievement = false;
            gun.CanSneakAttack = false;
            gun.SneakAttackDamageMultiplier = 1f;
            gun.SuppressLaserSight = false;
            gun.RequiresFundsToShoot = false;
            gun.CurrencyCostPerShot = 1;
            gun.weaponPanelSpriteOverride = null;
            gun.IsLuteCompanionBuff = false;
            gun.MovesPlayerForwardOnChargeFire = false;
            gun.LockedHorizontalOnCharge = false;
            gun.LockedHorizontalCenterFireOffset = 0f;
            gun.LockedHorizontalOnReload = false;
            gun.GoopReloadsFree = false;
            gun.IsUndertaleGun = false;
            gun.LocalActiveReload = false;
            gun.UsesRechargeLikeActiveItem = false;
            gun.ActiveItemStyleRechargeAmount = 0f;
            gun.CanAttackThroughObjects = false;
            gun.CanCriticalFire = false;
            gun.CriticalChance = 1f;
            gun.CriticalDamageMultiplier = 1f;
            gun.CriticalMuzzleFlashEffects = new VFXPool { type = VFXPoolType.None };
            gun.CriticalReplacementProjectile = null;
            gun.GainsRateOfFireAsContinueAttack = false;
            gun.RateOfFireMultiplierAdditionPerSecond = 0f;
            gun.OnlyUsesIdleInWeaponBox = false;
            gun.DisablesRendererOnCooldown = false;
            gun.ObjectToInstantiateOnReload = null;
            gun.AdditionalClipCapacity = 0;
            gun.LastShotIndex = -1;
            gun.DidTransformGunThisFrame = false;
            gun.CustomLaserSightDistance = 30f;
            gun.CustomLaserSightHeight = 0.25f;
            gun.LastLaserSightEnemy = null;
            gun.HasEverBeenAcquiredByPlayer = false;
            gun.HasBeenPickedUp = false;
            gun.OverrideAngleSnap = null;
            gun.SetBaseMaxAmmo(100);
            if (obj.transform.Find("SecondaryHand") != null)
            {
                gun.gunHandedness = GunHandedness.TwoHanded;
            }
            else
            {
                gun.gunHandedness = GunHandedness.OneHanded;
            }
            obj.AddComponent<EncounterTrackable>();
            return gun;
        }

        public static Gun EasyGunInit(string assetPath, string gunName, string gunShortDesc, string gunLongDesc, int maxAmmo, float realoadTime, VFXPool muzzleflash, string gunSwitchGroup, PickupObject.ItemQuality quality, GunClass gunClass, out Action finish, int? ammonomiconPlacement = null, string overrideConsoleId = null, GameObject baseObject = null)
        {
            GameObject gameObject = baseObject ?? Plugin.bundle.LoadAsset<GameObject>(assetPath);
            Gun gun = SetupBasicGunComponents(gameObject);

            gun.gunName = gunName;
            gun.DefaultSpriteID = gun.sprite.spriteId;

            Game.Items.Add(overrideConsoleId ?? $"ccr:{gunName.ToID()}", gun);

            gun.NewUpdateAnimations();

            var notSpapiName = gun.name;

            gun.gameObject.name = $"ccr_{gun.gameObject.name}";
            string keyName = "#" + gun.name.Replace(" ", "").ToUpperInvariant();

            gun.encounterTrackable.journalData = new()
            {
                AmmonomiconSprite = gun.sprite.CurrentSprite.name,
                PrimaryDisplayName = keyName + "_ENCNAME",
                NotificationPanelDescription = keyName + "_SHORTDESC",
                AmmonomiconFullEntry = keyName + "_LONGDESC"
            };

            AddToAmmonomicon(gun.sprite.CurrentSprite);

            gun.encounterTrackable.EncounterGuid = gun.gameObject.name.RemoveUnacceptableCharactersForGUID();
            gun.encounterTrackable.prerequisites = new DungeonPrerequisite[0];
            gun.encounterTrackable.journalData.SuppressKnownState = false;

            gun.SetName(gunName);
            gun.SetShortDescription(gunShortDesc);
            gun.SetLongDescription(gunLongDesc);
            gun.SetBaseMaxAmmo(maxAmmo);

            gun.ammo = maxAmmo;
            gun.quality = quality;
            gun.gunClass = gunClass;
            gun.reloadTime = realoadTime;
            gun.gunSwitchGroup = gunSwitchGroup;

            if (muzzleflash != null)
            {
                gun.muzzleFlashEffects = muzzleflash;
            }

            if (ammonomiconPlacement != null)
            {
                gun.PlaceItemInAmmonomiconAfterItemById(ammonomiconPlacement.Value);
            }

            finish = () =>
            {
                ETGMod.Databases.Items.AddSpecific(gun, false, "ANY");
                ItemIds.Add(notSpapiName.ToLowerInvariant(), gun.PickupObjectId);
                Guns.Add(notSpapiName.ToLowerInvariant(), gun);
                Item.Add(notSpapiName.ToLowerInvariant(), gun);
            };

            return gun;
        }

        public static T SetupBasicProjectileComponents<T>(GameObject obj) where T : Projectile
        {
            var body = obj.AddComponent<SpeculativeRigidbody>();
            body.TK2DSprite = obj.GetComponentInChildren<tk2dSprite>();

            body.PixelColliders = new()
            {
                new()
                {
                    ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Tk2dPolygon,
                    CollisionLayer = CollisionLayer.Projectile,
                    Sprite = body.TK2DSprite
                }
            };

            var proj = obj.AddComponent<T>();
            proj.baseData = new ProjectileData();
            proj.persistTime = 0f;
            proj.AdditionalBurstLimits = new SynergyBurstLimit[0];
            return proj;
        }

        public static T EasyProjectileInit<T>(string assetPath, float damage, float speed, float range, float knockback, bool shouldRotate, bool ignoreDamageCaps, bool pierceMinorBreakables, int? overrideSpriteId = null, tk2dSpriteCollectionData overrideSpriteCollection = null, tk2dBaseSprite.Anchor anchor = tk2dBaseSprite.Anchor.MiddleCenter, int? overrideColliderPixelWidth = null, int? overrideColliderPixelHeight = null, int? overrideColliderOffsetX = null, int? overrideColliderOffsetY = null) where T : Projectile
        {
            T proj = SetupBasicProjectileComponents<T>(Plugin.bundle.LoadAsset<GameObject>(assetPath));
            if (overrideSpriteId != null)
            {
                proj.GetComponentInChildren<tk2dSprite>()?.SetSprite(overrideSpriteCollection ?? ETGMod.Databases.Items.ProjectileCollection, overrideSpriteId.Value);
            }
            if (proj.GetComponentInChildren<tk2dSprite>() == null)
            {
                int xOffset = 0;
                if (anchor == tk2dBaseSprite.Anchor.LowerCenter || anchor == tk2dBaseSprite.Anchor.MiddleCenter || anchor == tk2dBaseSprite.Anchor.UpperCenter)
                {
                    xOffset = -(overrideColliderPixelHeight.GetValueOrDefault() / 2);
                }
                else if (anchor == tk2dBaseSprite.Anchor.LowerRight || anchor == tk2dBaseSprite.Anchor.MiddleRight || anchor == tk2dBaseSprite.Anchor.UpperRight)
                {
                    xOffset = -overrideColliderPixelHeight.GetValueOrDefault();
                }
                int yOffset = 0;
                if (anchor == tk2dBaseSprite.Anchor.MiddleLeft || anchor == tk2dBaseSprite.Anchor.MiddleCenter || anchor == tk2dBaseSprite.Anchor.MiddleLeft)
                {
                    yOffset = -(overrideColliderPixelHeight.GetValueOrDefault() / 2);
                }
                else if (anchor == tk2dBaseSprite.Anchor.UpperLeft || anchor == tk2dBaseSprite.Anchor.UpperCenter || anchor == tk2dBaseSprite.Anchor.UpperRight)
                {
                    yOffset = -overrideColliderPixelHeight.GetValueOrDefault();
                }
                proj.specRigidbody.PixelColliders = new List<PixelCollider>()
                {
                    new PixelCollider()
                    {
                        ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual,
                        CollisionLayer = CollisionLayer.Projectile,
                        ManualHeight = overrideColliderPixelHeight.GetValueOrDefault(),
                        ManualWidth = overrideColliderPixelWidth.GetValueOrDefault(),
                        ManualOffsetX = overrideColliderOffsetX.GetValueOrDefault() + xOffset,
                        ManualOffsetY = overrideColliderOffsetY.GetValueOrDefault() + yOffset
                    }
                };
            }
            proj.baseData.damage = damage;
            proj.baseData.speed = speed;
            proj.baseData.range = range;
            proj.baseData.force = knockback;
            proj.shouldRotate = shouldRotate;
            proj.shouldFlipVertically = true;
            proj.ignoreDamageCaps = ignoreDamageCaps;
            proj.pierceMinorBreakables = pierceMinorBreakables;
            SpecialAssets.assets.Add(proj.gameObject);
            return proj;
        }

        public static void NewUpdateAnimations(this Gun gun)
        {
            gun.idleAnimation = gun.NewUpdateAnimation("idle");
            gun.dodgeAnimation = gun.NewUpdateAnimation("dodge");
            gun.introAnimation = gun.NewUpdateAnimation("intro");
            gun.emptyAnimation = gun.NewUpdateAnimation("empty");
            gun.shootAnimation = gun.NewUpdateAnimation("fire");
            if (gun.spriteAnimator.Library.GetClipByName(gun.shootAnimation) != null && gun.spriteAnimator.Library.GetClipByName(gun.shootAnimation).wrapMode is tk2dSpriteAnimationClip.WrapMode.Loop or tk2dSpriteAnimationClip.WrapMode.LoopSection)
            {
                gun.usesContinuousFireAnimation = true;
            }
            gun.reloadAnimation = gun.NewUpdateAnimation("reload");
            gun.chargeAnimation = gun.NewUpdateAnimation("charge");
            gun.outOfAmmoAnimation = gun.NewUpdateAnimation("out_of_ammo");
            gun.dischargeAnimation = gun.NewUpdateAnimation("discharge");
            gun.finalShootAnimation = gun.NewUpdateAnimation("final_fire");
            gun.emptyReloadAnimation = gun.NewUpdateAnimation("empty_reload");
            gun.criticalFireAnimation = gun.NewUpdateAnimation("critical_fire");
            gun.enemyPreFireAnimation = gun.NewUpdateAnimation("enemy_pre_fire");
            gun.alternateShootAnimation = gun.NewUpdateAnimation("alternate_shoot");
            gun.alternateReloadAnimation = gun.NewUpdateAnimation("alternate_reload");
            gun.alternateIdleAnimation = gun.NewUpdateAnimation("alternate_idle");
        }

        public static string NewUpdateAnimation(this Gun g, string name)
        {
            var fullname = $"{g.name.ToLowerInvariant()}_{name}";

            if (g.spriteAnimator.Library.GetClipByName(fullname) != null)
            {
                return fullname;
            }
            return "";
        }

        /// <summary>
        /// Sets the cooldown type and length of a PlayerItem, and resets all other cooldown types
        /// </summary>
        public static PlayerItem SetCooldownType(this PlayerItem item, CooldownType cooldownType, float value)
        {
            item.damageCooldown = 0;
            item.roomCooldown = 0;
            item.timeCooldown = 0;

            switch (cooldownType)
            {
                case CooldownType.Timed:
                    item.timeCooldown = value;
                    break;
                case CooldownType.Damage:
                    item.damageCooldown = value;
                    break;
                case CooldownType.PerRoom:
                    item.roomCooldown = (int)value;
                    break;
            }
            return item;
        }

        public static PickupObject AddToFlyntShop(this PickupObject po)
        {
            Shop_Key_Items_01.defaultItemDrops.Add(new WeightedGameObject
            {
                rawGameObject = null,
                pickupId = po.PickupObjectId,
                weight = 1f,
                forceDuplicatesPossible = false,
                additionalPrerequisites = new DungeonPrerequisite[0]
            });
            return po;
        }

        public static PickupObject AddToTrorkShop(this PickupObject po)
        {
            Shop_Truck_Items_01.defaultItemDrops.Add(new WeightedGameObject
            {
                rawGameObject = null,
                pickupId = po.PickupObjectId,
                weight = 1f,
                forceDuplicatesPossible = false,
                additionalPrerequisites = new DungeonPrerequisite[0]
            });
            return po;
        }

        public static PickupObject AddToGooptonShop(this PickupObject po)
        {
            Shop_Goop_Items_01.defaultItemDrops.Add(new WeightedGameObject
            {
                rawGameObject = null,
                pickupId = po.PickupObjectId,
                weight = 1f,
                forceDuplicatesPossible = false,
                additionalPrerequisites = new DungeonPrerequisite[0]
            });
            return po;
        }

        public static PickupObject AddToCursulaShop(this PickupObject po)
        {
            Shop_Curse_Items_01.defaultItemDrops.Add(new WeightedGameObject
            {
                rawGameObject = null,
                pickupId = po.PickupObjectId,
                weight = 1f,
                forceDuplicatesPossible = false,
                additionalPrerequisites = new DungeonPrerequisite[0]
            });
            return po;
        }

        public static PickupObject AddToOldRedShop(this PickupObject po)
        {
            Shop_Blank_Items_01.defaultItemDrops.Add(new WeightedGameObject
            {
                rawGameObject = null,
                pickupId = po.PickupObjectId,
                weight = 1f,
                forceDuplicatesPossible = false,
                additionalPrerequisites = new DungeonPrerequisite[0]
            });
            return po;
        }

        public static PickupObject AddToBlacksmithShop(this PickupObject po)
        {
            BlackSmith_Items_01.defaultItemDrops.Add(new WeightedGameObject
            {
                rawGameObject = null,
                pickupId = po.PickupObjectId,
                weight = 1f,
                forceDuplicatesPossible = false,
                additionalPrerequisites = new DungeonPrerequisite[0]
            });
            return po;
        }

        /// <summary>
        /// Adds a passive player stat modifier to a PlayerItem or PassiveItem
        /// </summary>
        public static PickupObject AddPassiveStatModifier(this PickupObject po, PlayerStats.StatType statType, float amount, ModifyMethod method = StatModifier.ModifyMethod.ADDITIVE)
        {
            StatModifier modifier = new StatModifier
            {
                amount = amount,
                statToBoost = statType,
                modifyType = method
            };
            if (po is PlayerItem)
            {
                var item = (po as PlayerItem);
                if (item.passiveStatModifiers == null)
                    item.passiveStatModifiers = new StatModifier[] { modifier };
                else
                    item.passiveStatModifiers = item.passiveStatModifiers.Concat(new StatModifier[] { modifier }).ToArray();
            }
            else if (po is PassiveItem)
            {
                var item = (po as PassiveItem);
                if (item.passiveStatModifiers == null)
                    item.passiveStatModifiers = new StatModifier[] { modifier };
                else
                    item.passiveStatModifiers = item.passiveStatModifiers.Concat(new StatModifier[] { modifier }).ToArray();
            }
            else if (po is Gun)
            {
                var item = (po as Gun);
                if (item.passiveStatModifiers == null)
                    item.passiveStatModifiers = new StatModifier[] { modifier };
                else
                    item.passiveStatModifiers = item.passiveStatModifiers.Concat(new StatModifier[] { modifier }).ToArray();
            }
            else
            {
                throw new NotSupportedException("Object must be of type PlayerItem, PassiveItem or Gun!");
            }
            return po;
        }

        /*public static PickupObject AddPassiveStatModifier(this PickupObject po, string statType, float amount, ModifyMethod method = StatModifier.ModifyMethod.ADDITIVE)
        {
            var modifier = CreateCustomStatModifier(statType, amount, method);
            if (po is PlayerItem)
            {
                var item = (po as PlayerItem);
                if (item.passiveStatModifiers == null)
                    item.passiveStatModifiers = new StatModifier[] { modifier };
                else
                    item.passiveStatModifiers = item.passiveStatModifiers.Concat(new StatModifier[] { modifier }).ToArray();
            }
            else if (po is PassiveItem)
            {
                var item = (po as PassiveItem);
                if (item.passiveStatModifiers == null)
                    item.passiveStatModifiers = new StatModifier[] { modifier };
                else
                    item.passiveStatModifiers = item.passiveStatModifiers.Concat(new StatModifier[] { modifier }).ToArray();
            }
            else if (po is Gun)
            {
                var item = (po as Gun);
                if (item.passiveStatModifiers == null)
                    item.passiveStatModifiers = new StatModifier[] { modifier };
                else
                    item.passiveStatModifiers = item.passiveStatModifiers.Concat(new StatModifier[] { modifier }).ToArray();
            }
            else
            {
                throw new NotSupportedException("Object must be of type PlayerItem, PassiveItem or Gun!");
            }
            return po;
        }*/ //probably not going to need this

        public static IEnumerator HandleDuration(this PlayerItem item, float duration, PlayerController user, Action<PlayerController> OnFinish)
        {
            if (item.IsCurrentlyActive)
            {
                yield break;
            }

            item.IsCurrentlyActive = true;
            item.m_activeElapsed = 0f;
            item.m_activeDuration = duration;
            item.OnActivationStatusChanged?.Invoke(item);

            while (item.m_activeElapsed < item.m_activeDuration && item.IsCurrentlyActive)
            {
                yield return null;
            }
            item.IsCurrentlyActive = false;

            OnFinish?.Invoke(user);
            yield break;
        }

        public static AssetBundle sharedAssets;
        public static Dungeon ForgeDungeonPrefab;
        public static PrototypeDungeonRoom BlacksmithShop;
        public static GenericLootTable Shop_Key_Items_01;
        public static GenericLootTable Shop_Truck_Items_01;
        public static GenericLootTable Shop_Curse_Items_01;
        public static GenericLootTable Shop_Goop_Items_01;
        public static GenericLootTable Shop_Blank_Items_01;
        public static GenericLootTable BlackSmith_Items_01;
        public static Dictionary<string, Gun> Guns = new();
        public static Dictionary<string, PassiveItem> Passive = new();
        public static Dictionary<string, PlayerItem> Active = new();
        public static Dictionary<string, PickupObject> Item = new();
        public static Dictionary<string, int> ItemIds = new();
    }
}
