global using static ReturnUnusedCharacters.Tools.Toolbox;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static UnityEngine.UI.CanvasScaler;

namespace ReturnUnusedCharacters.Tools
{
    [HarmonyPatch]
    public static class Toolbox
    {
        public static Gun AsGun(this PickupObject po) => po as Gun;
        public static PlayerItem AsActive(this PickupObject po) => po as PlayerItem;
        public static PassiveItem AsPassive(this PickupObject po) => po as PassiveItem;

        public static T As<T>(this object o) => o is T t ? t : default;

        public static T AddComponent<T>(this Component self) where T : Component => self.gameObject.AddComponent<T>();
        public static T GetOrAddComponent<T>(this Component self) where T : Component => self.gameObject.GetOrAddComponent<T>();

        public static string AddToAtlas(string textureName, string overrideName = null) => AddToAtlas(Plugin.bundle.LoadAsset<Texture2D>(textureName), out _, overrideName);
        public static string AddToAtlas(string textureName, out dfAtlas.ItemInfo info, string overrideName = null) => AddToAtlas(Plugin.bundle.LoadAsset<Texture2D>(textureName), out info, overrideName);

        public static tk2dSpriteDefinition Sprite(this tk2dSpriteAnimationFrame frame) => frame.spriteCollection.spriteDefinitions[frame.spriteId];

        public static Components.PlayerControllerExt Ext(this PlayerController pc)
        {
            if(pc != null)
            {
                return pc.GetOrAddComponent<Components.PlayerControllerExt>();
            }
            return null;
        }

        public static void StealthPlayer(this PlayerController player, string reason, bool allowStealing = true, bool disableEnemyCollision = true, GameObject poof = null, string stealthUnstealthSound = null,
            bool enableStealthShader = true)
        {
            void BreakStealthOnSteal(PlayerController arg1, ShopItemController arg2)
            {
                BreakStealth(arg1);
            }

            void BreakStealth(PlayerController obj)
            {
                if (poof != null)
                {
                    obj.PlayEffectOnActor(poof, Vector3.zero, false, true, false);
                }
                obj.OnDidUnstealthyAction -= BreakStealth;
                if (allowStealing)
                {
                    obj.SetCapableOfStealing(false, reason, null);
                    obj.OnItemStolen -= BreakStealthOnSteal;
                }
                if (disableEnemyCollision)
                {
                    obj.specRigidbody.RemoveCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox, CollisionLayer.EnemyCollider));
                }
                if (enableStealthShader)
                {
                    obj.ChangeSpecialShaderFlag(1, 0f);
                }
                obj.SetIsStealthed(false, reason);
                if (!string.IsNullOrEmpty(stealthUnstealthSound))
                {
                    AkSoundEngine.PostEvent(stealthUnstealthSound, obj.gameObject);
                }
            }

            if (enableStealthShader)
            {
                player.ChangeSpecialShaderFlag(1, 1f);
            }
            player.SetIsStealthed(true, reason);
            if (disableEnemyCollision)
            {
                player.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox, CollisionLayer.EnemyCollider));
            }
            if (poof != null)
            {
                player.PlayEffectOnActor(poof, Vector3.zero, false, true, false);
            }
            if (allowStealing)
            {
                player.SetCapableOfStealing(true, reason, null);
                player.OnItemStolen += BreakStealthOnSteal;
            }
            player.OnDidUnstealthyAction += BreakStealth;
            if (!string.IsNullOrEmpty(stealthUnstealthSound))
            {
                AkSoundEngine.PostEvent(stealthUnstealthSound, player.gameObject);
            }
        }


        public static void ReplaceShader(this tk2dSpriteDefinition def, string shaderName)
        {
            if(def != null)
            {
                if(def.material != null)
                {
                    def.material.shader = ShaderCache.Acquire(shaderName);
                }
                if(def.materialInst != null)
                {
                    def.materialInst.shader = ShaderCache.Acquire(shaderName);
                }
            }
        }


        public static string AddToAtlas(Texture2D tex, string overrideName = null)
        {
            return AddToAtlas(tex, out _, overrideName);
        }

        public static string AddToAtlas(Texture2D tex, out dfAtlas.ItemInfo info, string overrideName = null)
        {
            var name = overrideName ?? $"ccr_{tex.name}";
            info = Plugin.UIRootPrefab.Manager.DefaultAtlas.AddNewItemToAtlas(tex, name);
            return name;
        }

        public static dfAtlas.ItemInfo AddNewItemToAtlas(this dfAtlas atlas, Texture2D tex, string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = tex.name;
            }
            if (atlas[name] != null)
            {
                return atlas[name];
            }
            dfAtlas.ItemInfo item = new dfAtlas.ItemInfo
            {
                border = new RectOffset(),
                deleted = false,
                name = name,
                region = atlas.FindFirstValidEmptySpace(new IntVector2(tex.width, tex.height)),
                rotated = false,
                sizeInPixels = new Vector2(tex.width, tex.height),
                texture = tex,
                textureGUID = name
            };
            int startPointX = Mathf.RoundToInt(item.region.x * atlas.Texture.width);
            int startPointY = Mathf.RoundToInt(item.region.y * atlas.Texture.height);
            for (int x = startPointX; x < Mathf.RoundToInt(item.region.xMax * atlas.Texture.width); x++)
            {
                for (int y = startPointY; y < Mathf.RoundToInt(item.region.yMax * atlas.Texture.height); y++)
                {
                    atlas.Texture.SetPixel(x, y, tex.GetPixel(x - startPointX, y - startPointY));
                }
            }
            atlas.Texture.Apply();
            atlas.AddItem(item);
            return item;
        }

        public static Rect FindFirstValidEmptySpace(this dfAtlas atlas, IntVector2 pixelScale)
        {
            if (atlas == null || atlas.Texture == null || !atlas.Texture.IsReadable())
            {
                return new Rect(0f, 0f, 0f, 0f);
            }
            Vector2Int point = new Vector2Int(0, 0);
            int pointIndex = -1;
            List<RectInt> rects = atlas.GetPixelRegions();
            while (true)
            {
                bool shouldContinue = false;
                foreach (RectInt rint in rects)
                {
                    if (rint.Overlaps(new RectInt(point, pixelScale.ToVector2Int())))
                    {
                        shouldContinue = true;
                        pointIndex++;
                        if (pointIndex >= rects.Count)
                        {
                            return new Rect(0f, 0f, 0f, 0f);
                        }
                        point = rects[pointIndex].max + Vector2Int.one;
                        if (point.x > atlas.Texture.width || point.y > atlas.Texture.height)
                        {
                            atlas.ResizeAtlas(new IntVector2(atlas.Texture.width * 2, atlas.Texture.height * 2));
                        }
                        break;
                    }
                    bool shouldBreak = false;
                    foreach (RectInt rint2 in rects)
                    {
                        RectInt currentRect = new RectInt(point, pixelScale.ToVector2Int());
                        if (rint2.x < currentRect.x || rint2.y < currentRect.y)
                        {
                            continue;
                        }
                        else
                        {
                            if (currentRect.Overlaps(rint2))
                            {
                                shouldContinue = true;
                                shouldBreak = true;
                                pointIndex++;
                                if (pointIndex >= rects.Count)
                                {
                                    return new Rect(0f, 0f, 0f, 0f);
                                }
                                point = rects[pointIndex].max + Vector2Int.one;
                                if (point.x > atlas.Texture.width || point.y > atlas.Texture.height)
                                {
                                    atlas.ResizeAtlas(new IntVector2(atlas.Texture.width * 2, atlas.Texture.height * 2));
                                }
                                break;
                            }
                        }
                    }
                    if (shouldBreak)
                    {
                        break;
                    }
                }
                if (shouldContinue)
                {
                    continue;
                }
                RectInt currentRect2 = new RectInt(point, pixelScale.ToVector2Int());
                if (currentRect2.xMax > atlas.Texture.width || currentRect2.yMax > atlas.Texture.height)
                {
                    atlas.ResizeAtlas(new IntVector2(atlas.Texture.width * 2, atlas.Texture.height * 2));
                }
                break;
            }
            RectInt currentRect3 = new RectInt(point, pixelScale.ToVector2Int());
            Rect rect = new Rect((float)currentRect3.x / atlas.Texture.width, (float)currentRect3.y / atlas.Texture.height, (float)currentRect3.width / atlas.Texture.width, (float)currentRect3.height / atlas.Texture.height);
            return rect;
        }

        public static List<RectInt> GetPixelRegions(this dfAtlas atlas)
        {
            return atlas.Items.ConvertAll(item => 
                new RectInt(Mathf.RoundToInt(item.region.x * atlas.Texture.width), Mathf.RoundToInt(item.region.y * atlas.Texture.height), Mathf.RoundToInt(item.region.width * atlas.Texture.width), Mathf.RoundToInt(item.region.height * atlas.Texture.height))
            );
        }

        public static Vector2Int ToVector2Int(this IntVector2 vector)
        {
            return new Vector2Int(vector.x, vector.y);
        }

		public static bool Overlaps(this RectInt self, RectInt other)
        {
            return other.xMax > self.xMin && other.xMin < self.xMax && other.yMax > self.yMin && other.yMin < self.yMax;
        }

        public static void ChangeMaterials(this tk2dSpriteDefinition def, ChangeMaterialsAction change)
        {
            if(def.material != null)
            {
                change(ref def.material);
            }

            if(def.materialInst != null)
            {
                change(ref def.materialInst);
            }
        }

        public delegate void ChangeMaterialsAction(ref Material mat);

        public static void ResizeAtlas(this dfAtlas atlas, IntVector2 newDimensions)
        {
            Texture2D tex = atlas.Texture;
            if (!tex.IsReadable())
            {
                return;
            }
            if (tex.width == newDimensions.x && tex.height == newDimensions.y)
            {
                return;
            }
            foreach (dfAtlas.ItemInfo item in atlas.Items)
            {
                if (item.region != null)
                {
                    item.region.x = (item.region.x * tex.width) / newDimensions.x;
                    item.region.y = (item.region.y * tex.height) / newDimensions.y;
                    item.region.width = (item.region.width * tex.width) / newDimensions.x;
                    item.region.height = (item.region.height * tex.height) / newDimensions.y;
                }
            }
            tex.ResizeBetter(newDimensions.x, newDimensions.y);
            atlas.Material.SetTexture("_MainTex", tex);
        }

        public static Projectile OwnedShootProjectile(Projectile proj, Vector2 position, float angle, GameActor owner)
        {
            var obj = SpawnManager.SpawnProjectile(proj.gameObject, position, Quaternion.Euler(0f, 0f, angle), true);
            var bullet = obj.GetComponent<Projectile>();
            if (bullet != null)
            {
                bullet.Owner = owner;
                bullet.Shooter = owner.specRigidbody;
            }
            return bullet;
        }

        public static Vector2 GetRelativeAim(this PlayerController player)
        {
            var instanceForPlayer = BraveInput.GetInstanceForPlayer(player.PlayerIDX);

            if (instanceForPlayer != null)
            {
                if (instanceForPlayer.IsKeyboardAndMouse(false))
                    return player.unadjustedAimPoint.XY() - player.CenterPosition;
                else
                {
                    if (instanceForPlayer.ActiveActions == null)
                        return Vector2.zero;

                    return instanceForPlayer.ActiveActions.Aim.Vector;
                }
            }

            return Vector2.zero;
        }

        public static T EnumeratorGetField<T>(this object obj, string name) => (T)obj.GetType().EnumeratorField(name).GetValue(obj);
        public static FieldInfo EnumeratorField(this MethodBase method, string name) => method.DeclaringType.EnumeratorField(name);
        public static FieldInfo EnumeratorField(this Type tp, string name) => tp.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).First(x => x != null && x.Name != null && (x.Name.Contains($"<{name}>__") || x.Name == name));

        public static bool ResizeBetter(this Texture2D tex, int width, int height)
        {
            if (tex.IsReadable())
            {
                Color[][] pixels = new Color[Math.Min(tex.width, width)][];
                for (int x = 0; x < Math.Min(tex.width, width); x++)
                {
                    for (int y = 0; y < Math.Min(tex.height, height); y++)
                    {
                        if (pixels[x] == null)
                        {
                            pixels[x] = new Color[Math.Min(tex.height, height)];
                        }
                        pixels[x][y] = tex.GetPixel(x, y);
                    }
                }
                bool result = tex.Resize(width, height);
                for (int x = 0; x < tex.width; x++)
                {
                    for (int y = 0; y < tex.height; y++)
                    {
                        bool isInOrigTex = false;
                        if (x < pixels.Length)
                        {
                            if (y < pixels[x].Length)
                            {
                                isInOrigTex = true;
                                tex.SetPixel(x, y, pixels[x][y]);
                            }
                        }
                        if (!isInOrigTex)
                        {
                            tex.SetPixel(x, y, Color.clear);
                        }
                    }
                }
                tex.Apply();
                return result;
            }
            return tex.Resize(width, height);
        }

        public static VariableDefinition DeclareLocal<T>(this ILContext ctx)
        {
            var loc = new VariableDefinition(ctx.Import(typeof(T)));
            ctx.Body.Variables.Add(loc);

            return loc;
        }

        public static VariableDefinition DeclareLocal<T>(this ILCursor curs)
        {
            return curs.Context.DeclareLocal<T>();
        }

        public static bool Calls(this Instruction instr, MethodBase mthd)
        {
            return instr.MatchCallOrCallvirt(mthd);
        }

        public static bool JumpToNext(this ILCursor curs, Func<Instruction, bool> predicate, int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                if (!curs.TryGotoNext(MoveType.After, predicate))
                    return false;
            }

            return true;
        }

        public static bool JumpBeforeNext(this ILCursor curs, Func<Instruction, bool> predicate, int times = 1)
        {
            for (int i = 0; i < times - 1; i++)
            {
                if (!curs.TryGotoNext(MoveType.After, predicate))
                    return false;
            }

            if (curs.TryGotoNext(MoveType.Before, predicate))
                return true;

            return false;
        }

        public static IEnumerable MatchBefore(this ILCursor curs, Func<Instruction, bool> predicate)
        {
            for (; curs.JumpBeforeNext(predicate); curs.JumpToNext(predicate))
            {
                yield return null;
            }
        }

        public static IEnumerable MatchAfter(this ILCursor curs, Func<Instruction, bool> predicate)
        {
            while (curs.JumpToNext(predicate))
            {
                yield return null;
            }
        }

        public static void EmitStaticDelegate(this ILCursor crs, Delegate call)
        {
            if (call.GetInvocationList().Length != 1 || call.Target != null)
                throw new ArgumentException("Delegate is either not static or has additional invocations");

            crs.Emit(OpCodes.Call, call.Method);
        }

        public static FoyerCharacterSelectFlag AddToBreach(this PlayerController player, string assetPath, Vector3 breachPosition, List<DungeonPrerequisite> unlockPrereqs, IntRect obstacle, IntRect bulletblocker,
            IntVector2 additionalShadowOffset, string overheadElementPath, string breachName, string facecardAnimationPrefix, float facecardIdleFps, string itemsTexture, string description1Text = null, string description2Text = null,
            string overrideChangeCharacterString = null, string overrideYesResponse = null, string overrideNoResponse = null)
        {
            var go = Plugin.bundle.LoadAsset<GameObject>(assetPath);
            if (go == null)
            {
                return null;
            }
            SpecialAssets.assets.Add(go);
            var overheadgo = Plugin.bundle.LoadAsset<GameObject>(overheadElementPath);
            if (overheadgo == null)
            {
                return null;
            }
            SpecialAssets.assets.Add(overheadgo);
            var braveAssetName = player.name;
            if (!braveAssetName.ToLowerInvariant().StartsWith("player"))
            {
                braveAssetName = $"Player{braveAssetName}";
            }

            //setup sprite
            var sprite = tk2dSprite.AddComponent(go, player.transform.Find("PlayerSprite").GetComponent<tk2dBaseSprite>().Collection,
                player.transform.Find("PlayerSprite").GetComponent<tk2dSpriteAnimator>().GetClipByName("select_idle")?.frames?.FirstOrDefault()?.spriteId ?? player.transform.Find("PlayerSprite").GetComponent<tk2dBaseSprite>().spriteId);

            //setup animator
            var anim = go.AddComponent<tk2dSpriteAnimator>();
            anim.Library = player.transform.Find("PlayerSprite").GetComponent<tk2dSpriteAnimator>().Library;
            anim.defaultClipId = anim.GetClipIdByName("select_idle");
            anim.playAutomatically = anim.defaultClipId >= 0;

            //setup rigidbody
            var rigidbody = go.AddComponent<SpeculativeRigidbody>();
            rigidbody.PixelColliders = new()
            {
                new()
                {
                    CollisionLayer = CollisionLayer.LowObstacle,
                    ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual,
                    ManualOffsetX = obstacle.Left,
                    ManualOffsetY = obstacle.Bottom,
                    ManualWidth = obstacle.Width,
                    ManualHeight = obstacle.Height
                },
                new()
                {
                    CollisionLayer = CollisionLayer.BulletBlocker,
                    ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual,
                    ManualOffsetX = bulletblocker.Left,
                    ManualOffsetY = bulletblocker.Bottom,
                    ManualWidth = bulletblocker.Width,
                    ManualHeight = bulletblocker.Height
                }
            };
            rigidbody.TK2DSprite = sprite;

            //setup talkdoer
            var talkdoer = go.AddComponent<TalkDoerLite>();
            talkdoer.usesOverrideInteractionRegion = false;
            talkdoer.overrideRegionOffset = Vector2.zero;
            talkdoer.overrideRegionDimensions = new(10f, 10f);
            talkdoer.overrideInteractionRadius = 2f;
            talkdoer.PreventInteraction = false;
            talkdoer.AllowPlayerToPassEventually = true;
            talkdoer.speakPoint = go.transform.Find("TalkPoint");
            talkdoer.SpeaksGleepGlorpenese = false;
            talkdoer.audioCharacterSpeechTag = player.characterAudioSpeechTag;
            talkdoer.playerApproachRadius = 5f;
            talkdoer.conversationBreakRadius = 7f;
            talkdoer.echo1 = null;
            talkdoer.echo2 = null;
            talkdoer.PreventCoopInteraction = true;
            talkdoer.IsPaletteSwapped = false;
            talkdoer.PaletteTexture = null;
            talkdoer.teleportInSettings = talkdoer.teleportOutSettings = new() { anim = "", animDelay = 0f, timing = Teleport.Timing.Simultaneous, vfx = null, vfxAnchor = null, vfxDelay = 0f };
            talkdoer.itemsToLeaveBehind = new();
            talkdoer.shadow = go.transform.Find("DefaultShadowSprite").gameObject;
            talkdoer.DisableOnShortcutRun = false;
            talkdoer.OptionalMinimapIcon = null;
            talkdoer.OverheadUIElementDelay = 0.5f;
            talkdoer.OptionalCustomNotificationSprite = null;
            talkdoer.OutlineDepth = 0.4f;
            talkdoer.OutlineLuminanceCutoff = 0.05f;
            talkdoer.ReassignPrefabReferences = new();
            talkdoer.MovementSpeed = 3f;
            talkdoer.PathableTiles = CellTypes.FLOOR;
            talkdoer.IsDoingForcedSpeech = false;

            //setup playmaker fsm
            #region bad
            var playmakerfsm = go.AddComponent<PlayMakerFSM>();
            playmakerfsm.fsm = new()
            {
                dataVersion = 1,
                usedInTemplate = null,
                name = $"{player.name} FoyerFSM",
                startState = "Sitting Around",
                states = new FsmState[5],
                events = new FsmEvent[11],
                globalTransitions = new FsmTransition[2],
                variables = new()
                {
                    categories = new string[] { "" },
                    floatVariables = new FsmFloat[0],
                    intVariables = new FsmInt[0],
                    boolVariables = new FsmBool[0],
                    stringVariables = new FsmString[] { new("currentMode") { value = "modeBegin" } },
                    vector2Variables = new FsmVector2[0],
                    vector3Variables = new FsmVector3[0],
                    colorVariables = new FsmColor[0],
                    rectVariables = new FsmRect[0],
                    quaternionVariables = new FsmQuaternion[0],
                    gameObjectVariables = new FsmGameObject[0],
                    objectVariables = new FsmObject[0],
                    materialVariables = new FsmMaterial[0],
                    textureVariables = new FsmTexture[0],
                    arrayVariables = new FsmArray[0],
                    enumVariables = new FsmEnum[0],
                    variableCategoryIDs = new int[0]
                },
                description = "Convict\n", //???
                docUrl = "",
                showStateLabel = true,
                maxLoopCount = 0,
                watermark = "Spy.png",
                password = "",
                locked = false,
                manualUpdate = false,
                keepDelayedEventsOnStateExit = false,
                preprocessed = false,
                editorFlags = Fsm.EditorFlags.nameIsExpanded | Fsm.EditorFlags.controlsIsExpanded,
                activeStateName = "",
                mouseEvents = false,
                handleTriggerEnter2D = false,
                handleTriggerExit2D = false,
                handleTriggerStay2D = false,
                handleCollisionEnter2D = false,
                handleCollisionExit2D = false,
                handleCollisionStay2D = false,
                handleTriggerEnter = false,
                handleTriggerExit = false,
                handleTriggerStay = false,
                handleCollisionEnter = false,
                handleCollisionExit = false,
                handleCollisionStay = false,
                handleParticleCollision = false,
                handleControllerColliderHit = false,
                handleJointBreak = false,
                handleJointBreak2D = false,
                handleOnGUI = false,
                handleFixedUpdate = false,
                handleApplicationEvents = false,
                handleAnimatorMove = false,
                handleAnimatorIK = false
            };
            playmakerfsm.fsm.states = new FsmState[]
            {
                new((Fsm)null)
                {
                    name = "Sitting Around",
                    description = "",
                    colorIndex = 1,
                    position = new(63.75f, 105.5f, 115f, 32f),
                    isBreakpoint = false,
                    isSequence = false,
                    hideUnused = false,
                    transitions = new FsmTransition[]
                    {
                        new()
                        {
                            fsmEvent = new(FsmEvent.FindEvent("playerInteract")),
                            toState = "Mode Switchboard",
                            linkStyle = FsmTransition.CustomLinkStyle.Default,
                            linkConstraint = FsmTransition.CustomLinkConstraint.None,
                            colorIndex = 0
                        }
                    },
                    actionData = new()
                    {
                        actionNames = new()
                        {
                            "HutongGames.PlayMaker.Actions.EndConversation"
                        },
                        customNames = new()
                        {
                            ""
                        },
                        actionEnabled = new()
                        {
                            true
                        },
                        actionIsOpen = new()
                        {
                            true
                        },
                        actionStartIndex = new()
                        {
                            0
                        },
                        actionHashCodes = new()
                        {
                            68280249
                        },
                        unityObjectParams = new(),
                        fsmGameObjectParams = new(),
                        fsmOwnerDefaultParams = new(),
                        animationCurveParams = new(),
                        functionCallParams = new(),
                        fsmTemplateControlParams = new(),
                        fsmEventTargetParams = new(),
                        fsmPropertyParams = new(),
                        layoutOptionParams = new(),
                        fsmStringParams = new(),
                        fsmObjectParams = new(),
                        fsmVarParams = new(),
                        fsmArrayParams = new(),
                        fsmEnumParams = new(),
                        fsmFloatParams = new(),
                        fsmIntParams = new(),
                        fsmBoolParams = new(),
                        fsmVector2Params = new(),
                        fsmVector3Params = new(),
                        fsmColorParams = new(),
                        fsmRectParams = new(),
                        fsmQuaternionParams = new(),
                        stringParams = new(),
                        byteData = new()
                        {
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0
                        },
                        arrayParamSizes = new(),
                        arrayParamTypes = new(),
                        customTypeSizes = new(),
                        customTypeNames = new(),
                        paramDataType = new()
                        {
                            ParamDataType.FsmBool,
                            ParamDataType.FsmBool,
                            ParamDataType.FsmBool,
                            ParamDataType.FsmBool,
                        },
                        paramName = new()
                        {
                            "killZombieTextBoxes",
                            "doNotLerpCamera",
                            "suppressReinteractDelay",
                            "suppressFurtherInteraction"
                        },
                        paramDataPos = new()
                        {
                            0,
                            2,
                            4,
                            6
                        },
                        paramByteDataSize = new()
                        {
                            2,
                            2,
                            2,
                            2
                        }
                    }
                },
                new((Fsm)null)
                {
                    name = "Mode Switchboard",
                    description = "",
                    colorIndex = 0,
                    position = new(231.5313f, 113.6172f, 142f, 16f),
                    isBreakpoint = false,
                    isSequence = false,
                    hideUnused = false,
                    transitions = new FsmTransition[0],
                    actionData = new()
                    {
                        actionNames = new()
                        {
                            "HutongGames.PlayMaker.Actions.ModeSwitchboard"
                        },
                        customNames = new()
                        {
                            ""
                        },
                        actionEnabled = new()
                        {
                            true
                        },
                        actionIsOpen = new()
                        {
                            true
                        },
                        actionStartIndex = new()
                        {
                            0
                        },
                        actionHashCodes = new()
                        {
                            0
                        },
                        unityObjectParams = new(),
                        fsmGameObjectParams = new(),
                        fsmOwnerDefaultParams = new(),
                        animationCurveParams = new(),
                        functionCallParams = new(),
                        fsmTemplateControlParams = new(),
                        fsmEventTargetParams = new(),
                        fsmPropertyParams = new(),
                        layoutOptionParams = new(),
                        fsmStringParams = new(),
                        fsmObjectParams = new(),
                        fsmVarParams = new(),
                        fsmArrayParams = new(),
                        fsmEnumParams = new(),
                        fsmFloatParams = new(),
                        fsmIntParams = new(),
                        fsmBoolParams = new(),
                        fsmVector2Params = new(),
                        fsmVector3Params = new(),
                        fsmColorParams = new(),
                        fsmRectParams = new(),
                        fsmQuaternionParams = new(),
                        stringParams = new(),
                        byteData = new(),
                        arrayParamSizes = new(),
                        arrayParamTypes = new(),
                        customTypeSizes = new(),
                        customTypeNames = new(),
                        paramDataType = new(),
                        paramName = new(),
                        paramDataPos = new(),
                        paramByteDataSize = new()
                    }
                },
                new((Fsm)null)
                {
                    name = "First Speech",
                    description = "",
                    colorIndex = 0,
                    position = new(240.1953f, 247.6641f, 102f, 64f),
                    isBreakpoint = false,
                    isSequence = false,
                    hideUnused = false,
                    transitions = new FsmTransition[]
                    {
                        new()
                        {
                            fsmEvent = new(FsmEvent.FindEvent("yes")),
                            toState = "Do Character Change",
                            linkStyle = FsmTransition.CustomLinkStyle.Default,
                            linkConstraint = FsmTransition.CustomLinkConstraint.None,
                            colorIndex = 0
                        },
                        new()
                        {
                            fsmEvent = new(FsmEvent.FindEvent("no")),
                            toState = "End",
                            linkStyle = FsmTransition.CustomLinkStyle.Default,
                            linkConstraint = FsmTransition.CustomLinkConstraint.None,
                            colorIndex = 0
                        },
                        new()
                        {
                            fsmEvent = new(FsmEvent.FindEvent("FINISHED")),
                            toState = "End",
                            linkStyle = FsmTransition.CustomLinkStyle.Default,
                            linkConstraint = FsmTransition.CustomLinkConstraint.None,
                            colorIndex = 0
                        },
                    },
                    actionData = new()
                    {
                        actionNames = new()
                        {
                            "HutongGames.PlayMaker.Actions.BeginConversation",
                            "HutongGames.PlayMaker.Actions.DialogueBox"
                        },
                        customNames = new()
                        {
                            "",
                            ""
                        },
                        actionEnabled = new()
                        {
                            true,
                            true
                        },
                        actionIsOpen = new()
                        {
                            true,
                            true
                        },
                        actionStartIndex = new()
                        {
                            0,
                            5
                        },
                        actionHashCodes = new()
                        {
                            89987726,
                            34040686
                        },
                        unityObjectParams = new()
                        {
                            null
                        },
                        fsmGameObjectParams = new(),
                        fsmOwnerDefaultParams = new(),
                        animationCurveParams = new(),
                        functionCallParams = new(),
                        fsmTemplateControlParams = new(),
                        fsmEventTargetParams = new(),
                        fsmPropertyParams = new(),
                        layoutOptionParams = new(),
                        fsmStringParams = new()
                        {
                            new FsmString("") { value = overrideChangeCharacterString ?? "#CHARACTER_SELECT_CHANGE_QUESTION" },
                            new FsmString("") { value = overrideYesResponse ?? "#YES" },
                            new FsmString("") { value = overrideNoResponse ?? "#NO" },
                            new FsmString("") { value = "" },
                        },
                        fsmObjectParams = new(),
                        fsmVarParams = new(),
                        fsmArrayParams = new(),
                        fsmEnumParams = new(),
                        fsmFloatParams = new(),
                        fsmIntParams = new(),
                        fsmBoolParams = new(),
                        fsmVector2Params = new(),
                        fsmVector3Params = new(),
                        fsmColorParams = new(),
                        fsmRectParams = new(),
                        fsmQuaternionParams = new(),
                        stringParams = new(),
                        byteData = new()
                        {
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            128,
                            191,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            4,
                            0,
                            0,
                            0,
                            1,
                            0,
                            0,
                            0,
                            0,
                            121,
                            101,
                            115,
                            110,
                            111,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            1,
                            0,
                            0,
                            0,
                            0,
                            0,
                        },
                        arrayParamSizes = new()
                        {
                            1,
                            2,
                            2
                        },
                        arrayParamTypes = new()
                        {
                            "HutongGames.PlayMaker.FsmString",
                            "HutongGames.PlayMaker.FsmString",
                            "HutongGames.PlayMaker.FsmEvent"
                        },
                        customTypeSizes = new(),
                        customTypeNames = new(),
                        paramDataType = new()
                        {
                            ParamDataType.Enum,
                            ParamDataType.Enum,
                            ParamDataType.FsmFloat,
                            ParamDataType.Boolean,
                            ParamDataType.Vector2,
                            ParamDataType.Enum,
                            ParamDataType.Enum,
                            ParamDataType.FsmInt,
                            ParamDataType.Array,
                            ParamDataType.FsmString,
                            ParamDataType.Array,
                            ParamDataType.FsmString,
                            ParamDataType.FsmString,
                            ParamDataType.Array,
                            ParamDataType.FsmEvent,
                            ParamDataType.FsmEvent,
                            ParamDataType.FsmBool,
                            ParamDataType.FsmFloat,
                            ParamDataType.FsmFloat,
                            ParamDataType.FsmBool,
                            ParamDataType.FsmString,
                            ParamDataType.FsmBool,
                            ParamDataType.FsmBool,
                            ParamDataType.ObjectReference,
                        },
                        paramName = new()
                        {
                            "conversationType",
                            "locked",
                            "overrideNpcScreenHeight",
                            "UsesCustomScreenBuffer",
                            "CustomScreenBuffer",
                            "condition",
                            "sequence",
                            "persistentStringsToShow",
                            "dialogue",
                            "",
                            "responses",
                            "",
                            "",
                            "events",
                            "",
                            "",
                            "skipWalkAwayEvent",
                            "forceCloseTime",
                            "zombieTime",
                            "SuppressDefaultAnims",
                            "OverrideTalkAnim",
                            "PlayBoxOnInteractingPlayer",
                            "IsThoughtBubble",
                            "AlternativeTalker"
                        },
                        paramDataPos = new()
                        {
                            0,
                            4,
                            8,
                            13,
                            14,
                            22,
                            26,
                            30,
                            0,
                            0,
                            1,
                            1,
                            2,
                            2,
                            35,
                            38,
                            40,
                            42,
                            47,
                            52,
                            3,
                            54,
                            56,
                            0
                        },
                        paramByteDataSize = new()
                        {
                            4,
                            4,
                            5,
                            1,
                            8,
                            4,
                            4,
                            5,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            3,
                            2,
                            2,
                            5,
                            5,
                            2,
                            0,
                            2,
                            2,
                            0
                        }
                    }
                },
                new((Fsm)null)
                {
                    name = "Do Character Change",
                    description = "",
                    colorIndex = 0,
                    position = new(477.6328f, 256.0547f, 160f, 16f),
                    isBreakpoint = false,
                    isSequence = true,
                    hideUnused = false,
                    transitions = new FsmTransition[0],
                    actionData = new()
                    {
                        actionNames = new()
                        {
                            "HutongGames.PlayMaker.Actions.EndConversation",
                            "HutongGames.PlayMaker.Actions.ChangeToNewCharacter",
                            "HutongGames.PlayMaker.Actions.RestartWhenFinished"
                        },
                        customNames = new()
                        {
                            "",
                            "",
                            ""
                        },
                        actionEnabled = new()
                        {
                            true,
                            true,
                            true
                        },
                        actionIsOpen = new()
                        {
                            true,
                            true,
                            true
                        },
                        actionStartIndex = new()
                        {
                            0,
                            4,
                            5
                        },
                        actionHashCodes = new()
                        {
                            68280249,
                            19973496,
                            0
                        },
                        unityObjectParams = new(),
                        fsmGameObjectParams = new(),
                        fsmOwnerDefaultParams = new(),
                        animationCurveParams = new(),
                        functionCallParams = new(),
                        fsmTemplateControlParams = new(),
                        fsmEventTargetParams = new(),
                        fsmPropertyParams = new(),
                        layoutOptionParams = new(),
                        fsmStringParams = new(),
                        fsmObjectParams = new(),
                        fsmVarParams = new(),
                        fsmArrayParams = new(),
                        fsmEnumParams = new(),
                        fsmFloatParams = new(),
                        fsmIntParams = new(),
                        fsmBoolParams = new(),
                        fsmVector2Params = new(),
                        fsmVector3Params = new(),
                        fsmColorParams = new(),
                        fsmRectParams = new(),
                        fsmQuaternionParams = new(),
                        stringParams = new(),
                        byteData = new List<byte>()
                        {
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0
                        }.Concat(FsmUtility.Encoding.GetBytes(braveAssetName)).ToList(),
                        arrayParamSizes = new(),
                        arrayParamTypes = new(),
                        customTypeSizes = new(),
                        customTypeNames = new(),
                        paramDataType = new()
                        {
                            ParamDataType.FsmBool,
                            ParamDataType.FsmBool,
                            ParamDataType.FsmBool,
                            ParamDataType.FsmBool,
                            ParamDataType.String
                        },
                        paramName = new()
                        {
                            "killZombieTextBoxes",
                            "doNotLerpCamera",
                            "suppressReinteractDelay",
                            "suppressFurtherInteraction",
                            "PlayerPrefabPath"
                        },
                        paramDataPos = new()
                        {
                            0,
                            2,
                            4,
                            6,
                            8
                        },
                        paramByteDataSize = new()
                        {
                            2,
                            2,
                            2,
                            2,
                            braveAssetName.Length
                        }
                    }
                },
                new((Fsm)null)
                {
                    name = "End",
                    description = "",
                    colorIndex = 0,
                    position = new(498.5f, 315f, 100f, 16f),
                    isBreakpoint = false,
                    isSequence = true,
                    hideUnused = false,
                    transitions = new FsmTransition[0],
                    actionData = new()
                    {
                        actionNames = new()
                        {
                            "HutongGames.PlayMaker.Actions.RestartWhenFinished"
                        },
                        customNames = new()
                        {
                            ""
                        },
                        actionEnabled = new()
                        {
                            true
                        },
                        actionIsOpen = new()
                        {
                            true
                        },
                        actionStartIndex = new()
                        {
                            0
                        },
                        actionHashCodes = new()
                        {
                            0
                        },
                        unityObjectParams = new(),
                        fsmGameObjectParams = new(),
                        fsmOwnerDefaultParams = new(),
                        animationCurveParams = new(),
                        functionCallParams = new(),
                        fsmTemplateControlParams = new(),
                        fsmEventTargetParams = new(),
                        fsmPropertyParams = new(),
                        layoutOptionParams = new(),
                        fsmStringParams = new(),
                        fsmObjectParams = new(),
                        fsmVarParams = new(),
                        fsmArrayParams = new(),
                        fsmEnumParams = new(),
                        fsmFloatParams = new(),
                        fsmIntParams = new(),
                        fsmBoolParams = new(),
                        fsmVector2Params = new(),
                        fsmVector3Params = new(),
                        fsmColorParams = new(),
                        fsmRectParams = new(),
                        fsmQuaternionParams = new(),
                        stringParams = new(),
                        byteData = new(),
                        arrayParamSizes = new(),
                        arrayParamTypes = new(),
                        customTypeSizes = new(),
                        customTypeNames = new(),
                        paramDataType = new(),
                        paramName = new(),
                        paramDataPos = new(),
                        paramByteDataSize = new()
                    }
                }
            };
            playmakerfsm.fsm.events = new FsmEvent[]
            {
                new(FsmEvent.FindEvent("FINISHED")),
                new(FsmEvent.FindEvent("RESTART")),
                new(FsmEvent.FindEvent("modeBegin")),
                new(FsmEvent.FindEvent("modeCompletedTutorial")),
                new(FsmEvent.FindEvent("modeFirstSpeech")),
                new(FsmEvent.FindEvent("modeGatlingGull")),
                new(FsmEvent.FindEvent("modeNotCompletedTutorial")),
                new(FsmEvent.FindEvent("modeNothingElseToSay")),
                new(FsmEvent.FindEvent("no")),
                new(FsmEvent.FindEvent("playerInteract")),
                new(FsmEvent.FindEvent("yes"))
            };
            playmakerfsm.fsm.globalTransitions = new FsmTransition[]
            {
                new()
                {
                    fsmEvent = new(FsmEvent.FindEvent("RESTART")),
                    toState = "Sitting Around",
                    linkStyle = FsmTransition.CustomLinkStyle.Default,
                    linkConstraint = FsmTransition.CustomLinkConstraint.None,
                    colorIndex = 0
                },
                new()
                {
                    fsmEvent = new(FsmEvent.FindEvent("modeBegin")),
                    toState = "First Speech",
                    linkStyle = FsmTransition.CustomLinkStyle.Default,
                    linkConstraint = FsmTransition.CustomLinkConstraint.None,
                    colorIndex = 0
                }
            };
            playmakerfsm.fsmTemplate = null;
            #endregion

            //setup idle doer
            var idledoer = go.AddComponent<CharacterSelectIdleDoer>();
            idledoer.coreIdleAnimation = "select_idle";
            idledoer.onSelectedAnimation = "select_select";
            idledoer.phases = new CharacterSelectIdlePhase[0];
            idledoer.IsEevee = false;
            idledoer.EeveeTex = null;
            idledoer.AnimationLibraries = new tk2dSpriteAnimation[0];

            //setup select flag
            var selectflag = go.AddComponent<FoyerCharacterSelectFlag>();
            selectflag.CharacterPrefabPath = braveAssetName;
            selectflag.OverheadElement = talkdoer.OverheadUIElementOnPreInteract = overheadgo;
            selectflag.IsCoopCharacter = false;
            selectflag.IsGunslinger = false;
            selectflag.IsEevee = false;
            selectflag.prerequisites = unlockPrereqs.ToArray();
            selectflag.AltCostumeLibrary = player.AlternateCostumeLibrary;

            //setup shadow sprite
            var defaultShadowSprite = ResourceCache.Acquire("DefaultShadowSprite").As<GameObject>().GetComponent<tk2dBaseSprite>();
            tk2dSprite.AddComponent(go.transform.Find("DefaultShadowSprite").gameObject, defaultShadowSprite.Collection, defaultShadowSprite.spriteId);

            //setup panel
            var panel = overheadgo.AddComponent<dfPanel>();
            var techBlueSkin = LoadHelper.LoadAssetFromAnywhere<GameObject>("TechBlue Skin").GetComponent<dfAtlas>();
            var gameUiAtlas = LoadHelper.LoadAssetFromAnywhere<GameObject>("GameUIAtlas").GetComponent<dfAtlas>();
            var daikonBitmap = LoadHelper.LoadAssetFromAnywhere<GameObject>("OurFont_DaikonBitmap").GetComponent<dfFont>();
            panel.atlas = techBlueSkin;
            panel.backgroundSprite = "";
            panel.anchorStyle = dfAnchorStyle.All | dfAnchorStyle.Proportional;
            panel.layout = new(dfAnchorStyle.All | dfAnchorStyle.Proportional) { owner = panel, margins = new() { bottom = 0.5f, left = 0.4438f, right = 0.5563f, top = 0.3f } };
            panel.pivot = dfPivotPoint.BottomCenter;
            panel.renderOrder = 34;
            panel.size = new(162f, 162f);
            panel.zindex = 0;
            panel.tooltip = "";
            var infopanel = overheadgo.AddComponent<FoyerInfoPanelController>();
            infopanel.characterIdentity = player.characterIdentity;
            var textpanel = overheadgo.transform.Find("TextPanel");
            var panelcomp = textpanel.AddComponent<dfPanel>();
            infopanel.textPanel = panelcomp;
            panelcomp.atlas = techBlueSkin;
            panelcomp.backgroundSprite = "";
            panelcomp.anchorStyle = dfAnchorStyle.All | dfAnchorStyle.Proportional;
            panelcomp.layout = new(dfAnchorStyle.All | dfAnchorStyle.Proportional) { owner = panelcomp, margins = new() { bottom = 0.7037f, left = 0.8519f, right = 0.8519f, top = -0.4074f } };
            panelcomp.renderOrder = 35;
            panelcomp.size = new(0f, 202f);
            panelcomp.zindex = 0;
            panelcomp.tooltip = "";
            panelcomp.clipChildren = true;
            var nameLabel = textpanel.Find("NameLabel").AddComponent<dfLabel>();
            nameLabel.atlas = gameUiAtlas;
            nameLabel.backgroundSprite = "chamber_flash_letter_001";
            nameLabel.backgroundColor = new(0, 0, 0, 255);
            nameLabel.font = daikonBitmap;
            nameLabel.text = breachName;
            nameLabel.isLocalized = false;
            nameLabel.textScale = 3f;
            nameLabel.anchorStyle = dfAnchorStyle.Left;
            nameLabel.renderOrder = 36;
            nameLabel.size = Vector2.zero;
            nameLabel.autoSize = true;
            nameLabel.tooltip = "";
            nameLabel.zindex = 0;
            var gunLabel = textpanel.Find("GunLabel").AddComponent<dfLabel>();
            gunLabel.atlas = gameUiAtlas;
            gunLabel.backgroundSprite = "chamber_flash_letter_001";
            gunLabel.backgroundColor = new(0, 0, 0, 255);
            gunLabel.font = daikonBitmap;
            gunLabel.text = description2Text ?? "";
            gunLabel.isVisible = !string.IsNullOrEmpty(description2Text);
            gunLabel.isLocalized = false;
            gunLabel.textScale = 3f;
            gunLabel.anchorStyle = dfAnchorStyle.Left;
            gunLabel.renderOrder = 36;
            gunLabel.size = Vector2.zero;
            gunLabel.autoSize = true;
            gunLabel.tooltip = "";
            gunLabel.zindex = 0;
            var descLabel = textpanel.Find("DescLabel").AddComponent<dfLabel>();
            descLabel.atlas = gameUiAtlas;
            descLabel.backgroundSprite = "chamber_flash_letter_001";
            descLabel.backgroundColor = new(0, 0, 0, 255);
            descLabel.font = daikonBitmap;
            descLabel.text = description1Text ?? "";
            descLabel.isVisible = !string.IsNullOrEmpty(description1Text);
            descLabel.textScale = 3f;
            descLabel.anchorStyle = dfAnchorStyle.Left;
            descLabel.renderOrder = 39;
            descLabel.size = Vector2.zero;
            descLabel.autoSize = true;
            descLabel.tooltip = "";
            descLabel.zindex = 3;
            var pastKilledLabel = textpanel.Find("PastKilledLabel").AddComponent<dfLabel>();
            pastKilledLabel.atlas = gameUiAtlas;
            pastKilledLabel.backgroundSprite = "chamber_flash_letter_001";
            pastKilledLabel.backgroundColor = new(0, 0, 0, 255);
            pastKilledLabel.font = daikonBitmap;
            pastKilledLabel.text = "#CHARACTERSELECT_PASTKILLED";
            pastKilledLabel.isLocalized = true;
            pastKilledLabel.textScale = 3f;
            pastKilledLabel.anchorStyle = dfAnchorStyle.Left;
            pastKilledLabel.renderOrder = 37;
            pastKilledLabel.size = Vector2.zero;
            pastKilledLabel.autoSize = true;
            pastKilledLabel.tooltip = "";
            pastKilledLabel.zindex = 1;
            var itemsPanel = infopanel.itemsPanel = textpanel.Find("ItemsPanel").AddComponent<dfPanel>();
            itemsPanel.atlas = gameUiAtlas;
            itemsPanel.backgroundColor = new(0, 0, 0, 255);
            itemsPanel.backgroundSprite = "chamber_flash_letter_001";
            itemsPanel.anchorStyle = dfAnchorStyle.Top | dfAnchorStyle.Left;
            itemsPanel.renderOrder = 40;
            itemsPanel.zindex = 4;
            itemsPanel.tooltip = "";
            itemsPanel.size = new(850f, 75f);
            if (!string.IsNullOrEmpty(itemsTexture))
            {
                var itemsSprite = itemsPanel.transform.Find("ItemsSprite").AddComponent<dfSprite>();
                itemsSprite.atlas = gameUiAtlas;
                itemsSprite.spriteName = AddToAtlas(itemsTexture, out var info);
                itemsSprite.size = info.sizeInPixels * 3;
                itemsSprite.renderOrder = 41;
                itemsSprite.zindex = 5;
                itemsSprite.tooltip = "";
            }

            var arrow = overheadgo.transform.Find("Arrow");
            var facecardColl = LoadHelper.LoadAssetFromAnywhere<GameObject>("CHR_RoguePanel").transform.Find("Rogue Arrow").GetComponent<tk2dSprite>().Collection;
            var facecardAnimation = LoadHelper.LoadAssetFromAnywhere<GameObject>("Facecard_Animation").GetComponent<tk2dSpriteAnimation>();
            infopanel.scaledSprites = new tk2dSprite[] { infopanel.arrow = tk2dSprite.AddComponent(arrow.gameObject, facecardColl, facecardColl.GetSpriteIdByName("SelectionReticle")) };
            var arrowanim = arrow.AddComponent<tk2dSpriteAnimator>();
            arrowanim.defaultClipId = 22;
            arrowanim.playAutomatically = true;
            arrowanim.Library = facecardAnimation;
            var facecard = arrow.Find("Sprite FaceCard");
            facecard.gameObject.layer = LayerMask.NameToLayer("GUI");
            var facecardIdleDoer = facecard.AddComponent<CharacterSelectFacecardIdleDoer>();
            facecardIdleDoer.appearAnimation = $"{facecardAnimationPrefix}_facecard_appear";
            facecardIdleDoer.coreIdleAnimation = $"{facecardAnimationPrefix}_facecard_idle";
            facecardIdleDoer.idleMin = 0f;
            facecardIdleDoer.idleMax = 0f;
            facecardIdleDoer.usesMultipleIdleAnimations = false;
            facecardIdleDoer.multipleIdleAnimations = new string[0];
            facecardIdleDoer.EeveeTex = null;

            breachCharacters[go] = breachPosition;
            return selectflag;
        }

        private static readonly FieldInfo UncachedPlayerNamesField = typeof(PunchoutPlayerController).GetField("PlayerNames", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly FieldInfo UncachedPlayerUiNamesField = typeof(PunchoutPlayerController).GetField("PlayerUiNames", BindingFlags.NonPublic | BindingFlags.Static);

        public static string[] UncachedPlayerNames => ((string[])UncachedPlayerNamesField.GetValue(null));
        public static string[] UncachedPlayerUiNames => ((string[])UncachedPlayerUiNamesField.GetValue(null));

        private static void AddPlayerName(string name)
        {
            UncachedPlayerNamesField.SetValue(null, UncachedPlayerNames.AddToArray(name));
        }

        private static void AddPlayerUiName(string name)
        {
            UncachedPlayerUiNamesField.SetValue(null, UncachedPlayerUiNames.AddToArray(name));
        }

        public static void AddPunchoutSprites(this PlayerController p, string punchoutprefix, string facecardprefix)
        {
            if (UncachedPlayerNames.Length == 7)
            {
                AddPlayerName("dummy");
                AddPlayerUiName("dummy");
            }

            AddPlayerName(punchoutprefix);
            punchoutSprites.Add(p.characterIdentity, UncachedPlayerNames.Length - 1);
            AddPlayerUiName($"ccr_{facecardprefix}_00");

            punchoutFacecardAtlas.AddNewItemToAtlas(Plugin.bundle.LoadAsset<Texture2D>($"{facecardprefix}_001"), $"ccr_{facecardprefix}_001");
            punchoutFacecardAtlas.AddNewItemToAtlas(Plugin.bundle.LoadAsset<Texture2D>($"{facecardprefix}_002"), $"ccr_{facecardprefix}_002");
            punchoutFacecardAtlas.AddNewItemToAtlas(Plugin.bundle.LoadAsset<Texture2D>($"{facecardprefix}_003"), $"ccr_{facecardprefix}_003");

            var animSoundDict = new Dictionary<string, string>()
            {
                { $"{punchoutprefix}_hit_left", "Play_BOSS_Punchout_Punch_Hit_01" },
                { $"{punchoutprefix}_hit_right", "Play_BOSS_Punchout_Punch_Hit_01" },
                { $"{punchoutprefix}_dodge_right", "Play_BOSS_RatPunchout_Player_Dodge_01" },
                { $"{punchoutprefix}_dodge_left", "Play_BOSS_RatPunchout_Player_Dodge_01" },
                { $"{punchoutprefix}_duck", "Play_BOSS_RatPunchout_Player_Dodge_01" },
                { $"{punchoutprefix}_block_hit", "Play_BOSS_Punchout_Punch_Block_01" },
                { $"{punchoutprefix}_punch_right", "Play_BOSS_Punchout_Swing_Right_01" },
                { $"{punchoutprefix}_punch_left", "Play_BOSS_Punchout_Swing_Left_01" },
                { $"{punchoutprefix}_super", "Play_BOSS_RatPunchout_Player_Charge_01" },
                { $"{punchoutprefix}_knockout", "Play_BOSS_Punchout_Punch_KO_01" },
            };

            foreach (var kvp in animSoundDict)
            {
                var f = punchoutAnim.GetClipByName(kvp.Key).frames[0];
                f.triggerEvent = true;
                f.eventAudio = kvp.Value;
            }
        }

        public static void AddOffset(this tk2dSpriteDefinition def, IntVector2 vec)
        {
            var unitsOffset = vec.ToVector3() / 16f;
            def.position0 += unitsOffset;
            def.position1 += unitsOffset;
            def.position2 += unitsOffset;
            def.position3 += unitsOffset;
        }

        public static string AddCustomAmmoType(string name, string objName, string texture)
        {
            var obj = Plugin.bundle.LoadAsset<GameObject>(objName).transform;

            var fgSprite = obj.Find("AmmoType").gameObject.SetupDfSpriteFromTexture<dfTiledSprite>(Plugin.bundle.LoadAsset<Texture2D>($"{texture}full"), ShaderCache.Acquire("Daikon Forge/Default UI Shader"));
            var bgSprite = obj.Find("Empty").gameObject.SetupDfSpriteFromTexture<dfTiledSprite>(Plugin.bundle.LoadAsset<Texture2D>($"{texture}empty"), ShaderCache.Acquire("Daikon Forge/Default UI Shader"));

            fgSprite.zindex = 7;
            bgSprite.zindex = 5;

            var uiammotype = new GameUIAmmoType()
            {
                ammoBarBG = bgSprite,
                ammoBarFG = fgSprite,
                ammoType = GameUIAmmoType.AmmoType.CUSTOM,
                customAmmoType = name
            };

            addedAmmoTypes.Add(uiammotype);

            if (GameUIRoot.HasInstance)
            {
                foreach (var uiammocontroller in GameUIRoot.Instance.ammoControllers)
                {
                    if (uiammocontroller.m_initialized)
                    {
                        uiammocontroller.ammoTypes = uiammocontroller.ammoTypes.AddToArray(uiammotype);
                    }
                }
            }

            return name;
        }

        public static T SetupDfSpriteFromTexture<T>(this GameObject obj, Texture2D texture, Shader shader) where T : dfSprite
        {
            T sprite = obj.GetOrAddComponent<T>();
            dfAtlas atlas = obj.GetOrAddComponent<dfAtlas>();
            atlas.Material = new(shader);
            atlas.Material.mainTexture = texture;
            atlas.Items.Clear();
            dfAtlas.ItemInfo info = new()
            {
                border = new RectOffset(),
                deleted = false,
                name = "main_sprite",
                region = new Rect(Vector2.zero, new Vector2(1, 1)),
                rotated = false,
                sizeInPixels = new Vector2(texture.width, texture.height),
                texture = null,
                textureGUID = "main_sprite"
            };
            atlas.AddItem(info);
            sprite.Atlas = atlas;
            sprite.SpriteName = "main_sprite";
            sprite.zindex = 0;
            stuff.Add(obj);
            return sprite;
        }

        public static List<GameUIAmmoType> addedAmmoTypes = new();
        public static PunchoutPlayerController punchoutPlayer;
        public static tk2dSpriteCollectionData punchoutColl;
        public static tk2dSpriteAnimation punchoutAnim;
        public static dfAtlas punchoutFacecardAtlas;
        public static Dictionary<GameObject, Vector3> breachCharacters = new();
        public static Dictionary<PlayableCharacters, int> punchoutSprites = new();
        public static List<GameObject> stuff = new();
    }
}
