using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.Components
{
    public class BountyTarget : BraveBehaviour
    {
        public void Start()
        {
            if(healthHaver != null && aiActor != null)
            {
                disableCR = StartCoroutine(DisableAfterTime());
                healthHaver.OnPreDeath += OnDeath;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (healthHaver != null)
            {
                healthHaver.OnPreDeath -= OnDeath;
            }
        }

        public void OnDeath(Vector2 d)
        {
            AkSoundEngine.PostEvent("Play_UI_challenge_clear_01", gameObject);
            if (vfx)
            {
                vfx.transform.parent = null;
                vfx.GetComponent<tk2dSpriteAnimator>().Play("contrack_skull_defeat");
                Destroy(vfx, 1.111f);
            }
            if(disableCR != null)
            {
                StopCoroutine(disableCR);
            }
            if (aiActor)
            {
                LootEngine.SpawnCurrency(aiActor.CenterPosition, currencyToDrop, false);
            }
            Destroy(this);
        }

        public IEnumerator DisableAfterTime()
        {
            yield return new WaitForSeconds(KillTime);
            if (vfx)
            {
                vfx.transform.parent = null;
                vfx.GetComponent<tk2dSpriteAnimator>().Play("contrack_skull_lose");
                Destroy(vfx, 1.333f);
            }
            AkSoundEngine.PostEvent("Play_OBJ_metronome_fail_01", gameObject);
            Destroy(this);
            yield break;
        }

        public GameObject vfx;
        public float KillTime;
        public int currencyToDrop;
        public Coroutine disableCR;
    }
}
