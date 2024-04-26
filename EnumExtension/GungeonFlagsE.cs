using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.EnumExtension
{
    [EnumExtension(typeof(GungeonFlags))]
    public static class GungeonFlagsE
    {
        public static GungeonFlags UNLOCKABLE_DRAGUN_LAMEY;
        public static GungeonFlags UNLOCKABLE_DRAGUN_COSMONAUT;
        public static GungeonFlags UNLOCKABLE_DRAGUN_NINJA;

        public static GungeonFlags UNLOCKABLE_RAT_LAMEY;
        public static GungeonFlags UNLOCKABLE_RAT_COSMONAUT;
        public static GungeonFlags UNLOCKABLE_RAT_NINJA;

        public static GungeonFlags UNLOCKABLE_BOSSRUSH_LAMEY;
        public static GungeonFlags UNLOCKABLE_BOSSRUSH_COSMONAUT;
        public static GungeonFlags UNLOCKABLE_BOSSRUSH_NINJA;

        public static GungeonFlags UNLOCKABLE_LICH_LAMEY;
        public static GungeonFlags UNLOCKABLE_LICH_COSMONAUT;
        public static GungeonFlags UNLOCKABLE_LICH_NINJA;
    }
}
