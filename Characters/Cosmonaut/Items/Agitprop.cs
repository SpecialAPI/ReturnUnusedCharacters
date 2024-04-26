using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.Characters.Cosmonaut.Items
{
    public class Agitprop : PassiveItem
    {
        public static void Init()
        {
            var name = "Agitprop";
            var shortdesc = "Chains to lose";
            var longdesc = "A piece of revolutionary propaganda from ages long past. Even today, the inflaming slogans do not fail to incite a righteous rage in some hearts.\n\nAble to grant bursts of power to the most destructive.";
            var item = EasyItemInit<Agitprop>("agitprop", name, shortdesc, longdesc, ItemQuality.C);
            item.rageDuration = 4f;
            item.rageChanceOnBreak = 0.1f;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
			player.Ext().OnMinorBreakableBreak += RageOnMinorBreak;
			player.Ext().OnMajorBreakableBreak += RageOnMajorBreak;
		}

		public void RageOnMinorBreak(MinorBreakable m, PlayerController p)
        {
			if(Random.value <= rageChanceOnBreak)
            {
                p.Ext().Rage(rageDuration);
            }
        }

		public void RageOnMajorBreak(MajorBreakable m, Vector2 v, PlayerController p)
		{
			if (Random.value <= rageChanceOnBreak)
			{
				p.Ext().Rage(rageDuration);
			}
		}

        public override void DisableEffect(PlayerController player)
        {
			if(player != null)
			{
				player.Ext().OnMinorBreakableBreak -= RageOnMinorBreak;
				player.Ext().OnMajorBreakableBreak -= RageOnMajorBreak;
			}
            base.DisableEffect(player);
        }

		public float rageDuration;
        public float rageChanceOnBreak;
	}
}
