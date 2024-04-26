using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.Characters.Ninja.Items
{
    public class Tamatebako : PlayerItem
    {
        public static void Init()
        {
            var name = "Tamatebako";
            var shortdesc = "Opened, to his regret";
            var longdesc = "A jeweled box that once belonged to the Mistress of the Sea. Although simply holding it gives one great luck, a terrible hum can be heard from within, and you can feel your hands grow old and weathered from simply holding it, and time itself slip away into it. What would happen if you were to open it?";
            var item = EasyItemInit<Tamatebako>("tamatebako", name, shortdesc, longdesc, ItemQuality.B, null, null);
            item.SetCooldownType(CooldownType.Timed, 1f);
            item.AddPassiveStatModifier(PlayerStats.StatType.Coolness, 3f, StatModifier.ModifyMethod.ADDITIVE);
            item.timeMult = 0.9f;
        }

        public override void DoEffect(PlayerController user)
        {
            base.DoEffect(user);
            user.ownerlessStatModifiers.Add(StatModifier.Create(PlayerStats.StatType.Curse, StatModifier.ModifyMethod.ADDITIVE, 1f));
            user.stats.RecalculateStats(user, false, false);
            var timeslowTransform = user.transform.Find("TamatebakoTimeSlow");
            GameObject timeslowGo;
            if(timeslowTransform != null)
            {
                timeslowGo = timeslowTransform.gameObject;
            }
            else
            {
                timeslowGo = new GameObject("TamatebakoTimeSlow")
                {
                    transform =
                    {
                        parent = user.transform,
                        localPosition = Vector2.zero
                    }
                };
            }
            if (timeslowGo != null)
            {
                timeslowGo.GetOrAddComponent<PassiveTimeSlow>().timeMult *= timeMult;
            }
        }

        public float timeMult;
    }
}
