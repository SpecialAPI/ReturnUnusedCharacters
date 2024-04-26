using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.Characters.Lamey.Items
{
    public class MagnifyingGlass
    {
        public static void Init()
        {
            string name = "Magnifying Glass";
            string shortdesc = "Magnifying Guon Stone";
            string longdesc = "Spins around the owner, magnifies bullets that pass through. Magnified bullets will have significantly increased size and slightly increased damage.\n\nLamey's trusty magnifying glass, she brings it" +
                " everywhere she goes";
            var item = EasyItemInit<PlayerOrbitalItem>("magnifyingglass", name, shortdesc, longdesc, PickupObject.ItemQuality.C, null, null);
            item.OrbitalPrefab = EasyGuonInit("MagnifyingGlassGuon", new(6, 6), 2.5f, 80f, 0, false, null, CollisionLayer.BulletBlocker);
            SpecialAssets.assets.Add(item.OrbitalPrefab.gameObject);
            item.OrbitalPrefab.specRigidbody.PixelColliders[0].IsTrigger = true;
            var magnificus = item.OrbitalPrefab.AddComponent<MagnifyPlayerBullets>();
            magnificus.scaleMultiplier = 2f;
            magnificus.damageMultiplier = 1.15f;
            magnificus.scaleMultiplierStealthed = 2.5f;
            magnificus.damageMultiplierStealthed = 3f;
            magnificus.stealthedAdditionalPierces = 1;
            var closerStealthed = item.OrbitalPrefab.AddComponent<ChangeOrbitSettingsOnStealth>();
            closerStealthed.stealthedOrbitRadius = 2f;
            closerStealthed.stealthedDegreesPerSecond = 80f;
            closerStealthed.stealthForgivenessTime = magnificus.stealthForgivenessTime = 0.5f;
        }

        public static GameObject fuckYouUnityImDoneWithYouIHateYou;
    }
}
