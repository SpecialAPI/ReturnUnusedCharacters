using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.Components
{
    public class PassiveTimeSlow : MonoBehaviour
    {
        public void Update()
        {
            BraveTime.SetTimeScaleMultiplier(timeMult, gameObject);
        }

        public void OnDestroy()
        {
            BraveTime.ClearMultiplier(gameObject);
        }

        public float timeMult = 1f;
    }
}
