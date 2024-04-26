using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.Components
{
    public class RadPoisoningSynergyProcessor : GunBehaviour
    {
        public override void PostProcessProjectile(Projectile projectile)
        {
            base.PostProcessProjectile(projectile);

            if(PlayerOwner && PlayerOwner.HasActiveBonusSynergy(CustomSynergyTypeE.RAD_POISONING) && Random.value < applyChance)
            {
                projectile.statusEffectsToApply.Add(debuff);
            }
        }

        public AIActorDebuffEffect debuff;
        public float applyChance = 0.35f;
    }
}
