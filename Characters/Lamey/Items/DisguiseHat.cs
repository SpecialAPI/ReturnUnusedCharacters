using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.Characters.Lamey.Items
{
    public class DisguiseHat : PlayerItem
    {
        public static void Init()
        {
            var name = "Disguise Kit";
            var shortdesc = "Temporary Disguise";
            var longdesc = "Grants stealth, can't be used to steal from shops.\n\nThis disguise kit allows for easy and quick disguise, but the shopkeepers won't be tricked that easily";
            var item = EasyItemInit<DisguiseHat>("disguisehat", name, shortdesc, longdesc, ItemQuality.D, null, null);
            item.SetCooldownType(CooldownType.PerRoom, 1f);
        }

        public override void DoEffect(PlayerController user)
        {
            base.DoEffect(user);
            user.StealthPlayer("spapi_disguise", false, false, SmokeBombObject.poofVfx, "Play_ENM_wizardred_appear_01", true);
        }

        public override bool CanBeUsed(PlayerController user)
        {
            return !user.IsStealthed;
        }
    }
}
