using ReturnUnusedCharacters.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ReturnUnusedCharacters.Characters.Lamey
{
    public class LameyCharacter
    {

        public static void Init()
        {
            //fix up lamey
            var lamey = (GameObject)BraveResources.Load("PlayerLamey");

            SpecialAssets.assets.Add(lamey);

            var lameyPlayer = lamey.GetComponent<PlayerController>();

            lameyPlayer.startingGunIds = new() { LameyGunId };
            lameyPlayer.startingAlternateGunIds = new() { ItemIds["altlamey_gun"] };
            lameyPlayer.startingActiveItemIds = new() { ItemIds["disguisehat"] };
            lameyPlayer.startingPassiveItemIds = new() { ItemIds["magnifyingglass"] };

            lameyPlayer.stats.BaseStatValues[(int)PlayerStats.StatType.AdditionalItemCapacity] = 1;

            lameyPlayer.uiPortraitName = AddToAtlas("lamey_portrait");
            lameyPlayer.characterIdentity = PlayableCharactersE.Lamey;
            lameyPlayer.characterAudioSpeechTag = "convict";

            var library = lameyPlayer.transform.Find("PlayerSprite").GetComponent<tk2dSpriteAnimator>().Library;
            var coll = lameyPlayer.transform.Find("PlayerSprite").GetComponent<tk2dBaseSprite>().Collection;

            //library.clips = library.clips.AddToArray(new(library.GetClipByName("idle_twohands")) { name = "select_idle" });
            //library.clips = library.clips.AddToArray(new(library.GetClipByName("item_get")) { name = "select_choose" });

            library.GetClipByName("jetpack_front_right").name = "jetpack_right";
            library.GetClipByName("jetpack_back_right").name = "jetpack_right_bw";
            library.GetClipByName("jetpack_front_right_hand").name = "jetpack_right_hand";

            lameyPlayer.AlternateCostumeLibrary.GetClipByName("jetpack_front_right").name = "jetpack_right";
            lameyPlayer.AlternateCostumeLibrary.GetClipByName("jetpack_back_right").name = "jetpack_right_bw";
            lameyPlayer.AlternateCostumeLibrary.GetClipByName("jetpack_front_right_hand").name = "jetpack_right_hand";

            var altCollection = lameyPlayer.AlternateCostumeLibrary.clips[0].frames[0].spriteCollection;
            var extraanim = Plugin.bundle.LoadAsset<GameObject>("RUCLameyExtraAnimation").GetComponent<tk2dSpriteAnimation>();
            var extracoll = Plugin.bundle.LoadAsset<GameObject>("RUCLameyExtraCollection").GetComponent<tk2dSpriteCollectionData>();

            var ftontTwohands = library.GetClipByName("idle_forward_twohands");
            
            for(int i = 0; i < ftontTwohands.frames.Length; i++)
            {
                ftontTwohands.frames[i].spriteId = ftontTwohands.frames[i].spriteCollection.GetSpriteIdByName($"lamey_idle_front_twohand_00{i + 1}");
            }

            library.clips                               = library.clips                             .Where(x => !extraanim.clips.Any(x2 => x2.name == x.name))      .Concat(extraanim.clips.Where(x => !x.name.Contains("lamey2"))).ToArray();
            lameyPlayer.AlternateCostumeLibrary.clips   = lameyPlayer.AlternateCostumeLibrary.clips .Where(x => !extraanim.clips.Any(x2 => x2.name == x.name))      .Concat(extraanim.clips.Where(x =>  x.name.Contains("lamey2"))).ToArray();

            tk2dSpriteAnimationClip AltClip(tk2dSpriteAnimationClip orig)
            {
                return new(orig)
                {
                    frames = orig.frames.Select(x =>
                    {
                        var f = new tk2dSpriteAnimationFrame();
                        f.CopyFrom(x);

                        f.spriteId = (f.spriteCollection = altCollection).GetSpriteIdByName(x.Sprite().name.Replace("lamey", "lamey2"), -1);

                        if(f.spriteId == -1)
                        {
                            f.spriteId = (f.spriteCollection = extracoll).GetSpriteIdByName(x.Sprite().name.Replace("lamey", "lamey2"), -1);
                        }

                        return f;
                    }).ToArray()
                };
            }

            lameyPlayer.AlternateCostumeLibrary.clips = lameyPlayer.AlternateCostumeLibrary.clips.AddRangeToArray(new tk2dSpriteAnimationClip[]
            {
                AltClip(library.GetClipByName("run_right_twohands")),
                AltClip(library.GetClipByName("idle_backward_twohands")),
                AltClip(library.GetClipByName("idle_backward_hand")),
                AltClip(library.GetClipByName("doorway")),
            });

            var libraries = new tk2dSpriteAnimation[] { library, lameyPlayer.AlternateCostumeLibrary };

            var dog = new List<string>()
            {
                "dodge",
                "dodge_bw",
                "dodge_left",
                "dodge_left_bw",
            };

            var walk = new List<string>()
            {
                "run_right",
                "run_right_bw",
                "run_down",
                "run_up",
                "run_right_hand",
                "run_down_hand",
                "run_up_hand",
                "run_down_twohands",
                "run_right_twohands",
                "run_up_twohands",
                "run_right_bw_twohands"
            };

            var jetpackOffsets = new Dictionary<string, Vector2>()
            {
                { "jetpack_down", new(0.6875f, 0.875f) },
                { "jetpack_down_hand", new(0.6875f, 0.875f) },
                { "jetpack_right", new(0.3125f, 0.6875f) },
                { "jetpack_right_hand", new(0.3125f, 0.6875f) },
                { "jetpack_right_bw", new(0.375f, 0.5625f) },
                { "jetpack_up", new(0.6875f, 0.625f) }
            };

            var animationEvents = new Dictionary<string, string>()
            {
                { "pitfall", "Play_Fall" },
                { "pitfall_down", "Play_Fall" },
                { "pitfall_return", "Play_Respawn" },
            };

            foreach (var lib in libraries)
            {
                foreach (var d in dog)
                {
                    var clippy = lib.GetClipByName(d);

                    if(clippy == null) Debug.Log($"clip {d} is null for library {lib.name}");

                    if (clippy.frames.Length > 0)
                    {
                        clippy.frames[0].triggerEvent = true;
                        clippy.frames[0].eventAudio = "Play_Leap";

                        var ground = clippy.frames.Where(x => x.groundedFrame && x.invulnerableFrame).FirstOrDefault();

                        if (ground != null)
                        {
                            ground.triggerEvent = true;
                            ground.eventAudio = "Play_Roll";
                        }
                    }
                }

                foreach (var w in walk)
                {
                    var clippy = lib.GetClipByName(w);

                    if (clippy == null) continue; //Debug.Log($"clip {w} is null for library {lib.name}");

                    if (clippy.frames.Length > 0)
                    {
                        clippy.frames[clippy.frames.Length - 1].triggerEvent = true;
                        clippy.frames[clippy.frames.Length - 1].eventAudio = "Play_FS";
                        clippy.frames[Mathf.RoundToInt(clippy.frames.Length / 2f) - 1].triggerEvent = true;
                        clippy.frames[Mathf.RoundToInt(clippy.frames.Length / 2f) - 1].eventAudio = "Play_FS";
                    }
                }

                foreach (var kvp in jetpackOffsets)
                {
                    var clippy = lib.GetClipByName(kvp.Key);

                    if (clippy == null) continue;//Debug.Log($"clip {kvp.Key} is null for library {lib.name}");

                    for (int i = 0; i < clippy.frames.Length; i++)
                    {
                        clippy.frames[i].spriteCollection.SetAttachPoints(clippy.frames[i].spriteId, new tk2dSpriteDefinition.AttachPoint[] { new() { name = "jetpack", position = kvp.Value, angle = 0f } });
                    }
                }

                foreach(var kvp in animationEvents)
                {
                    var clippy = lib.GetClipByName(kvp.Key);

                    if (clippy == null) Debug.Log($"clip {kvp.Key} is null for library {lib.name}");
                    var frame = clippy.frames[0];

                    frame.triggerEvent = true;
                    frame.eventAudio = kvp.Value;
                }

                lib.clips = lib.clips.AddToArray(new()
                {
                    fps = 6,
                    wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop,
                    name = "timefall",

                    frames = new int[]
                    {
                        1,
                        2,
                        3,
                        4,
                        5,
                        6
                    }.Select(x => new tk2dSpriteAnimationFrame()
                    {
                        spriteCollection = coll,
                        spriteId = coll.GetSpriteIdByName($"lamey_timefall_00{x}")
                    }).ToArray()
                });

                foreach (var clippy in lib.clips)
                {
                    foreach (var frame in clippy.frames)
                    {
                        var def = frame.Sprite();

                        def.ReplaceShader(PlayerController.DefaultShaderName);
                    }
                }
            }

            lamey.AddComponent<DefaultLootMods>().lootMods = new()
            {
                new()
                {
                    AssociatedPickupId = GreyMauserId,
                    DropRateMultiplier = 2f
                },
                new()
                {
                    AssociatedPickupId = ThePredatorId,
                    DropRateMultiplier = 2f
                },
                new()
                {
                    AssociatedPickupId = BoxId,
                    DropRateMultiplier = 2f
                },
                new()
                {
                    AssociatedPickupId = VorpalGunId,
                    DropRateMultiplier = 2f
                },
                new()
                {
                    AssociatedPickupId = SuperSpaceTurtleId,
                    DropRateMultiplier = 2f
                }
            };
            ETGMod.Databases.Strings.Core.Set($"#PLAYER_NAME_{PlayableCharactersE.Lamey.ToString().ToUpperInvariant()}", "Lamey");
            ETGMod.Databases.Strings.Core.Set($"#PLAYER_NICK_{PlayableCharactersE.Lamey.ToString().ToUpperInvariant()}", "detective");

            //add lamey to breach
            var lameyPos =      new Vector2(32f, 17.5f);
            var costumeOffset = new Vector2(-0.5f, -0.75f);

            var lameyBreach = lameyPlayer.AddToBreach("lameybreachflag", lameyPos, new(), new(8, 1, 10, 4), new(7, 1, 12, 19), new(-1, 0), "lameyoverheadpanel", "The Detective", "lamey", 5, "lamey_items");
            var lameyBreachAnim = lameyBreach.GetComponent<CharacterSelectIdleDoer>();

            lameyBreachAnim.phases = new CharacterSelectIdlePhase[]
            {
                new()
                {
                    inAnimation = "select_bored",

                    holdMin = 0f,
                    holdMax = 0f
                },
                new()
                {
                    inAnimation = "select_sneeze",

                    holdMin = 0f,
                    holdMax = 0f
                },
                new()
                {
                    inAnimation = "select_bounty_pull",

                    holdAnimation = "select_bounty_slap",
                    holdMin = 1.125f,
                    holdMax = 1.125f,

                    outAnimation = "select_bounty_hide"
                }
            };

            var swapper = Plugin.bundle.LoadAsset<GameObject>("LameyAltCostumeSwapper");

            var swap = swapper.AddComponent<CharacterCostumeSwapper>();
            swap.TargetCharacter = PlayableCharactersE.Lamey;

            swap.AlternateCostumeSprite = swapper.transform.Find("AltSprite").GetComponent<tk2dSprite>();
            swap.CostumeSprite = swapper.transform.Find("NormalSprite").GetComponent<tk2dSprite>();

            var hog = -0.5f;

            swap.AlternateCostumeSprite.CurrentSprite.ReplaceShader("Brave/LitTk2dCustomFalloffTiltedCutout");
            swap.AlternateCostumeSprite.IsPerpendicular = false;
            swap.AlternateCostumeSprite.HeightOffGround = hog;
            swap.AlternateCostumeSprite.UpdateZDepth();

            swap.CostumeSprite.CurrentSprite.ReplaceShader("Brave/LitTk2dCustomFalloffTiltedCutout");
            swap.CostumeSprite.IsPerpendicular = false;
            swap.CostumeSprite.HeightOffGround = hog;
            swap.CostumeSprite.UpdateZDepth();

            breachCharacters[swapper] = lameyPos + costumeOffset;

            //add punchout
            lameyPlayer.AddPunchoutSprites("ruc_lamey", "lamey_facecard");

            var offsetDict = new Dictionary<string, IntVector2>()
            {
                {"lamey_punch_block_001", new IntVector2(-9, 5)},
                {"lamey_punch_block_002", new IntVector2(-9, 5)},
                {"lamey_punch_block_hit_001", new IntVector2(-11, 5)},
                {"lamey_punch_dodge_left_001", new IntVector2(-20, 5)},
                {"lamey_punch_dodge_left_002", new IntVector2(-20, 5)},
                {"lamey_punch_dodge_right_001", new IntVector2(-7, 5)},
                {"lamey_punch_dodge_right_002", new IntVector2(-7, 5)},
                {"lamey_punch_duck_001", new IntVector2(-12, 5)},
                {"lamey_punch_duck_002", new IntVector2(-12, 5)},
                {"lamey_punch_exhaust_001", new IntVector2(-8, 5)},
                {"lamey_punch_exhaust_002", new IntVector2(-7, 5)},
                {"lamey_punch_exhaust_003", new IntVector2(-7, 5)},
                {"lamey_punch_hit_left_001", new IntVector2(-17, 0)},
                {"lamey_punch_hit_left_002", new IntVector2(-20, 0)},
                {"lamey_punch_hit_left_003", new IntVector2(-22, 0)},
                {"lamey_punch_hit_right_001", new IntVector2(-10, 0)},
                {"lamey_punch_hit_right_002", new IntVector2(-7, 0)},
                {"lamey_punch_hit_right_003", new IntVector2(-5, 0)},
                {"lamey_punch_idle_001", new IntVector2(-9, 5)},
                {"lamey_punch_idle_002", new IntVector2(-10, 5)},
                {"lamey_punch_idle_004", new IntVector2(-11, 5)},
                {"lamey_punch_knockout_001", new IntVector2(-9, 13)},
                {"lamey_punch_knockout_002", new IntVector2(-10, 14)},
                {"lamey_punch_knockout_003", new IntVector2(-10, 15)},
                {"lamey_punch_knockout_004", new IntVector2(-13, 20)},
                {"lamey_punch_knockout_005", new IntVector2(-15, 13)},
                {"lamey_punch_knockout_006", new IntVector2(-16, 8)},
                {"lamey_punch_knockout_007", new IntVector2(-17, 8)},
                {"lamey_punch_miss_left_002", new IntVector2(-6, 7)},
                {"lamey_punch_miss_left_003", new IntVector2(-6, 7)},
                {"lamey_punch_miss_right_002", new IntVector2(-11, 7)},
                {"lamey_punch_miss_right_003", new IntVector2(-10, 7)},
                {"lamey_punch_punch_blocked_left_002", new IntVector2(-9, 3)},
                {"lamey_punch_punch_blocked_left_003", new IntVector2(-8, 3)},
                {"lamey_punch_punch_blocked_right_002", new IntVector2(-14, 3)},
                {"lamey_punch_punch_blocked_right_003", new IntVector2(-13, 3)},
                {"lamey_punch_punch_left_001", new IntVector2(-8, 7)},
                {"lamey_punch_punch_left_002", new IntVector2(-8, 7)},
                {"lamey_punch_punch_left_003", new IntVector2(-12, 7)},
                {"lamey_punch_punch_right_001", new IntVector2(-12, 7)},
                {"lamey_punch_punch_right_002", new IntVector2(-12, 7)},
                {"lamey_punch_punch_right_003", new IntVector2(-11, 7)},
                {"lamey_punch_star_001", new IntVector2(-10, -9)},
                {"lamey_punch_star_002", new IntVector2(-10, -11)},
                {"lamey_punch_star_003", new IntVector2(-10, -11)},
                {"lamey_punch_star_004", new IntVector2(-12, 26)},
                {"lamey_punch_star_005", new IntVector2(-15, 33)},
                {"lamey_punch_star_006", new IntVector2(-14, 28)},
                {"lamey_punch_star_007", new IntVector2(-14, 19)},
                {"lamey_punch_star_008", new IntVector2(-11, 18)},
                {"lamey_punch_star_009", new IntVector2(-11, -8)},
                {"lamey_punch_win_001", new IntVector2(-9, 6)},
                {"lamey_punch_win_002", new IntVector2(-9, 6)},
                {"lamey_punch_win_003", new IntVector2(-9, 6)},
                {"lamey_punch_win_004", new IntVector2(-9, 6)}
            };

            foreach(var kvp in offsetDict)
            {
                punchoutColl.GetSpriteDefinition(kvp.Key).AddOffset(kvp.Value);
            }
        }
    }
}
