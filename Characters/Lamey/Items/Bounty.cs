using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.Characters.Lamey.Items
{
    public class Bounty : PassiveItem
    {
        public static void Init()
        {
            var name = "Contrackter";
            var shortdesc = "Bounty-full";
            var longdesc = "An attachable bounty tool used by detectives and bounty hunters in Hegemotropolis. Capable of displaying detailed information about the target, down to their fingerprints, genetic makeup, and preferred pizza flavour. A good detective knows to appreciate whatever information is available. Of course, however, the most important is always the cash reward...\n\nRandomly marks an enemy for a bounty. Killing that enemy fast enough rewards you with extra casings.";
            var item = EasyItemInit<Bounty>("bounty", name, shortdesc, longdesc, ItemQuality.B, null, null);
            item.TimeToKillBounty = 15f;
            item.bountyReward = 5;
            item.BountyVFX = Plugin.bundle.LoadAsset<GameObject>("contrackskull");
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.OnEnteredCombat += ApplyBounty;
        }

        public void ApplyBounty()
        {
            if(Owner.CurrentRoom != null)
            {
                var enemies = Owner.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear);
                if (enemies != null)
                {
                    var valids = enemies.FindAll(x => IsValidEnemy(x));
                    if(valids.Count > 0)
                    {
                        var validenemy = BraveUtility.RandomElement(valids);
                        var target = validenemy.AddComponent<BountyTarget>();
                        target.vfx = validenemy.PlayEffectOnActor(BountyVFX, (!validenemy.sprite ? Vector2.up : Vector2.up * (validenemy.sprite.WorldTopCenter.y - validenemy.sprite.WorldBottomCenter.y)) + Vector2.up, true, true);
                        SpriteOutlineManager.AddOutlineToSprite(target.vfx.GetComponent<tk2dBaseSprite>(), Color.black);
                        target.currencyToDrop = bountyReward;
                        target.KillTime = TimeToKillBounty;
                    }
                }
            }
        }

        public bool IsValidEnemy(AIActor testEnemy)
        {
            if (!testEnemy || testEnemy.IsHarmlessEnemy)
            {
                return false;
            }
            if ((bool)testEnemy.healthHaver && (testEnemy.healthHaver.PreventAllDamage || testEnemy.healthHaver.IsBoss))
            {
                return false;
            }
            if ((bool)testEnemy.GetComponent<ExplodeOnDeath>() && !testEnemy.IsSignatureEnemy)
            {
                return false;
            }
            return true;
        }

        public override void DisableEffect(PlayerController player)
        {
            player.OnEnteredCombat -= ApplyBounty;
            base.DisableEffect(player);
        }

        public float TimeToKillBounty;
        public int bountyReward;
        public GameObject BountyVFX;
    }
}
