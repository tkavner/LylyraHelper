using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Celeste.Mod.LylyraHelper.Entities;
using Celeste.Mod.LylyraHelper.Intefaces;
using Celeste.Mod.LylyraHelper.Other;
using FMOD.Studio;
using LylyraHelper;
using LylyraHelper.Other;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.ModInterop;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Celeste.GaussianBlur;
using static Celeste.Tentacles;

namespace Celeste.Mod.LylyraHelper.Components
{
    //gives the entity this is added to the ability to "slice" (See Cutting Algorithm documentation). Entity must have a hitbox that is active.
    [Tracked(false)]
    public class Slicer : Component
    {
        public class SlicerSettings {
            public SlicerSettings(string settings)
            {
                SliceableList = LyraUtils.GetFullNames(settings);
            }

            public SlicerSettings(string[] settings)
            {
                SliceableList = settings.ToList();
            }

            private static SlicerSettings _default = new SlicerSettings(default_string);
            private static string default_string = "nothing";
            private List<string> SliceableList;
            private static SlicerSettings globalSettings;
            public static LylyraHelperSession Session => LylyraHelperModule.Session;

            public static SlicerSettings DefaultSettings { 
                get 
                { 
                    return globalSettings ?? (Session.defaultSlicerSettings != null ? new SlicerSettings(Session.defaultSlicerSettings) : _default);
                } 
                set 
                {
                    Session.defaultSlicerSettings = value.SliceableList.ToArray();
                    globalSettings = value;
                }
            }

            public bool CanSlice(Type type)
            {
                return CanSlice(type.FullName) || CanSlice(type.Name);
            }

            private bool CanSlice(string name)
            {
                return SliceableList != null ? SliceableList.Contains(name) : false;
            }
        }

    private List<Entity> slicingEntities = new List<Entity>();
        //some entities take a frame advancement to activate properly (Such as Kevins and MoveBlocks). This list is for those entities.
        private List<Entity> secondFrameActivation = new List<Entity>();
        private List<Entity> intermediateFrameActivation = new List<Entity>();
        public Collider slicingCollider { get; set; }
        private Vector2 Direction;
        private int cutSize;
        private Dictionary<Entity, Vector2> sliceStartPositions = new Dictionary<Entity, Vector2>();
        private Level level;
        public int directionalOffset { get; set; }

        private bool sliceOnImpact;
        private bool fragile;
        private Vector2 ColliderOffset;
        private Action entityCallback;

        private static List<Entity> masterCuttingList = new List<Entity>();
        private static ulong lastPurge;
        public SlicerSettings settings;

        public int entitiesCut { get; private set; }

        public Slicer(
            Vector2 Direction,
            int cutSize,
            Level level,
            int directionalOffset,
            Collider slicingCollider = null,
            bool active = true,
            bool sliceOnImpact = false,
            bool fragile = false,
            string settings = "") : this(Direction, cutSize, level, directionalOffset, Vector2.Zero, slicingCollider, active, sliceOnImpact, fragile, settings)
        {

        }

        public Slicer(
            Vector2 Direction,
            int cutSize,
            Level level,
            int directionalOffset,
            Vector2 colliderOffset,
            Collider slicingCollider = null,
            bool active = true,
            bool sliceOnImpact = false,
            bool fragile = false,
            string settings = "") : base(active, false)
        {
            this.slicingCollider = slicingCollider;
            this.Direction = Direction;
            this.cutSize = cutSize;
            this.level = level;
            this.directionalOffset = directionalOffset;
            this.sliceOnImpact = sliceOnImpact;
            this.fragile = fragile;
            ColliderOffset = colliderOffset;
            settings = settings.Trim();
            this.settings = settings != "" ? new SlicerSettings(settings) : SlicerSettings.DefaultSettings;
            if (Cuttable.paperScraps == null)
            {
                Chooser<MTexture> sourceChooser = new Chooser<MTexture>(
                    GFX.Game["particles/LylyraHelper/dashpapershard00"],
                    GFX.Game["particles/LylyraHelper/dashpapershard01"],
                    GFX.Game["particles/LylyraHelper/dashpapershard02"]);
                Cuttable.paperScraps = new ParticleType()
                {
                    SourceChooser = sourceChooser,
                    Color = Calc.HexToColor("cac7e3"),
                    Acceleration = new Vector2(0f, 4f),
                    LifeMin = 0.8f,
                    LifeMax = 1.6f,
                    Size = .8f,
                    SizeRange = 0.2f,
                    Direction = (float)Math.PI / 2f,
                    DirectionRange = (float)Math.PI * 2F,
                    SpeedMin = 5f,
                    SpeedMax = 7F,
                    RotationMode = ParticleType.RotationModes.Random,
                    ScaleOut = true,
                    UseActualDeltaTime = true
                };
            }
        }

        

        public override void Update()
        {
            base.Update();
            double oldTime = lastPurge;
            if (Engine.FrameCounter != lastPurge)
            {
                lastPurge  = Engine.FrameCounter;
                masterCuttingList.Clear();
            }


            Vector2 positionHold = Entity.Position;
            Entity.Position = Entity.Position + ColliderOffset;
            Collider tempHold = Entity.Collider;
            if (slicingCollider != null) Entity.Collider = slicingCollider;
            if(this.Entity.Collidable) CheckCollisions();
            Slice();
            Entity.Position = positionHold;
            Entity.Collider = tempHold;
            Visible = true;
        }

        private void CheckCollisions()
        {
            StaticMover sm = Entity.Get<StaticMover>();
            Vector2 Position = Entity.Position;
            //get dash paper, check if colliding, if so add to list (we need to check each type of DashPaper manually apparently for speed)

            /*if (settings.CanSlice(typeof(DashPaper))) foreach (Paper d in Scene.Tracker.GetEntities<DashPaper>())
            {
                if (d == Entity) continue;
                if (sm != null && sm.Entity != null && sm.Entity == d) continue;

                if (!slicingEntities.Contains(d))
                {
                    if (d.CollideCheck(Entity))
                    {
                        slicingEntities.Add(d);
                        sliceStartPositions.Add(d, Position);
                    }
                }
            }

            if (settings.CanSlice(typeof(DeathNote))) foreach (Paper d in base.Scene.Tracker.GetEntities<DeathNote>())
            {
                if (d == Entity) continue;
                if (sm != null && sm.Entity != null && sm.Entity == d) continue;

                if (!slicingEntities.Contains(d))
                {
                    if (d.CollideCheck(Entity))
                    {
                        slicingEntities.Add(d);
                        sliceStartPositions.Add(d, Position);
                    }
                }
            }*/

            foreach (Entity d in Scene.Entities)
            {
                if (!settings.CanSlice(d.GetType())) continue;
                if (masterCuttingList.Contains(d)) continue;
                if (d == Entity) continue;

                if (sm != null && sm.Entity != null && sm.Entity == d) continue;
                //custom entities from other mods
                if (CustomSlicingActions.ContainsKey(d.GetType()))
                {
                    if (!slicingEntities.Contains(d) && Entity.CollideCheck(d))
                    {
                        slicingEntities.Add(d);
                        sliceStartPositions.Add(d, Position);
                        continue;
                    }
                }
                else if (d is ICuttable)
                {
                    if (!slicingEntities.Contains(d) && settings.CanSlice(d.GetType()) && Entity.CollideCheck(d))
                    {
                        slicingEntities.Add(d);
                        sliceStartPositions.Add(d, Position);
                        continue;
                    }
                }
                //vanilla entity handling
                else if (d is Booster)
                {
                    Booster booster = d as Booster;
                    if (!slicingEntities.Contains(d) && d.CollideCheck(Entity))
                    {
                        Type boosterType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.Booster", true, true);
                        bool respawning = ((float)boosterType?.GetField("respawnTimer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(d) > 0);
                        if (!respawning) booster.PlayerReleased();
                    }
                } 
                else if (
                    d.GetType() == typeof(CrystalStaticSpinner) ||
                    d.GetType() == typeof(CrushBlock) || 
                    d.GetType() == typeof(FallingBlock) || 
                    d.GetType() == typeof(DreamBlock) || 
                    d.GetType() == typeof(MoveBlock) || 
                    d.GetType() == typeof(DashBlock) ||
                    d.GetType() == typeof(StarJumpBlock) ||
                    d.GetType() == typeof(BounceBlock) ||
                    d.GetType() == typeof(FloatySpaceBlock))
                {
                    if (!slicingEntities.Contains(d) && Entity.CollideCheck(d))
                    {
                        slicingEntities.Add(d);
                        sliceStartPositions.Add(d, Position);
                    }
                }
            }
        }

        public void AddListener(Action p)
        {
            entityCallback = p;
        }

        //Basically all cutting requires a wild amount of differing requirements to cut in half.
        //This is because we're essentially cloning the object, which is a very complicated issue in computer programming.
        //We have to go through on a case by case basis because some objects require some of their fields to be deep copies (see: CrushBlock ReturnStack), while others should absolutely not be.
        //for modded objects, we can work under the assumption 
        //There's definitely cleanup to be done in here, but in general because we want two (almost) identical copies of objects, with lots of weird exceptions
        public void Slice(bool collisionOverride = false)
        {
            Vector2 Position = Entity.Center;
            float Width = Entity.Width;
            float Height = Entity.Width;

            secondFrameActivation.RemoveAll(d =>
            {
                if (CustomSecondFrameActions.TryGetValue(d.GetType(), out Action<Entity, DynamicData> customAction))
                {
                    customAction.Invoke(d, new DynamicData(this));
                    return true;
                }
                else if (d is CrushBlock)
                {
                    d.Awake(Scene);
                    Type cbType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.CrushBlock", true, true);
                    cbType.GetField("crushDir", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(d, -Direction);
                    cbType.GetField("level", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(d, level);
                    cbType.GetMethod("Attack", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(d, new object[] { -Direction });

                } 
                else if (d is MoveBlock)
                {
                    Type bType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.MoveBlock", true, true);
                    bType.GetField("triggered", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(d, true);
                    Entity border = (Entity) bType.GetField("border", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(d);
                    border.Visible = false;
                }
                return true;
            });

            intermediateFrameActivation.RemoveAll(d =>
            {
                secondFrameActivation.Add(d);
                return true;
            });

            entitiesCut += slicingEntities.RemoveAll(d => { return FinishedCutting(d, collisionOverride); }
            );
        }

        private bool FinishedCutting(Entity d, bool collisionOverride)
        {
            if (!Scene.Contains(d))
                {
                    sliceStartPositions.Remove(d);
                    return true;
                }
                if (masterCuttingList.Contains(d))
                {
                    sliceStartPositions.Remove(d);
                    return true;
                }
                if (d is ICuttable icut)
                {
                    if (collisionOverride || sliceOnImpact || (!d.CollideCheck(Entity)))
                    {
                        sliceStartPositions.TryGetValue(d, out Vector2 startPosition);
                        bool toReturn = icut.Cut(GetDirectionalPosition(), Direction, cutSize, startPosition);
                        sliceStartPositions.Remove(d);
                        return toReturn;
                    }
                    return false;
                }
                else if ((collisionOverride || sliceOnImpact || (!d.CollideCheck(Entity))))
                {
                    if (d is Solid)
                    {
                        if (CustomSlicingActions.ContainsKey(d.GetType()))
                        {
                            if (CustomSlicingActions[d.GetType()].Invoke(d, new DynamicData(this)))
                            {
                                intermediateFrameActivation.Add(d);
                            }
                            sliceStartPositions.Remove(d);
                            return true;
                        }
                        else if (d is DreamBlock)
                        {
                            SliceDreamBlock(d as DreamBlock);
                            return true;
                        }
                        else if (d is CrushBlock)
                        {
                            SliceKevin(d as CrushBlock);
                            return true;
                        }
                        else if (d is FallingBlock)
                        {
                            SliceFallingBlock(d as FallingBlock);
                            return true;
                        }
                        else if (d is MoveBlock)
                        {
                            SliceMoveBlock(d as MoveBlock);
                            return true;
                        }
                        else if (d is DashBlock)
                        {
                            SliceDashBlock(d as DashBlock);
                            return true;
                        }
                        else if (d is StarJumpBlock)
                        {
                            SliceStarJumpBlock(d as StarJumpBlock);
                            return true;
                        }
                        else if (d is BounceBlock)
                        {
                            SliceBounceBlock(d as BounceBlock);
                            return true;
                        }
                        else if (d is FloatySpaceBlock)
                        {
                            SliceFloatySpaceBlock(d as FloatySpaceBlock);
                            return true;
                        }
                    }
                    else if (d is CrystalStaticSpinner)
                    {

                        (d as CrystalStaticSpinner).Destroy();
                        sliceStartPositions.Remove(d);
                        return true;
                    }
                }

                //else this item should not be in the list because cutting it is not supported. Warn and have it removed.

                return false;
        }

        private void SliceFloatySpaceBlock(FloatySpaceBlock original)
        {

            Type bType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.FloatySpaceBlock", true, true);
            Vector2[] resultArray = CalcCuts(original.Position, new Vector2(original.Width, original.Height), Entity.Center, Direction, cutSize);

            Vector2 b1Pos = resultArray[0];
            Vector2 b2Pos = resultArray[1];
            int b1Width = (int)resultArray[2].X;
            int b1Height = (int)resultArray[2].Y;

            int b2Width = (int)resultArray[3].X;
            int b2Height = (int)resultArray[3].Y;

            FloatySpaceBlock b1 = null;
            FloatySpaceBlock b2 = null;

            var tileTypeField = bType.GetField("tileType", BindingFlags.NonPublic | BindingFlags.Instance);
            List<StaticMover> staticMovers = (List<StaticMover>)original.GetType().GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original);
            char tileTypeChar = (char)tileTypeField.GetValue(original);

            if (tileTypeChar == '1')
            {
                Audio.Play("event:/game/general/wall_break_dirt", Entity.Position);
            }
            else if (tileTypeChar == '3')
            {
                Audio.Play("event:/game/general/wall_break_ice", Entity.Position);
            }
            else if (tileTypeChar == '9')
            {
                Audio.Play("event:/game/general/wall_break_wood", Entity.Position);
            }
            else
            {
                Audio.Play("event:/game/general/wall_break_stone", Entity.Position);
            }
            if (b1Width >= 8 && b1Height >= 8)
            {
                b1 = new FloatySpaceBlock(b1Pos, b1Width, b1Height, tileTypeChar, false);
                Scene.Add(b1);
                PropertyInfo pi = bType.GetProperty("Scene");
            }

            if (b2Width >= 8 && b2Height >= 8)
            {
                b2 = new FloatySpaceBlock(b2Pos, b2Width, b2Height, tileTypeChar, false);
                Scene.Add(b2);
            }
            List<FloatySpaceBlock> group = original.Group;
            if (!original.MasterOfGroup)
            {
                FloatySpaceBlock master = (FloatySpaceBlock)bType.GetField("master", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original);
                group = master.Group;
            }

            //disassemble group
            if (group.Count > 1) { 
                foreach (FloatySpaceBlock block in group)
                {
                    if (block == original) continue;
                    if (masterCuttingList.Contains(block)) continue;
                    Scene.Add(new FloatySpaceBlock(block.Position, block.Width, block.Height, tileTypeChar, false));
                    Scene.Remove(block);
                    masterCuttingList.Add(block);
                }

            }

            foreach (StaticMover mover in staticMovers)
            {
                HandleStaticMover(Scene, Direction, Entity, original, b1, b2, mover, 8);
            }

            AddParticles(
                original.Position,
                new Vector2(original.Width, original.Height),
                Calc.HexToColor("444444"));
            masterCuttingList.Add(original);
            Scene.Remove(original);
            sliceStartPositions.Remove(original);
        }

        private void SliceBounceBlock(BounceBlock original)
        {
            Vector2[] resultArray = CalcCuts(original.Position, new Vector2(original.Width, original.Height), Entity.Center, Direction, cutSize);

            Vector2 b1Pos = resultArray[0];
            Vector2 b2Pos = resultArray[1];
            int b1Width = (int)resultArray[2].X;
            int b1Height = (int)resultArray[2].Y;

            int b2Width = (int)resultArray[3].X;
            int b2Height = (int)resultArray[3].Y;

            BounceBlock sjb1 = null;
            BounceBlock sjb2 = null;

            Type bType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.BounceBlock", true, true);

            float respawnTimer = (float)bType.GetField("respawnTimer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
            string state = (string)bType.GetField("state", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original).ToString();

            masterCuttingList.Add(original);
            Scene.Remove(original);
            sliceStartPositions.Remove(original);
            if (respawnTimer > 0 || state == "Broken")
            {
                return;
            }
            if (b1Width >= 16 && b1Height >= 16 && original.CollideRect(new Rectangle((int)b1Pos.X, (int)b1Pos.Y, b1Width, b1Height))) Scene.Add(sjb1 = new BounceBlock(b1Pos, b1Width, b1Height));
            if (b2Width >= 16 && b2Height >= 16 && original.CollideRect(new Rectangle((int)b2Pos.X, (int)b2Pos.Y, b2Width, b2Height))) Scene.Add(sjb2 = new BounceBlock(b2Pos, b2Width, b2Height));

            if (Session.CoreModes.Cold == SceneAs<Level>().CoreMode)
            {
                AddParticles(original.Position, new Vector2(original.Width, original.Height), Calc.HexToColor("53cee6"));
            } 
            else
            {
                Vector2 range = new Vector2(original.Width, original.Height);
                int numParticles = (int)(range.X * range.Y) / 10; //proportional to the area to cover
                level.ParticlesFG.Emit(Cuttable.paperScraps, numParticles / 4, original.Position + new Vector2(range.X / 2, range.Y / 2), new Vector2(range.X / 2, range.Y / 2), Calc.HexToColor("f3570e"));
                level.ParticlesFG.Emit(Cuttable.paperScraps, 3 * numParticles / 4, original.Position + new Vector2(range.X / 2, range.Y / 2), new Vector2(range.X / 2, range.Y / 2), Calc.HexToColor("16152b"));
            }
        }

        private void SliceDashBlock(DashBlock dashBlock)
        {
            dashBlock.Break(Entity.Position, Direction, true);
            masterCuttingList.Add(dashBlock);
            sliceStartPositions.Remove(dashBlock);
        }

        private void SliceStarJumpBlock(StarJumpBlock original)
        {
            Vector2[] resultArray = CalcCuts(original.Position, new Vector2(original.Width, original.Height), Entity.Center, Direction, cutSize);

            Vector2 b1Pos = resultArray[0];
            Vector2 b2Pos = resultArray[1];
            int b1Width = (int)resultArray[2].X;
            int b1Height = (int)resultArray[2].Y;

            int b2Width = (int)resultArray[3].X;
            int b2Height = (int)resultArray[3].Y;

            StarJumpBlock sjb1 = null;
            StarJumpBlock sjb2 = null;

            Type bType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.StarJumpBlock", true, true);
            bool sinks = (bool) bType.GetField("sinks", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);

            masterCuttingList.Add(original);
            Scene.Remove(original);
            sliceStartPositions.Remove(original);

            AddParticles(original.Position, new Vector2(original.Width, original.Height), Calc.HexToColor("FFFFFF"));
            if (b1Width >= 8 && b1Height >= 8 && original.CollideRect(new Rectangle((int)b1Pos.X, (int)b1Pos.Y, b1Width, b1Height))) Scene.Add(sjb1 = new StarJumpBlock(b1Pos, b1Width, b1Height, sinks));
            if (b2Width >= 8 && b2Height >= 8 && original.CollideRect(new Rectangle((int)b2Pos.X, (int)b2Pos.Y, b2Width, b2Height))) Scene.Add(sjb2 = new StarJumpBlock(b2Pos, b2Width, b2Height, sinks));
        }

        private void SliceDreamBlock(DreamBlock original)
        {
            Vector2[] resultArray = CalcCuts(original.Position, new Vector2(original.Width, original.Height), Entity.Center, Direction, cutSize);

            Vector2 db1Pos = resultArray[0];
            Vector2 db2Pos = resultArray[1];
            int db1Width = (int)resultArray[2].X;
            int db1Height = (int)resultArray[2].Y;

            int db2Width = (int)resultArray[3].X;
            int db2Height = (int)resultArray[3].Y;

            DreamBlock d1 = null;
            DreamBlock d2 = null;

            if (db1Width >= 8 && db1Height >= 8 && original.CollideRect(new Rectangle((int)db1Pos.X, (int)db1Pos.Y, db1Width, db1Height))) Scene.Add(d1 = new DreamBlock(db1Pos, db1Width, db1Height, null, false, false));
            if (db2Width >= 8 && db2Height >= 8 && original.CollideRect(new Rectangle((int)db2Pos.X, (int)db2Pos.Y, db2Width, db2Height))) Scene.Add(d2 = new DreamBlock(db2Pos, db2Width, db2Height, null, false, false));

            List<StaticMover> staticMovers = (List<StaticMover>)
                FakeAssembly.GetFakeEntryAssembly().
                GetType("Celeste.Solid", true, true).
                GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).
                GetValue(original);
            foreach (StaticMover mover in staticMovers)
            {
                HandleStaticMover(Scene, Direction, Entity, original, d1, d2, mover, 8);
            }
            masterCuttingList.Add(original);
            Scene.Remove(original);
            sliceStartPositions.Remove(original);
            AddParticles(original.Position, new Vector2(original.Width, original.Height), Calc.HexToColor("000000"));
        }

        private void SliceKevin(CrushBlock original)
        {
            if (!Scene.Contains(original))
            {
                sliceStartPositions.Remove(original);
                return;
            }

            //get private fields
            Type cbType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.CrushBlock", true, true);
            bool canMoveVertically = (bool)cbType?.GetField("canMoveVertically", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original);
            bool canMoveHorizontally = (bool)cbType?.GetField("canMoveHorizontally", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original);
            bool chillOut = (bool)cbType.GetField("chillOut", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original);

            var returnStack = cbType.GetField("returnStack", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original);
            List<StaticMover> staticMovers = (List<StaticMover>)cbType.GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original);
            SoundSource soundSource = (SoundSource) cbType.GetField("currentMoveLoopSfx", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original);
            soundSource?.Stop();
            var newReturnStack1 = Activator.CreateInstance(returnStack.GetType(), returnStack);
            var newReturnStack2 = Activator.CreateInstance(returnStack.GetType(), returnStack);

            Vector2 crushDir = (Vector2)cbType?.GetField("crushDir", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original);

            //Process private fields
            CrushBlock.Axes axii = (canMoveVertically && canMoveHorizontally) ? CrushBlock.Axes.Both : canMoveVertically ? CrushBlock.Axes.Vertical : CrushBlock.Axes.Horizontal;
            cbType.GetField("level", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(original, level);
            Vector2[] resultArray = CalcCuts(original.Position, new Vector2(original.Width, original.Height), Entity.Center, Direction, cutSize);
            Vector2 cb1Pos = Vector2Int(resultArray[0]);
            Vector2 cb2Pos = Vector2Int(resultArray[1]);
            int cb1Width = (int)resultArray[2].X;
            int cb1Height = (int)resultArray[2].Y;

            int cb2Width = (int)resultArray[3].X;
            int cb2Height = (int)resultArray[3].Y;

            //create cloned crushblocks + set data

            CrushBlock cb1 = null;
            bool cb1Added;
            if (cb1Added = (cb1Width >= 24 && cb1Height >= 24 && original.CollideRect(new Rectangle((int)cb1Pos.X, (int)cb1Pos.Y, cb1Width, cb1Height))))
            {
                cb1 = new CrushBlock(cb1Pos, cb1Width, cb1Height, axii, chillOut);
                cbType.GetField("returnStack", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(cb1, newReturnStack1);
                Scene.Add(cb1);
                intermediateFrameActivation.Add(cb1);
            }

            CrushBlock cb2 = null;
            bool cb2Added;
            if (cb2Added = (cb2Width >= 24 && cb2Height >= 24 && original.CollideRect(new Rectangle((int) cb2Pos.X, (int) cb2Pos.Y, cb2Width, cb2Height))))
            {
                cb2 = new CrushBlock(cb2Pos, cb2Width, cb2Height, axii, chillOut);
                cbType.GetField("returnStack", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(cb2, newReturnStack2);
                Scene.Add(cb2);
                intermediateFrameActivation.Add(cb2);
            }

            foreach (StaticMover mover in staticMovers)
            {
                HandleStaticMover(Scene, Direction, Entity, original, cb1, cb2, mover, 8);
            }
            masterCuttingList.Add(original);
            Scene.Remove(original);
            AddParticles(original.Position, new Vector2(original.Width, original.Height), Calc.HexToColor("62222b"));

            sliceStartPositions.Remove(original);
        }

        //flips the slicer to face the new cutdirection. Can also supply a new directional collider.
        public void Flip(Vector2 cutDirection, Collider directionalCollider = null)
        {
            slicingCollider = directionalCollider;
            Direction = cutDirection;
        }

        private static Vector2 Vector2Int(Vector2 vector2)
        {
            return new Vector2((int)Math.Round(vector2.X), (int)Math.Round(vector2.Y));
        }

        private void SliceMoveBlock(MoveBlock original)
        {

            Type bType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.MoveBlock", true, true);
            Type stateType = bType.GetNestedType("MovementState", BindingFlags.NonPublic);
            string[] names = stateType.GetEnumNames();
            string stateName = bType.GetField("state", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original).ToString();
            if (stateName == "Breaking")
            {
                sliceStartPositions.Remove(original);
                return;
            }

            Vector2[] resultArray = CalcCuts(original.Position, new Vector2(original.Width, original.Height), Entity.Center, Direction, cutSize);
            Vector2 b1Pos = resultArray[0];
            Vector2 b2Pos = resultArray[1];
            int b1Width = (int)resultArray[2].X;
            int b1Height = (int)resultArray[2].Y;

            int b2Width = (int)resultArray[3].X;
            int b2Height = (int)resultArray[3].Y;

            MoveBlock mb1 = null;
            MoveBlock mb2 = null;
            List<StaticMover> staticMovers = (List<StaticMover>)bType.GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original);
            AddParticles(
            original.Position,
                new Vector2(original.Width, original.Height),
                Calc.HexToColor("111111")); 
            Audio.Play("event:/game/general/wall_break_stone", original.Position);
            bool triggered = (bool)bType.GetField("triggered", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original);
            MoveBlock.Directions direction = (MoveBlock.Directions) bType.GetField("direction", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original);
            bool canSteer = (bool) bType.GetField("canSteer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original);
            bool fast = (bool) bType.GetField("fast", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original);
            Vector2 startPosition = (Vector2)bType.GetField("startPosition", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original);

            bool vertical = direction == MoveBlock.Directions.Up || direction == MoveBlock.Directions.Down;

            masterCuttingList.Add(original);
            sliceStartPositions.Remove(original);
            Scene.Remove(original);

            if (b1Width >= 16 && b1Height >= 16)
            {
                mb1 = new MoveBlock(b1Pos, b1Width, b1Height, direction, canSteer, fast);
                Scene.Add(mb1);
                intermediateFrameActivation.Add(mb1);
                bType.GetField("startPosition", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mb1, vertical ? new Vector2(b1Pos.X, startPosition.Y) : new Vector2(startPosition.X, b1Pos.Y));
            }
            if (b2Width >= 16 && b2Height >= 16)
            {
                mb2 = new MoveBlock(b2Pos, b2Width, b2Height, direction, canSteer, fast);
                Scene.Add(mb2);
                intermediateFrameActivation.Add(mb2);
                bType.GetField("startPosition", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mb2, vertical ? new Vector2(b2Pos.X, startPosition.Y) : new Vector2(startPosition.X, b2Pos.Y));
            }

            foreach (StaticMover mover in staticMovers)
            {
                HandleStaticMover(Scene, Direction, Entity, original, mb1, mb2, mover, 8);
            }
        }

        private void SliceFallingBlock(FallingBlock original)
        {
            Vector2[] resultArray = CalcCuts(original.Position, new Vector2(original.Width, original.Height), Entity.Center, Direction, cutSize);
            Vector2 cb1Pos = resultArray[0];
            Vector2 cb2Pos = resultArray[1];
            int cb1Width = (int)resultArray[2].X;
            int cb1Height = (int)resultArray[2].Y;

            int cb2Width = (int)resultArray[3].X;
            int cb2Height = (int)resultArray[3].Y;

            var tileTypeField = original.GetType().GetField("TileType", BindingFlags.NonPublic | BindingFlags.Instance);
            List<StaticMover> staticMovers = (List<StaticMover>)original.GetType().GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(original);
            char tileTypeChar = (char)tileTypeField.GetValue(original);

            if (tileTypeChar == '1')
            {
                Audio.Play("event:/game/general/wall_break_dirt", Entity.Position);
            }
            else if (tileTypeChar == '3')
            {
                Audio.Play("event:/game/general/wall_break_ice", Entity.Position);
            }
            else if (tileTypeChar == '9')
            {
                Audio.Play("event:/game/general/wall_break_wood", Entity.Position);
            }
            else
            {
                Audio.Play("event:/game/general/wall_break_stone", Entity.Position);
            }
            FallingBlock fb1 = null;
            FallingBlock fb2 = null;
            if (cb1Width >= 8 && cb1Height >= 8)
            {
                fb1 = new FallingBlock(cb1Pos, tileTypeChar, cb1Width, cb1Height, false, false, true);
                Scene.Add(fb1);
                fb1.Triggered = true;
                fb1.FallDelay = 0;
            }
            if (cb2Width >= 8 && cb2Height >= 8)
            {
                fb2 = new FallingBlock(cb2Pos, tileTypeChar, cb2Width, cb2Height, false, false, true);
                Scene.Add(fb2);
                fb2.Triggered = true;
                fb2.FallDelay = 0;
            }
            foreach (StaticMover mover in staticMovers)
            {
                HandleStaticMover(Scene, Direction, Entity, original, fb1, fb2, mover, 8);
            }
            AddParticles(
                original.Position,
                new Vector2(original.Width, original.Height),
                Calc.HexToColor("444444"));
            masterCuttingList.Add(original);
            Scene.Remove(original);
            sliceStartPositions.Remove(original);
        }

        private static void HandleBottomSideSpikes(
    Scene Scene,
    Vector2 Direction,
    Solid cb1,
    Solid cb2,
    StaticMover mover)
        {
            Entity spikes = mover.Entity;
            bool cb1Added = cb1 != null;
            bool cb2Added = cb2 != null;

            //figure out case type, split 
            //case 1: horizontal slicer (direction.x != 0), slicer going through top side. Cb1 will not be added. Solution: Reattach
            //case 2: horizontal slicer (direction.x != 0), slicer going through middle. both will be added. Solution: Reattach
            //case 2: horizontal slicer (direction.x != 0), slicer going through bottom side. Cb2 will not be added. Solution: Delete

            //case 4: vertical slicer (direction.x == 0), slicer going through left side. Cb1 will not be added.
            //case 5: vertical slicer (direction.x == 0), slicer going through middle. both will be added.
            //case 6: vertical slicer (direction.x == 0), slicer going through right side. Cb2 will not be added.

            //case 2 & 1 are identical (ignore)

            //case 1-3: horizontal slicer

            if (Direction.X != 0)
            {
                //case 1
                if (cb1Added && !cb2Added)
                {
                    Scene.Remove(spikes);
                    return;
                }
                //case 2 & 3
                else if (cb2Added)
                {
                    ReattachStaticMover(cb2, mover, Orientation.Down);
                }
            }
            else
            {
                //case 4: left side cut
                if (!cb1Added && cb2Added)
                {
                    //find out if our spikes are completely taken out (right of spikes is less than right of cut / left of cb2
                    if (cb2.Left >= spikes.Right)
                    {
                        Scene.Remove(spikes);
                        return;
                    }
                    //find out if spikes were untouched (left of spikes is greater than right of cut / left of cb2)
                    if (spikes.Left >= cb2.Left)
                    {
                        //do nothing? reattachment needed at best
                        ReattachStaticMover(cb2, mover, Orientation.Down);
                        return;
                    }
                    //spikes were cut. spikes should be remade.
                    Entity newSpikes = GetNewStaticMoverEntity(Scene, mover.Entity, cb2.BottomLeft, (int)(spikes.Right - cb2.Left), Orientation.Down);
                    if (newSpikes != null)
                        Scene.Add(newSpikes);
                    Scene.Remove(spikes);
                    return;
                }
                //case 5: vertical middle cut
                else if (cb1Added && cb2Added)
                {
                    //find if the spikes were completely removed
                    if (cb1.Right <= spikes.Left && cb2.Left >= spikes.Right)
                    {
                        Scene.Remove(spikes);
                        return;
                    }
                    //find out if spikes were unharmed
                    if (cb1.Right >= spikes.Right || cb2.Left <= spikes.Left)
                    {
                        ReattachStaticMover(cb1, mover, Orientation.Down);
                        return;
                    }
                    //then they intersect the hole. up to two spikes can be added in this case
                    if (cb1.Right > spikes.Left)
                    {
                        Entity newSpikes = GetNewStaticMoverEntity(Scene, mover.Entity, spikes.TopLeft, (int)(cb1.Right - spikes.Left), Orientation.Down);
                        if (newSpikes != null)
                            Scene.Add(newSpikes);
                    }
                    if (cb2.Left < spikes.Right)
                    {
                        Entity newSpikes = GetNewStaticMoverEntity(Scene, mover.Entity, cb2.BottomLeft, (int)(spikes.Right - cb2.Left), Orientation.Down);
                        if (newSpikes != null)
                            Scene.Add(newSpikes);
                    }
                    Scene.Remove(spikes);
                    return;
                }
                //case 6
                else if (cb1Added && !cb2Added)
                {
                    //find out if our spikes are completely taken out (top of spikes is less than top of cut / bottom of cb1
                    if (cb1.Right <= spikes.Left)
                    {
                        Scene.Remove(spikes);
                        return;
                    }
                    //find out if spikes were untouched (bottom of spikes is greater than top of cut / bottom of cb1)
                    if (spikes.Right <= cb1.Right)
                    {
                        //do nothing? reattachment needed at best
                        ReattachStaticMover(cb1, mover, Orientation.Down);
                        return;
                    }
                    //spikes were cut. spikes should be remade.
                    Entity newSpikes = GetNewStaticMoverEntity(Scene, mover.Entity, spikes.TopLeft, (int)(cb1.Right - spikes.Left), Orientation.Down);
                    if (newSpikes != null)
                        Scene.Add(newSpikes);
                    Scene.Remove(spikes);
                    return;
                }
            }
        }

        private static void HandleTopSideSpikes(
            Scene Scene,
            Vector2 Direction,
            Solid cb1,
            Solid cb2,
            StaticMover mover)
        {
            Entity spikes = mover.Entity;
            bool cb1Added = cb1 != null;
            bool cb2Added = cb2 != null;


            //figure out case type, split 
            //case 1: horizontal slicer (direction.x != 0), slicer going through top side. Cb1 will not be added. Solution: Delete
            //case 2: horizontal slicer (direction.x != 0), slicer going through middle. both will be added. Solution: Reattach
            //case 2: horizontal slicer (direction.x != 0), slicer going through bottom side. Cb2 will not be added. Solution: Reattach

            //case 4: vertical slicer (direction.x == 0), slicer going through left side. Cb1 will not be added.
            //case 5: vertical slicer (direction.x == 0), slicer going through middle. both will be added.
            //case 6: vertical slicer (direction.x == 0), slicer going through right side. Cb2 will not be added.

            //case 2 & 3 are identical (ignore)

            //case 1-3: horizontal slicer

            if (Direction.X != 0)
            {
                //case 1
                if (!cb1Added && cb2Added)
                {
                    Scene.Remove(spikes);
                    return;
                }
                //case 2 & 3
                else
                {
                    ReattachStaticMover(cb1, mover, Orientation.Up);
                }
            } else
            {
                //case 4: left side cut
                if (!cb1Added && cb2Added)
                {
                    //find out if our spikes are completely taken out (right of spikes is less than right of cut / left of cb2
                    if (cb2.Left >= spikes.Right)
                    {
                        Scene.Remove(spikes);
                        return;
                    }
                    //find out if spikes were untouched (left of spikes is greater than right of cut / left of cb2)
                    if (spikes.Left >= cb2.Left)
                    {
                        //do nothing? reattachment needed at best
                        ReattachStaticMover(cb2, mover, Orientation.Up);
                        return;
                    }
                    //spikes were cut. spikes should be remade.
                    Entity newSpikes = GetNewStaticMoverEntity(Scene, mover.Entity, cb2.TopLeft, (int)(spikes.Right - cb2.Left), Orientation.Up);
                    if (newSpikes != null)
                        Scene.Add(newSpikes);
                    Scene.Remove(spikes);
                    return;
                }
                //case 5: vertical middle cut
                else if (cb1Added && cb2Added)
                {
                    //find if the spikes were completely removed
                    if (cb1.Right <= spikes.Left && cb2.Left >= spikes.Right)
                    {
                        Scene.Remove(spikes);
                        return;
                    }
                    //find out if spikes were unharmed
                    if (cb1.Right >= spikes.Right || cb2.Left <= spikes.Left)
                    {
                        ReattachStaticMover(cb1, mover, Orientation.Up);
                        return;
                    }
                    //then they intersect the hole. up to two spikes can be added in this case
                    if (cb1.Right > spikes.Left)
                    {
                        Entity newSpikes = GetNewStaticMoverEntity(Scene, mover.Entity, spikes.BottomLeft, (int)(cb1.Right - spikes.Left), Orientation.Up);
                        if (newSpikes != null)
                            Scene.Add(newSpikes);
                    }
                    if (cb2.Left < spikes.Right)
                    {
                        Entity newSpikes = 
                        GetNewStaticMoverEntity(Scene, mover.Entity, cb2.TopLeft, (int)(spikes.Right - cb2.Left), Orientation.Up);
                        if (newSpikes != null)
                            Scene.Add(newSpikes);
                    }
                    Scene.Remove(spikes);
                    return;
                }
                //case 6
                else if (cb1Added && !cb2Added)
                {
                    //find out if our spikes are completely taken out (top of spikes is less than top of cut / bottom of cb1
                    if (cb1.Right <= spikes.Left)
                    {
                        Scene.Remove(spikes);
                        return;
                    }
                    //find out if spikes were untouched (bottom of spikes is greater than top of cut / bottom of cb1)
                    if (spikes.Right <= cb1.Right)
                    {
                        //do nothing? reattachment needed at best
                        ReattachStaticMover(cb1, mover, Orientation.Up);
                        return;
                    }
                    //spikes were cut. spikes should be remade.
                    Entity newSpikes =
                    GetNewStaticMoverEntity(Scene, mover.Entity, spikes.BottomLeft, (int)(cb1.Right - spikes.Left), Orientation.Up);
                    if (newSpikes != null)
                        Scene.Add(newSpikes);
                    Scene.Remove(spikes);
                    return;
                }
            }
        }

        private static void HandleLeftSideSpikes(
            Scene Scene, 
            Vector2 Direction, 
            Solid cb1, 
            Solid cb2, 
            StaticMover mover)
        {
            Entity spikes = mover.Entity;
            bool cb1Added = cb1 != null;
            bool cb2Added = cb2 != null;


            //figure out case type, split 
            //case 1: horizontal slicer (direction.x != 0), slicer going through top side. Cb1 will not be added.
            //case 2: horizontal slicer (direction.x != 0), slicer going through middle. both will be added.
            //case 2: horizontal slicer (direction.x != 0), slicer going through bottom side. Cb2 will not be added.

            //case 4: vertical slicer (direction.x == 0), slicer going through left side. Cb1 will not be added.
            //case 5: vertical slicer (direction.x == 0), slicer going through middle. both will be added.
            //case 6: vertical slicer (direction.x == 0), slicer going through right side. Cb2 will not be added.

            //case 5 and 6 are identical (ignore)

            //case 1-3: horizontal slicer
            if (Direction.X != 0)
            {
                //case 1 slicer going through top side.
                if (!cb1Added && cb2Added)
                {
                    //find out if our spikes are completely taken out (bottom of spikes is less than bottom of cut / top of cb2
                    if (cb2.Top >= spikes.Bottom)
                    {
                        Scene.Remove(spikes);
                        return;
                    }
                    //find out if spikes were untouched (top of spikes is greater than top of cut)
                    if (spikes.Top >= cb2.Top)
                    {
                        //do nothing? reattachment needed at best
                        ReattachStaticMover(cb2, mover, Orientation.Left);
                        return;
                    }
                    //spikes were cut. spikes should be remade.
                    Entity newSpikes = GetNewStaticMoverEntity(Scene, mover.Entity, cb2.TopLeft, (int)(spikes.Bottom - cb2.Top), Orientation.Left);
                    if (newSpikes != null)
                        Scene.Add(newSpikes);
                    Scene.Remove(spikes);
                    return;
                }

                //case 2 slicer going through middle.
                else if (cb1Added && cb2Added) //&& cb1Added is implied
                {
                    //find if the spikes were completely removed
                    if (cb1.Bottom <= spikes.Top && cb2.Top >= spikes.Bottom)
                    {
                        Scene.Remove(spikes);
                        return;
                    }
                    //find out if spikes were unharmed
                    if (cb1.Bottom >= spikes.Bottom || cb2.Top <= spikes.Top)
                    {
                        ReattachStaticMover(cb1, mover, Orientation.Left);
                        return;
                    }
                    //then they intersect the hole. up to two spikes can be added in this case
                    if (cb1.Bottom > spikes.Top)
                    {
                        Entity newSpikes = GetNewStaticMoverEntity(Scene, mover.Entity, spikes.TopRight, (int)(cb1.Bottom - spikes.Top), Orientation.Left);
                        if (newSpikes != null)
                            Scene.Add(newSpikes);
                    }
                    if (cb2.Top < spikes.Bottom)
                    {
                        Entity newSpikes =
                        GetNewStaticMoverEntity(Scene, mover.Entity, cb2.TopLeft, (int)(spikes.Bottom - cb2.Top), Orientation.Left);

                        if (newSpikes != null) Scene.Add(newSpikes);
                    }
                    Scene.Remove(spikes);
                    return;
                }
                //case 3 slicer going through bottom.
                else if (cb1Added && !cb2Added)
                {
                    //find out if our spikes are completely taken out (top of spikes is less than top of cut / bottom of cb1
                    if (cb1.Bottom <= spikes.Top)
                    {
                        Scene.Remove(spikes);
                        return;
                    }
                    //find out if spikes were untouched (bottom of spikes is greater than top of cut / bottom of cb1)
                    if (spikes.Bottom <= cb1.Bottom)
                    {
                        //do nothing? reattachment needed at best
                        ReattachStaticMover(cb1, mover, Orientation.Left);
                        return;
                    }
                    //spikes were cut. spikes should be remade.
                    Entity newSpikes = 
                    GetNewStaticMoverEntity(Scene, mover.Entity, spikes.TopRight, (int)(cb1.Bottom - spikes.Top), Orientation.Left);

                    if (newSpikes != null) Scene.Add(newSpikes);
                    Scene.Remove(spikes);
                    return;
                }
            } 

            else
            {
                //case 4
                if (!cb1Added)
                {
                    Scene.Remove(spikes);
                    return;
                }
                //case 5 and 6 (ignore (reattach?))
                else
                {
                    ReattachStaticMover(cb1, mover, Orientation.Left);
                }
            }

        }

        private static void HandleRightSideSpikes(
            Scene Scene,
            Vector2 Direction,
            Solid cb1,
            Solid cb2,
            StaticMover mover)
        {
            Entity spikes = mover.Entity;
            bool cb1Added = cb1 != null;
            bool cb2Added = cb2 != null;


            //figure out case type, split 
            //case 1: horizontal slicer (direction.x != 0), slicer going through top side. Cb1 will not be added.
            //case 2: horizontal slicer (direction.x != 0), slicer going through middle. both will be added.
            //case 2: horizontal slicer (direction.x != 0), slicer going through bottom side. Cb2 will not be added.

            //case 4: vertical slicer (direction.x == 0), slicer going through left side. Cb1 will not be added.
            //case 5: vertical slicer (direction.x == 0), slicer going through middle. both will be added.
            //case 6: vertical slicer (direction.x == 0), slicer going through right side. Cb2 will not be added.

            //case 4 and 5 are identical (ignore)

            //case 1-3: horizontal slicer
            if (Direction.X != 0)
            {
                //case 1 slicer going through top side.
                if (!cb1Added && cb2Added)
                {
                    //find out if our spikes are completely taken out (bottom of spikes is less than bottom of cut / top of cb2
                    if (cb2.Top >= spikes.Bottom)
                    {
                        Scene.Remove(spikes);
                        return;
                    }
                    //find out if spikes were untouched (top of spikes is greater than top of cut)
                    if (spikes.Top >= cb2.Top)
                    {
                        //do nothing? reattachment needed at best
                        ReattachStaticMover(cb2, mover, Orientation.Right);
                        return;
                    }
                    //spikes were cut. spikes should be remade.
                    Entity newSpikes = GetNewStaticMoverEntity(Scene, mover.Entity, cb2.TopRight, (int)(spikes.Bottom - cb2.Top), Orientation.Right);

                    if (newSpikes != null)
                        Scene.Add(newSpikes);
                    Scene.Remove(spikes);
                    return;
                }

                //case 2 slicer going through middle.
                else if (cb1Added && cb2Added) //&& cb1Added is implied
                {
                    //find if the spikes were completely removed
                    if (cb1.Bottom <= spikes.Top && cb2.Top >= spikes.Bottom)
                    {
                        Scene.Remove(spikes);
                        return;
                    }
                    //find out if spikes were unharmed
                    if (cb1.Bottom >= spikes.Bottom || cb2.Top <= spikes.Top)
                    {
                        ReattachStaticMover(cb1, mover, Orientation.Right);
                        return;
                    }
                    //then they intersect the hole. up to two spikes can be added in this case
                    if (cb1.Bottom >= spikes.Top)
                    {
                        Entity newSpikes =
                        GetNewStaticMoverEntity(Scene, mover.Entity, spikes.TopLeft, (int)(cb1.Bottom - spikes.Top), Orientation.Right);
                        if (newSpikes != null) Scene.Add(newSpikes);
                    }
                    if (cb2.Top < spikes.Bottom)
                    {
                        Entity newSpikes =
                        GetNewStaticMoverEntity(Scene, mover.Entity, cb2.TopRight, (int)(spikes.Bottom - cb2.Top), Orientation.Right);
                        if (newSpikes != null) Scene.Add(newSpikes);
                    }
                    Scene.Remove(spikes);
                    return;
                }
                //case 3 slicer going through bottom.
                else if (cb1Added && !cb2Added)
                {
                    //find out if our spikes are completely taken out (top of spikes is less than top of cut / bottom of cb1
                    if (cb1.Bottom <= spikes.Top)
                    {
                        Scene.Remove(spikes);
                        return;
                    }
                    //find out if spikes were untouched (bottom of spikes is greater than top of cut / bottom of cb1)
                    if (spikes.Bottom <= cb1.Bottom)
                    {
                        //do nothing? reattachment needed at best
                        ReattachStaticMover(cb1, mover, Orientation.Right);
                        return;
                    }
                    //spikes were cut. spikes should be remade.
                    Entity newSpikes = GetNewStaticMoverEntity(Scene, mover.Entity, spikes.TopLeft, (int)(cb1.Bottom - spikes.Top), Orientation.Right);
                    if (newSpikes != null) Scene.Add(newSpikes);
                    Scene.Remove(spikes);
                    return;
                }
            }

            else
            {
                //case 6
                if (!cb2Added)
                {
                    Scene.Remove(spikes);
                    return;
                }
                //case 4 and 5 (ignore (reattach?))
                else
                {
                    ReattachStaticMover(cb2, mover, Orientation.Right);
                }
            }

        }

        private static Entity GetNewStaticMoverEntity(Scene scene, Entity entity, Vector2 position, int length, Orientation orientation)
        {
            if (entity is KnifeSpikes ks)
            {
                Type spikesType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.Spikes", true, true);
                string overrideType = (string)spikesType?.GetField("overrideType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ks);
                
                switch (orientation)
                {
                    case Orientation.Right:

                        return new KnifeSpikes(position, length, Spikes.Directions.Right, overrideType, ks.sliceOnImpact);

                    case Orientation.Left:
                        return new KnifeSpikes(position, length, Spikes.Directions.Left, overrideType, ks.sliceOnImpact);
                    case Orientation.Up:
                        return new KnifeSpikes(position, length, Spikes.Directions.Up, overrideType, ks.sliceOnImpact);
                    case Orientation.Down:
                        return new KnifeSpikes(position, length, Spikes.Directions.Down, overrideType, ks.sliceOnImpact);

                }
            } else if (entity is Spikes spikes)
            {
                Type spikesType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.Spikes", true, true);
                string overrideType = (string)spikesType?.GetField("overrideType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(spikes);
                switch (orientation)
                {
                    case Orientation.Right:
                        return new Spikes(position, length, Spikes.Directions.Right, overrideType);

                    case Orientation.Left:
                        return new Spikes(position, length, Spikes.Directions.Left, overrideType);
                    case Orientation.Up:
                        return new Spikes(position, length, Spikes.Directions.Up, overrideType);
                    case Orientation.Down:
                        return new Spikes(position, length, Spikes.Directions.Down, overrideType);

                }
            } else if (entity is Spring spring)
            {
                if (length < 16) return null;
                switch (orientation)
                {
                    case Orientation.Right:
                        return new Spring(position, Spring.Orientations.WallRight, true);
                    case Orientation.Left:
                        return new Spring(position, Spring.Orientations.WallLeft, true);
                    case Orientation.Up:
                        return new Spring(position, Spring.Orientations.Floor, true);
                    case Orientation.Down:
                        return null;

                }
            }
            return null;
        }

        private enum Orientation
        {
            Up, Down, Left, Right
        }

        private static void ReattachStaticMover(Solid block, StaticMover mover, Orientation smDirection)
        {
            Type cbType = block.GetType();
            mover.Platform = block;
            Entity moverEntity = mover.Entity;
            List<StaticMover> staticMovers = (List<StaticMover>)cbType.GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(block);
            staticMovers.Add(mover);
            switch (smDirection)
            {
                case Orientation.Left:
                    moverEntity.Position = new Vector2(block.Left, moverEntity.Y);
                    break;
                case Orientation.Right:
                    moverEntity.Position = new Vector2(block.Right, moverEntity.Y);
                    break;
                case Orientation.Up:
                    moverEntity.Position = new Vector2(moverEntity.X, block.Top);
                    break;
                case Orientation.Down:
                    moverEntity.Position = new Vector2(moverEntity.X, block.Bottom);
                    break;
            }
        }

        //currently handles vanilla static movers (basically just spikes and springs). Welcome to hell.
        private static void HandleStaticMover(Scene Scene, Vector2 Direction, Entity Entity, Entity parent, Solid cb1, Solid cb2, StaticMover mover, int minLength, DynamicData dynData = null)
        {

            bool cb1Added = cb1 != null;
            bool cb2Added = cb2 != null;
            if (cb1Added || cb2Added) { 
                if (mover.Entity is Spikes)
                {
                    if ((mover.Entity as Spikes).Direction == Spikes.Directions.Left)
                    {
                        HandleLeftSideSpikes(Scene, Direction, cb1, cb2, mover);
                    }
                    else if ((mover.Entity as Spikes).Direction == Spikes.Directions.Right)
                    {
                        HandleRightSideSpikes(Scene, Direction, cb1, cb2, mover);
                    }
                    else if ((mover.Entity as Spikes).Direction == Spikes.Directions.Up)
                    {
                        HandleTopSideSpikes(Scene, Direction, cb1, cb2, mover);
                    }
                    else if ((mover.Entity as Spikes).Direction == Spikes.Directions.Down)
                    {
                        HandleBottomSideSpikes(Scene, Direction, cb1, cb2, mover);
                    }
                }
                if (mover.Entity is Spring spring)
                {
                    if (spring.Orientation == Spring.Orientations.WallRight)
                    {
                        HandleLeftSideSpikes(Scene, Direction, cb1, cb2, mover);
                    }
                    else if (spring.Orientation == Spring.Orientations.WallLeft)
                    {
                        HandleRightSideSpikes(Scene, Direction, cb1, cb2, mover);
                    }
                    else if (spring.Orientation == Spring.Orientations.Floor)
                    {
                        HandleTopSideSpikes(Scene, Direction, cb1, cb2, mover);
                    } else
                    {
                        Scene.Remove(mover.Entity);
                    }
                }

            } else
            {
                Scene.Remove(mover.Entity);
            }

            
        }

        private void AddParticles(Vector2 position, Vector2 range, Color color)
        {
            int numParticles = (int)(range.X * range.Y) / 10; //proportional to the area to cover
            level.ParticlesFG.Emit(Cuttable.paperScraps, numParticles, position + new Vector2(range.X / 2, range.Y / 2), new Vector2(range.X / 2, range.Y / 2), color);
        }

        //directional position is used with instant slicing because normally slicers depend on movement to slice
        private Vector2 GetDirectionalPosition()
        {
            if (Direction.X > 0)
            {
                return Entity.CenterLeft + new Vector2(directionalOffset, 0);
            }
            else if (Direction.X < 0)
            {
                return Entity.CenterRight + new Vector2(-directionalOffset, 0);
            }
            else if (Direction.Y > 0)
            {
                return Entity.TopCenter + new Vector2(0, directionalOffset);
            }
            else
            {
                return Entity.BottomCenter + new Vector2(0, -directionalOffset);
            }
        }

        public static Vector2[] CalcCuts(Vector2 blockPos, Vector2 blockSize, Vector2 cutPos, Vector2 cutDir, int gapWidth, int cutSize = 8)
        {
            Vector2 pos1, pos2, size1, size2;
            pos1 = pos2 = blockPos;
            size1 = new Vector2(blockSize.X, blockSize.Y);
            size2 = new Vector2(blockSize.X, blockSize.Y);

            if (cutDir.X != 0) //cut is horizontal
            {
                float delY = blockPos.Y + blockSize.Y - (cutPos.Y + gapWidth / 2);
                size2.Y = delY - Mod(delY, cutSize);
                pos2.Y = blockPos.Y + blockSize.Y - size2.Y;
                size1.Y = pos2.Y - pos1.Y - gapWidth;


                if (size1.Y >= blockSize.Y)
                {
                    size1.Y = blockSize.Y - 8;
                }
                if (size2.Y >= blockSize.Y)
                {
                    size2.Y = blockSize.Y - 8;
                }
            }
            else //cut vertical
            {
                float delX = blockPos.X + blockSize.X - (cutPos.X + gapWidth / 2);
                size2.X = delX - Mod(delX, cutSize);
                pos2.X = blockPos.X + blockSize.X - size2.X;
                size1.X = pos2.X - pos1.X - gapWidth;

                if (size1.X >= blockSize.X)
                {
                    size1.X = blockSize.X - 8;
                }
                if (size2.X >= blockSize.X)
                {
                    size2.X = blockSize.X - 8;
                }
            }

            return new Vector2[] { pos1, pos2, size1, size2 };
        }

        private static float Mod(float x, float m)
        {
            return (x % m + m) % m;
        }

        private static Dictionary<Type, Func<Entity, DynamicData, bool>> CustomSlicingActions = new Dictionary<Type, Func<Entity, DynamicData, bool>>();
        private static Dictionary<Type, Action<Entity, DynamicData>> CustomStaticHandlerActions = new Dictionary<Type, Action<Entity, DynamicData>>();
        private static Dictionary<Type, Action<Entity, DynamicData>> CustomSecondFrameActions = new Dictionary<Type, Action<Entity, DynamicData>>();
        public static void UnregisterSlicerAction(Type type)
        {
            CustomSlicingActions.Remove(type);
        }

        public static void RegisterSlicerAction(Type type, Func<Entity, DynamicData, bool> action)
        {
            CustomSlicingActions.Add(type, action);
        }

        public static void UnregisterSecondFrameSlicerAction(Type type)
        {
            CustomSecondFrameActions.Remove(type);
        }


        public static void RegisterSecondFrameSlicerAction(Type type, Action<Entity, DynamicData> action)
        {
            CustomSecondFrameActions.Add(type, action);
        }

        public static void UnregisterSlicerStaticHandler(Type type)
        {
            CustomStaticHandlerActions.Remove(type);
        }

        public static void RegisterSlicerStaticHandler(Type type, Action<Entity, DynamicData> action)
        {
            CustomStaticHandlerActions.Add(type, action);
        }

        public static void ModinteropHandleStaticMover(DynamicData dynData, Solid original, Solid cb1, Solid cb2, StaticMover mover, int minLength) 
        {
            Scene Scene = dynData.Get("Scene") as Scene;
            Vector2 Direction = (Vector2)dynData.Get("Direction");
            Entity Entity = dynData.Get("Entity") as Entity;
            HandleStaticMover(Scene, Direction, Entity, original, cb1, cb2, mover, minLength);
        }
        public override void DebugRender(Camera camera)
        {

            if (slicingCollider != null)
            {
                Vector2 positionHold = Entity.Position;
                Entity.Position = Entity.Position + ColliderOffset;
                Collider tempHold = Entity.Collider;
                Entity.Collider = slicingCollider;
                Draw.HollowRect(Entity.Collider, Color.Yellow);

                Entity.Position = positionHold;
                Entity.Collider = tempHold;
            }
        }
    }
}
