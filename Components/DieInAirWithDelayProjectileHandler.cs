using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.Components
{
    public class DieInAirWithDelayProjectileHandler : BraveBehaviour
    {
        public void Update()
        {
            if(projectile && projectile.ElapsedTime >= delay)
            {
                projectile.DieInAir();
            }
        }

        public float delay;
    }
}
