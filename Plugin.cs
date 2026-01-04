global using HarmonyLib;
global using BepInEx;
global using UnityEngine;
global using Object = UnityEngine.Object;
global using Random = UnityEngine.Random;
global using Debug = UnityEngine.Debug;
global using Gungeon;
global using ModifyMethod = StatModifier.ModifyMethod;
global using Dungeonator;
global using System.Collections;
global using HutongGames.PlayMaker;
global using HutongGames.PlayMaker.Actions;
global using ReturnUnusedCharacters.Tools;
global using ReturnUnusedCharacters.Components;
global using ReturnUnusedCharacters.API;
global using ReturnUnusedCharacters.API.ItemAPI;
global using ReturnUnusedCharacters.API.SoundAPI;
global using ReturnUnusedCharacters.Characters;
global using ReturnUnusedCharacters.Characters.Cosmonaut;
global using ReturnUnusedCharacters.Characters.Cosmonaut.Items;
global using ReturnUnusedCharacters.Characters.Lamey;
global using ReturnUnusedCharacters.Characters.Lamey.Items;
global using ReturnUnusedCharacters.Characters.Ninja;
global using ReturnUnusedCharacters.Characters.Ninja.Items;
global using ReturnUnusedCharacters.EnumExtension;
global using Mono.Cecil.Cil;
global using OpCodes = Mono.Cecil.Cil.OpCodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using SGUI;

namespace ReturnUnusedCharacters
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "spapi.etg.cutcharactersreborn";
        public const string NAME = "Cut Characters Reborn";
        public const string VERSION = "1.1.2";
        public static AssetBundle bundle;
        public static GameUIRoot UIRootPrefab;

        public void Awake()
        {
            var asm = Assembly.GetExecutingAssembly();
            foreach (var type in asm.GetTypes())
            {
                var custom = type.GetCustomAttributes(false);
                if (custom != null)
                {
                    var extension = custom.OfType<EnumExtensionAttribute>().FirstOrDefault();
                    if (extension != null && extension.type != null && extension.type.IsEnum)
                    {
                        foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                        {
                            f.SetValue(null, ETGModCompatibility.ExtendEnum(GUID, f.Name, extension.type));
                        }
                    }
                }
            }

            var platformFolder = Application.platform switch
            {
                RuntimePlatform.LinuxPlayer or RuntimePlatform.LinuxEditor => "Linux",
                RuntimePlatform.OSXPlayer or RuntimePlatform.OSXEditor => "MacOS",
                _ => "Windows"
            };

            using (var strem = asm.GetManifestResourceStream($"ReturnUnusedCharacters.ReturnUnusedCharacters.Assets.AssetBundles.{platformFolder}.unusedchar_bundle"))
            {
                bundle = AssetBundle.LoadFromStream(strem);
            }

            SoundManager.Init();
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
            UIRootPrefab = LoadHelper.LoadAssetFromAnywhere<GameObject>("UI Root").GetComponent<GameUIRoot>();
            new Harmony(GUID).PatchAll();
        }

        public void GMStart(GameManager gm)
        {
            //punchout stuff
            punchoutPlayer = EnemyDatabase.GetOrLoadByGuid("4d164ba3f62648809a4a82c90fc22cae").GetComponent<MetalGearRatDeathController>().PunchoutMinigamePrefab.GetComponentInChildren<PunchoutPlayerController>();

            punchoutAnim = punchoutPlayer.GetComponent<tk2dSpriteAnimator>().Library;
            punchoutFacecardAtlas = punchoutPlayer.PlayerUiSprite.Atlas;

            punchoutColl = bundle.LoadAsset<GameObject>("RUCPunchoutCollection").GetComponent<tk2dSpriteCollectionData>();
            var poanim = bundle.LoadAsset<GameObject>("RUCPunchoutAnimation").GetComponent<tk2dSpriteAnimation>();

            punchoutAnim.clips = punchoutAnim.clips.AddRangeToArray(poanim.clips);

            InitItemBuilder();

            //lamey stuff
            DisguiseHat.Init();
            MagnifyingGlass.Init();
            Bounty.Init();
            CyberPistol.Init();
            CyberSMG.Init();
            VegasSMG.Init();
            DotZipCarbine.Init();

            AltLameyGun.Init();

            LameyCharacter.Init();

            //cosmo stuff
            //Agitprop.Init();

            //ninja stuff
            //ShadowTwin.Init();
            //Tamatebako.Init();

            //other stuff
            Unlocks.Init();
            Synergies.Init();

            //add console commands
            ConsoleCommands();

            //load message
            LoadMessage();
        }

        public static void ConsoleCommands()
        {
            ETGModConsole.Commands.AddGroup("ccr");

            var gr = ETGModConsole.Commands.GetGroup("ccr");

            gr.AddUnit("unlock_everything", x =>
            {
                foreach(var fl in Unlocks.UnlockFlags)
                {
                    GameStatsManager.Instance.SetFlag(fl, true);
                }

                ETGModConsole.Log("All items in Cut Characters Reborn are now unlocked.");
            });

            gr.AddUnit("lock_everything", x =>
            {
                foreach (var fl in Unlocks.UnlockFlags)
                {
                    GameStatsManager.Instance.SetFlag(fl, false);
                }

                ETGModConsole.Log("All items in Cut Characters Reborn are now locked.");
            });

            var descs = ETGModConsole.CommandDescriptions;

            descs["ccr unlock_everything"] = "Unlocks every unlockable item in Cut Characters Reborn";
            descs["ccr lock_everything"] = "Locks every unlockable item in Cut Characters Reborn";
        }

        public static void LoadMessage()
        {
            var txt = "Cut Characters Reborn successfully loaded.";

            var groupHeight = 48;
            var group = new SGroup() { Size = new Vector2(20000, groupHeight), AutoLayoutPadding = 0, Background = Color.clear, AutoLayout = x => x.AutoLayoutHorizontal };
            var scale = 1.8f;

            var color1 = new Color32(124, 200, 74, 255);
            var color2 = new Color32(215, 232, 73, 255);

            var grad = new Gradient()
            {
                colorKeys = new GradientColorKey[]
                {
                    new(color1, 0f),
                    new(color2, 0.5f),
                    new(color1, 1f)
                }
            };

            for (int i = 0; i < txt.Length; i++)
            {
                var c = txt[i];

                if (c == ' ')
                {
                    group.Children.Add(new SRect(Color.clear) { Size = Vector2.one * 10 });
                }
                else
                {
                    group.Children.Add(new SLabel(c.ToString()) { With = { new GradientThingy(grad, 2f, 0.1f * i) } });
                }
            }

            group.Children.Add(new SRect(Color.clear) { Size = Vector2.one * 10 });

            var cosmoReleased = false;
            var ninjaReleased = false;

            var lameyTex = bundle.LoadAsset<Texture2D>("lamey");
            var cosmoTex = bundle.LoadAsset<Texture2D>($"cosmo{(cosmoReleased ? "" : "_hidden")}");
            var ninjaTex = bundle.LoadAsset<Texture2D>($"ninja{(ninjaReleased ? "" : "_hidden")}");

            var texes = new Texture2D[] { lameyTex, cosmoTex, ninjaTex };

            for(int i = 0; i < texes.Length; i++)
            {
                var tex = texes[i];

                group.Children.Add(new SLabel() { Icon = tex, IconScale = Vector2.one * scale, With = { new MovementThingy(1f, 0.2f * i, 1.5f) } });
                group.Children.Add(new SRect(Color.clear) { Size = new((tex.width + 4) * scale, groupHeight) });
            }

            ETGModConsole.Logger.LogMessage(txt);
            ETGModConsole.Instance.GUI[0].Children.Add(group);
        }
    }

    public class GradientThingy(Gradient gradient, float mult, float offs) : SModifier
    {
        public Gradient gradient = gradient;
        public float offs = offs;
        public float mult = mult;

        public override void Update()
        {
            Elem.Foreground = gradient.Evaluate((Time.realtimeSinceStartup * mult + offs) % 1f);
        }
    }

    public class MovementThingy(float mult, float offs, float amplitude) : SModifier
    {
        public float offs = offs;
        public float amplitude = amplitude;
        public float mult = mult;

        public override void Update()
        {
            Elem.Position = Elem.Position.WithY(Mathf.Sin((Time.realtimeSinceStartup * mult + offs) * Mathf.PI * 2) * amplitude);
        }
    }
}
