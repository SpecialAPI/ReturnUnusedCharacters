using static ReturnUnusedCharacters.SynergyBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters
{
    public static class Synergies
    {
        public static void Init()
        {
            // create synergy entries
            CreateSynergy("Rad Poisoning", CustomSynergyTypeE.RAD_POISONING, new() { ItemIds["vegas_smg"] }, new() { RadGunId, PlungerId, MonsterBloodId, BigBoyId });
            CreateSynergy("Mysterious Stranger", CustomSynergyTypeE.MYSTERIOUS_STRANGER, new() { ItemIds["vegas_smg"] }, new() { MagnumId, ShadowCloneId, FaceMelterId });
            CreateSynergy("Psychobuff", CustomSynergyTypeE.PSYCHOBUFF, new() { ItemIds["vegas_smg"] }, new() { AntibodyId, MuscleRelaxantId, MagicSweetId });
            CreateSynergy("Pewpew-Boy", CustomSynergyTypeE.PEWPEW_BOY, new() { ItemIds["vegas_smg"], IbombCompanionAppId });

            CreateSynergy("Cyberpunk", CustomSynergyTypeE.CYBERPUNK, new() { ItemIds["cyberpistol"], ItemIds["cybersmg"] });
            CreateSynergy("Lightning Fast", CustomSynergyTypeE.LIGHTNING_FAST, new(), new() { ShockRoundsId, ItemIds["cyberpistol"], ItemIds["cybersmg"] }, false, new() { StatModifier.Create(PlayerStats.StatType.RateOfFire, ModifyMethod.MULTIPLICATIVE, 1.5f), StatModifier.Create(PlayerStats.StatType.AdditionalClipCapacityMultiplier, ModifyMethod.MULTIPLICATIVE, 1.5f), StatModifier.Create(PlayerStats.StatType.ReloadSpeed, ModifyMethod.MULTIPLICATIVE, 0.5f), StatModifier.Create(PlayerStats.StatType.Accuracy, ModifyMethod.MULTIPLICATIVE, 1.5f) }, requiresAtLeastOneGunAndOneItem: true);
            CreateSynergy("New and Improved", CustomSynergyTypeE.NEW_AND_IMPROVED, new(), new() { ItemIds["cyberpistol"], ItemIds["cybersmg"], LaserSightId, ScopeId }, false, new() { StatModifier.Create(PlayerStats.StatType.AdditionalShotPiercing, ModifyMethod.ADDITIVE, 1) }, requiresAtLeastOneGunAndOneItem: true);
            CreateSynergy("Redundant Systems", CustomSynergyTypeE.REDUNDANT_SYSTEMS, new(), new() { ItemIds["cyberpistol"], ItemIds["cybersmg"], HomingBulletsId, CrutchId }, false, new() { StatModifier.Create(PlayerStats.StatType.AdditionalShotBounces, ModifyMethod.ADDITIVE, 1) }, requiresAtLeastOneGunAndOneItem: true);
            CreateSynergy("Spray and Pray", CustomSynergyTypeE.SPRAY_AND_PRAY, new(), new() { ItemIds["cyberpistol"], ItemIds["cybersmg"], EyepatchId, DoubleVisionId }, false, new() { StatModifier.Create(PlayerStats.StatType.AdditionalClipCapacityMultiplier, ModifyMethod.MULTIPLICATIVE, 2f), StatModifier.Create(PlayerStats.StatType.AmmoCapacityMultiplier, ModifyMethod.MULTIPLICATIVE, 1.5f), StatModifier.Create(PlayerStats.StatType.ReloadSpeed, ModifyMethod.MULTIPLICATIVE, 0.5f), StatModifier.Create(PlayerStats.StatType.Accuracy, ModifyMethod.MULTIPLICATIVE, 2f) }, requiresAtLeastOneGunAndOneItem: true);

            CreateSynergy("Zip Bomb", CustomSynergyTypeE.ZIP_BOMB, new() { ItemIds["dot_zip_carbine"] }, new() { BombId, IceBombId, LilBomberId, IbombCompanionAppId, SmokeBombId, RollBombId });
            CreateSynergy("Knot a Typo", CustomSynergyTypeE.KNOT_A_TYPO, new() { ItemIds["dot_zip_carbine"] }, new() { BracketKeyId, EscapeRopeId });
            CreateSynergy("You win, rar", CustomSynergyTypeE.YOU_WIN_RAR, new() { ItemIds["dot_zip_carbine"] }, new() { StuffedStarId, GalacticMedalOfValorId, CoinCrownId, BadgeId, CrownOfGunsId });
            CreateSynergy("File Compression", CustomSynergyTypeE.FILE_COMPRESSION, new() { ItemIds["dot_zip_carbine"] }, new() { BigBoyId, BigIronId, BigShotgunId, BsgId }, false, new() { StatModifier.Create(PlayerStats.StatType.AdditionalClipCapacityMultiplier, ModifyMethod.MULTIPLICATIVE, 1.5f) });

            // create synergy handlers for items
            var debuff = WolfEnemy.behaviorSpeculator.AttackBehaviors.OfType<WolfCompanionAttackBehavior>().First().EnemyDebuff;
            RadGunObject.AddComponent<RadPoisoningSynergyProcessor>().debuff = debuff;
            PlungerObject.AddComponent<RadPoisoningSynergyProcessor>().debuff = debuff;
            Guns["vegas_smg"].AddComponent<RadPoisoningSynergyProcessor>().debuff = debuff;

            AddDualWieldSynergyProcessor(Guns["cyberpistol"], Guns["cybersmg"], CustomSynergyTypeE.CYBERPUNK);

            // add synergy entries to the synergy database
            AddSynergiesToDB();
        }
    }

    public static class SynergyBuilder
    {
        /// <summary>
        /// Creates a new synergy.
        /// </summary>
        /// <param name="name">The name of the synergy.</param>
        /// <param name="mandatoryIds">Ids of items that are always required for the completion of the synergy.</param>
        /// <param name="optionalIds">Ids of "filler items" that will be needed to fill empty spaces in list of synergy-completing items.</param>
        /// <param name="activeWhenGunsUnequipped">If true, the synergy will still be active when the player is not holding the guns required for it's completion.</param>
        /// <param name="statModifiers">Stat modifiers that will be applied to the player when the synergy is active.</param>
        /// <param name="ignoreLichsEyeBullets">If true, Lich's Eye Bullets will not be able to activate the synergy.</param>
        /// <param name="numberObjectsRequired">Number of items required for the synergy's completion.</param>
        /// <param name="suppressVfx">If true, the synergy arrow VFX will not appear when the synergy is completed.</param>
        /// <param name="requiresAtLeastOneGunAndOneItem">If true, the player will have to have at least one item AND gun from either/both <paramref name="mandatoryIds"/> and <paramref name="optionalIds"/>.</param>
        /// <param name="bonusSynergies">List of "bonus synergies" for the synergy. Bonus synergies are used by base game items to detect if a synergy is active, but for modded synergies you don't need them.</param>
        /// <returns>The built synergy</returns>
        public static AdvancedSynergyEntry CreateSynergy(string name, CustomSynergyType synergyType, List<int> mandatoryIds, List<int> optionalIds = null, bool activeWhenGunsUnequipped = true, List<StatModifier> statModifiers = null, bool ignoreLichsEyeBullets = false,
            int numberObjectsRequired = 2, bool suppressVfx = false, bool requiresAtLeastOneGunAndOneItem = false)
        {
            var entry = new AdvancedSynergyEntry();

            var key = "#" + name.ToID().ToUpperInvariant();

            entry.NameKey = key;
            ETGMod.Databases.Strings.Synergy.Set(key, name);

            if (mandatoryIds != null)
            {
                foreach (int id in mandatoryIds)
                {
                    var po = PickupObjectDatabase.GetById(id);

                    if (po is Gun)
                        entry.MandatoryGunIDs.Add(id);
                    else if (po is PassiveItem or PlayerItem)
                        entry.MandatoryItemIDs.Add(id);
                }
            }

            if (optionalIds != null)
            {
                foreach (int id in optionalIds)
                {
                    var po = PickupObjectDatabase.GetById(id);

                    if (po is Gun)
                        entry.OptionalGunIDs.Add(id);
                    else if (po is PassiveItem or PlayerItem)
                        entry.OptionalItemIDs.Add(id);
                }
            }

            entry.ActiveWhenGunUnequipped = activeWhenGunsUnequipped;
            entry.IgnoreLichEyeBullets = ignoreLichsEyeBullets;
            entry.NumberObjectsRequired = numberObjectsRequired;
            entry.RequiresAtLeastOneGunAndOneItem = requiresAtLeastOneGunAndOneItem;
            entry.SuppressVFX = suppressVfx;

            entry.statModifiers = statModifiers ?? new();
            entry.bonusSynergies = new() { synergyType };

            addedSynergies.Add(entry);

            return entry;
        }

        /// <summary>
        /// Adds a <see cref="AdvancedDualWieldSynergyProcessor"/> to <paramref name="first"/> and <paramref name="second"/>.
        /// </summary>
        /// <param name="first">The first gun in the dual wield synergy.</param>
        /// <param name="second">The second gun in the dual wield synergy.</param>
        /// <param name="requiredSynergy">The synergy required for the dual wield.</param>
        public static void AddDualWieldSynergyProcessor(Gun first, Gun second, CustomSynergyType requiredSynergy)
        {
            var p1 = first.gameObject.AddComponent<DualWieldSynergyProcessor>();
            p1.SynergyToCheck = requiredSynergy;
            p1.PartnerGunID = second.PickupObjectId;

            var p2 = second.gameObject.AddComponent<DualWieldSynergyProcessor>();
            p2.SynergyToCheck = requiredSynergy;
            p2.PartnerGunID = first.PickupObjectId;
        }

        public static void AddSynergiesToDB()
        {
            var synergyManager = GameManager.Instance.SynergyManager;

            synergyManager.synergies = [.. synergyManager.synergies, .. addedSynergies];
        }

        public static List<AdvancedSynergyEntry> addedSynergies = new();
    }
}
