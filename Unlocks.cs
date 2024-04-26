using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters
{
    public static class Unlocks
    {
        public static void Init()
        {
            // add unlock components
            DragunEnemy                 .AddComponent<OnDeathCharacterUnlockHandler>().unlockType = OnDeathCharacterUnlockHandler.UnlockableType.Dragun;
            ResourcefulRatMechEnemy     .AddComponent<OnDeathCharacterUnlockHandler>().unlockType = OnDeathCharacterUnlockHandler.UnlockableType.Rat;
            InfinilichEnemy             .AddComponent<OnDeathCharacterUnlockHandler>().unlockType = OnDeathCharacterUnlockHandler.UnlockableType.Lich;


            // setup lamey unlocks
            Item["magnifyingglass"]     .SetupUnlockOnFlag(GungeonFlagsE.UNLOCKABLE_DRAGUN_LAMEY);
            Item["disguisehat"]         .SetupUnlockOnFlag(GungeonFlagsE.UNLOCKABLE_DRAGUN_LAMEY);

            Item["cyberpistol"]         .SetupUnlockOnFlag(GungeonFlagsE.UNLOCKABLE_RAT_LAMEY);
            Item["cybersmg"]            .SetupUnlockOnFlag(GungeonFlagsE.UNLOCKABLE_RAT_LAMEY);

            Item["bounty"]              .SetupUnlockOnFlag(GungeonFlagsE.UNLOCKABLE_BOSSRUSH_LAMEY);

            Item["dot_zip_carbine"]     .SetupUnlockOnFlag(GungeonFlagsE.UNLOCKABLE_LICH_LAMEY);
            Item["vegas_smg"]           .SetupUnlockOnFlag(GungeonFlagsE.UNLOCKABLE_LICH_LAMEY);
        }
    }

    public class OnDeathCharacterUnlockHandler : BraveBehaviour
    {
        public UnlockableType unlockType;

        public void Start()
        {
            if (healthHaver)
                healthHaver.OnDeath += HandleUnlocks;
        }

        public void HandleUnlocks(Vector2 dir)
        {
            if (!GameManager.HasInstance || GameManager.Instance.AllPlayers == null)
                return;

            foreach(var pl in GameManager.Instance.AllPlayers)
            {
                if(pl == null)
                    continue;

                var ch = pl.characterIdentity;

                var primaryFlag = GetFlagFromUnlockAndCharacter(unlockType, ch);

                if (primaryFlag != GungeonFlags.NONE)
                    GameStatsManager.Instance.SetFlag(primaryFlag, true);

                if(unlockType == UnlockableType.Dragun)
                {
                    if(GameManager.Instance.CurrentGameMode == GameManager.GameMode.BOSSRUSH)
                    {
                        var bossRushFlag = GetFlagFromUnlockAndCharacter(UnlockableType.BossRush, ch);

                        if (bossRushFlag != GungeonFlags.NONE)
                            GameStatsManager.Instance.SetFlag(bossRushFlag, true);
                    }

                    if (ch == PlayableCharactersE.Lamey || ch == PlayableCharactersE.Cosmonaut || ch == PlayableCharactersE.Ninja)
                    {
                        GameStatsManager.Instance.SetCharacterSpecificFlag(ch, CharacterSpecificGungeonFlags.KILLED_PAST, true);

                        if (pl.IsUsingAlternateCostume)
                            GameStatsManager.Instance.SetCharacterSpecificFlag(ch, CharacterSpecificGungeonFlags.KILLED_PAST_ALTERNATE_COSTUME, true);
                    }
                }
            }
        }

        public GungeonFlags GetFlagFromUnlockAndCharacter(UnlockableType unlock, PlayableCharacters ch)
        {
            if (ch == PlayableCharactersE.Lamey)
                return unlock switch
                {
                    UnlockableType.Rat =>       GungeonFlagsE.UNLOCKABLE_RAT_LAMEY,
                    UnlockableType.Dragun =>    GungeonFlagsE.UNLOCKABLE_DRAGUN_LAMEY,
                    UnlockableType.BossRush =>  GungeonFlagsE.UNLOCKABLE_BOSSRUSH_LAMEY,
                    UnlockableType.Lich =>      GungeonFlagsE.UNLOCKABLE_LICH_LAMEY,
                    _ => GungeonFlags.NONE
                };
            if (ch == PlayableCharactersE.Cosmonaut)
                return unlock switch
                {
                    UnlockableType.Rat =>       GungeonFlagsE.UNLOCKABLE_RAT_COSMONAUT,
                    UnlockableType.Dragun =>    GungeonFlagsE.UNLOCKABLE_DRAGUN_COSMONAUT,
                    UnlockableType.BossRush =>  GungeonFlagsE.UNLOCKABLE_BOSSRUSH_COSMONAUT,
                    UnlockableType.Lich =>      GungeonFlagsE.UNLOCKABLE_LICH_COSMONAUT,
                    _ => GungeonFlags.NONE
                };
            if (ch == PlayableCharactersE.Ninja)
                return unlock switch
                {
                    UnlockableType.Rat =>       GungeonFlagsE.UNLOCKABLE_RAT_NINJA,
                    UnlockableType.Dragun =>    GungeonFlagsE.UNLOCKABLE_DRAGUN_NINJA,
                    UnlockableType.BossRush =>  GungeonFlagsE.UNLOCKABLE_BOSSRUSH_NINJA,
                    UnlockableType.Lich =>      GungeonFlagsE.UNLOCKABLE_LICH_NINJA,
                    _ => GungeonFlags.NONE
                };

            return GungeonFlags.NONE;
        }

        public enum UnlockableType
        {
            None,
            Rat,
            Dragun,
            Lich,
            BossRush
        }
    }

    public static class UnlockTools
    {
        /// <summary>
        /// Setups a <see cref="DungeonPrerequisite"/> with the type of <see cref="DungeonPrerequisite.PrerequisiteType.FLAG"/>, flag of <paramref name="flag"/> and requiredFlagValue of 
        /// <paramref name="requiredFlagValue"/> and adds it to <paramref name="self"/>'s list of <see cref="DungeonPrerequisite"/>s
        /// </summary>
        /// <param name="self">The <see cref="PickupObject"/> to add the <see cref="DungeonPrerequisite"/> to</param>
        /// <param name="flag">The <see cref="GungeonFlags"/> to get the value from</param>
        /// <param name="requiredFlagValue">Value to compare <paramref name="flag"/>'s value to</param>
        /// <returns>The <see cref="DungeonPrerequisite"/> that was added to the list of <see cref="DungeonPrerequisite"/>s</returns>
        public static DungeonPrerequisite SetupUnlockOnFlag(this PickupObject self, GungeonFlags flag, bool requiredFlagValue = true)
        {
            if (self.encounterTrackable == null)
            {
                return null;
            }
            return self.encounterTrackable.SetupUnlockOnFlag(flag, requiredFlagValue);
        }

        /// <summary>
        /// Setups a <see cref="DungeonPrerequisite"/> with the type of <see cref="DungeonPrerequisite.PrerequisiteType.FLAG"/>, flag of <paramref name="flag"/> and requiredFlagValue of 
        /// <paramref name="requiredFlagValue"/> and adds it to <paramref name="self"/>'s list of <see cref="DungeonPrerequisite"/>s
        /// </summary>
        /// <param name="self">The <see cref="EncounterTrackable"/> to add the <see cref="DungeonPrerequisite"/> to</param>
        /// <param name="flag">The <see cref="GungeonFlags"/> to get the value from</param>
        /// <param name="requiredFlagValue">Value to compare <paramref name="flag"/>'s value to</param>
        /// <returns>The <see cref="DungeonPrerequisite"/> that was added to the list of <see cref="DungeonPrerequisite"/>s</returns>
        public static DungeonPrerequisite SetupUnlockOnFlag(this EncounterTrackable self, GungeonFlags flag, bool requiredFlagValue = true)
        {
            return self.AddPrerequisite(new DungeonPrerequisite
            {
                prerequisiteType = DungeonPrerequisite.PrerequisiteType.FLAG,
                saveFlagToCheck = flag,
                requireFlag = requiredFlagValue
            });
        }

        /// <summary>
        /// Adds <paramref name="prereq"/> to <paramref name="self"/>'s list of <see cref="DungeonPrerequisite"/>s
        /// </summary>
        /// <param name="self">The <see cref="PickupObject"/> to add <paramref name="prereq"/> to</param>
        /// <param name="prereq"><see cref="DungeonPrerequisite"/> to add</param>
        /// <returns><paramref name="prereq"/></returns>
        public static DungeonPrerequisite AddPrerequisite(this PickupObject self, DungeonPrerequisite prereq)
        {
            return self.encounterTrackable.AddPrerequisite(prereq);
        }

        /// <summary>
        /// Adds <paramref name="prereq"/> to <paramref name="self"/>'s list of <see cref="DungeonPrerequisite"/>s
        /// </summary>
        /// <param name="self">The <see cref="EncounterTrackable"/> to add <paramref name="prereq"/> to</param>
        /// <param name="prereq"><see cref="DungeonPrerequisite"/> to add</param>
        /// <returns><paramref name="prereq"/></returns>
        public static DungeonPrerequisite AddPrerequisite(this EncounterTrackable self, DungeonPrerequisite prereq)
        {
            if (!string.IsNullOrEmpty(self.ProxyEncounterGuid))
            {
                self.ProxyEncounterGuid = "";
            }
            if (self.prerequisites == null)
            {
                self.prerequisites = new DungeonPrerequisite[] { prereq };
            }
            else
            {
                DungeonPrerequisite[] prereqs = self.prerequisites;
                prereqs = prereqs.AddToArray(prereq);
                self.prerequisites = prereqs;
            }
            EncounterDatabaseEntry databaseEntry = EncounterDatabase.GetEntry(self.EncounterGuid);
            if (!string.IsNullOrEmpty(databaseEntry.ProxyEncounterGuid))
            {
                databaseEntry.ProxyEncounterGuid = "";
            }
            if (databaseEntry.prerequisites == null)
            {
                databaseEntry.prerequisites = new DungeonPrerequisite[] { prereq };
            }
            else
            {
                DungeonPrerequisite[] prereqs = databaseEntry.prerequisites;
                prereqs = prereqs.AddToArray(prereq);
                databaseEntry.prerequisites = prereqs;
            }
            return prereq;
        }
    }
}
