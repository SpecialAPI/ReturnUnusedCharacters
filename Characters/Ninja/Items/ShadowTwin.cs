using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.Characters.Ninja.Items
{
    public class ShadowTwin : PassiveItem
    {
        public static void Init()
        {
            var name = "Shadow Twin";
            var shortdesc = "Behold the soul";
            var longdesc = "A strange purple amulet of origins better left forgotten. The power inside it siphons from your very shadow; manifesting itself as a physical reflection of the darkness inside your very soul.";
            var item = EasyItemInit<ShadowTwin>("shadowtwin", name, shortdesc, longdesc, ItemQuality.A, null, null);
            item.shadowCloneObj = ShadowCloneObject.objectToSpawn;
            item.spawnDelay = 3f;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.OnEnteredCombat += Clone;
        }

        public void Clone()
        {
            StartCoroutine(CloneCR(Owner.CurrentRoom));
        }

        public IEnumerator CloneCR(RoomHandler r)
        {
            float ela = 0f;
            while(ela < spawnDelay)
            {
                if (this == null || !PickedUp || Owner == null || !Owner.IsInCombat || Owner.CurrentRoom != r)
                {
                    yield break;
                }
                ela += BraveTime.DeltaTime;
                yield return null;
            }
            var go = Instantiate(shadowCloneObj, Owner.specRigidbody.UnitCenter, Quaternion.identity);
            var s = go.GetComponent<tk2dBaseSprite>();
            if (s != null)
            {
                s.PlaceAtPositionByAnchor(Owner.specRigidbody.UnitCenter.ToVector3ZUp(s.transform.position.z), tk2dBaseSprite.Anchor.MiddleCenter);
                if (s.specRigidbody != null)
                {
                    s.specRigidbody.RegisterGhostCollisionException(Owner.specRigidbody);
                }
            }
            var sc = go.GetComponent<KageBunshinController>();
            if (sc)
            {
                sc.Duration = -1f;
                sc.InitializeOwner(Owner);
            }
            go.AddComponent<DestroyOnOwnerClearRoom>().Connect(Owner, r);
            yield break;
        }

        public override void DisableEffect(PlayerController player)
        {
            if(player != null)
            {
                player.OnEnteredCombat -= Clone;
            }
            base.DisableEffect(player);
        }

        public GameObject shadowCloneObj;
        public float spawnDelay;
    }
}
