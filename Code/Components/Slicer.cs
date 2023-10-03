﻿using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Celeste.Mod.LylyraHelper.Components;
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
        public class SlicerSettings
        {
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

            public static SlicerSettings DefaultSettings
            {
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

        public List<SliceableComponent> slicingEntities = new List<SliceableComponent>();
        //some entities take a frame advancement to activate properly (Such as Kevins and MoveBlocks). This list is for those entities.
        public List<Entity> secondFrameActivation = new List<Entity>();
        public List<Entity> intermediateFrameActivation = new List<Entity>();
        public Collider slicingCollider { get; set; }
        public Vector2 Direction { get; private set; }
        public int CutSize { get; private set; }
        private Level level;
        public int directionalOffset { get; set; }

        private bool sliceOnImpact;
        private bool fragile;
        private Vector2 ColliderOffset;
        private Action entityCallback;

        //TODO: Rename "master cutting list"
        //master cutting list is a static list entities are added to after they are cut in the event by multiple slicers on the same frame. Realistically this list should never see more than
        //4 or 5 entities at a time
        public static List<Entity> masterRemovedList = new List<Entity>();
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
            this.CutSize = cutSize;
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
                lastPurge = Engine.FrameCounter;
                masterRemovedList.Clear();
            }


            Vector2 positionHold = Entity.Position;
            Entity.Position = Entity.Position + ColliderOffset;
            Collider tempHold = Entity.Collider;
            if (slicingCollider != null) Entity.Collider = slicingCollider;
            if (Entity.Collidable) CheckCollisions();
            Slice();
            Entity.Position = positionHold;
            Entity.Collider = tempHold;
            Visible = true;
        }

        private void CheckCollisions()
        {
            StaticMover sm = Entity.Get<StaticMover>();
            Vector2 Position = Entity.Position;

            foreach (SliceableComponent sliceable in Scene.Tracker.GetComponents<SliceableComponent>()) {
                Entity sliceableEntity = sliceable.Entity;
                if(sliceableEntity != null)
                {
                    if (!settings.CanSlice(sliceableEntity.GetType())) continue;
                    if (masterRemovedList.Contains(sliceableEntity)) continue;
                    if (sliceableEntity == Entity) continue;
                    if (sm != null && sm.Platform != null && sm.Platform == sliceableEntity) continue; //do not cut the thing we are attached to?
                    if (slicingEntities.Contains(sliceable)) continue;

                    if (Entity.CollideCheck(sliceableEntity))
                    {
                        slicingEntities.Add(sliceable);
                        sliceable.OnSliceStart(this);
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

            secondFrameActivation.RemoveAll(secondFrameEntity =>
            {
                if (CustomSecondFrameActions.TryGetValue(secondFrameEntity.GetType(), out Action<Entity, DynamicData> customAction))
                {
                    customAction.Invoke(secondFrameEntity, new DynamicData(this));
                    return true;
                }
                else if (secondFrameEntity is CrushBlock crushBlock)
                {
                    crushBlock.Awake(Scene);
                    crushBlock.crushDir = -Direction;
                    crushBlock.level = level;
                    crushBlock.Attack(-Direction);

                }
                else if (secondFrameEntity is MoveBlock moveBlock)
                {
                    moveBlock.triggered = true;
                    moveBlock.border.Visible = false;
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


            //new code

            //cut second frame entities

            //cut first frame entities

            //add first frame entities to 

            entitiesCut += slicingEntities.RemoveAll(d => 
            {
                if (FinishedCutting(d, collisionOverride))
                {
                    return true;
                }
                return false; 
            }
            );
        }
        //TODO: rename this method, its poorly named
        //this method activates the actual slicing of an entity into two pieces.
        private bool FinishedCutting(SliceableComponent sliceableComp, bool collisionOverride)
        {
            Entity sliceableEntity = sliceableComp.Entity;
            if (!Scene.Contains(sliceableEntity))
            {
                return true;
            }
            if (masterRemovedList.Contains(sliceableEntity))
            {
                return true;
            }
            if (collisionOverride || sliceOnImpact || !sliceableEntity.CollideCheck(Entity))
            {
                return Entity.Get<SliceableComponent>()?.Slice(this) ?? true;
            }


            //else this item should not be in the list because cutting it is not supported. Warn and have it removed.

            return false;
        }


        private void SliceBounceBlock(BounceBlock original)
        {
            Vector2[] resultArray = CalcCuts(original.Position, new Vector2(original.Width, original.Height), Entity.Center, Direction, CutSize);

            Vector2 b1Pos = resultArray[0];
            Vector2 b2Pos = resultArray[1];
            int b1Width = (int)resultArray[2].X;
            int b1Height = (int)resultArray[2].Y;

            int b2Width = (int)resultArray[3].X;
            int b2Height = (int)resultArray[3].Y;

            BounceBlock sjb1 = null;
            BounceBlock sjb2 = null;

            float respawnTimer = original.respawnTimer;
            BounceBlock.States state = original.state;

            masterRemovedList.Add(original);
            Scene.Remove(original);
            if (respawnTimer > 0 || state == BounceBlock.States.Broken)
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
            masterRemovedList.Add(dashBlock);
        }

        private void SliceStarJumpBlock(StarJumpBlock original)
        {
            Vector2[] resultArray = CalcCuts(original.Position, new Vector2(original.Width, original.Height), Entity.Center, Direction, CutSize);

            Vector2 b1Pos = resultArray[0];
            Vector2 b2Pos = resultArray[1];
            int b1Width = (int)resultArray[2].X;
            int b1Height = (int)resultArray[2].Y;

            int b2Width = (int)resultArray[3].X;
            int b2Height = (int)resultArray[3].Y;

            StarJumpBlock sjb1 = null;
            StarJumpBlock sjb2 = null;

            bool sinks = original.sinks;

            masterRemovedList.Add(original);
            Scene.Remove(original);

            AddParticles(original.Position, new Vector2(original.Width, original.Height), Calc.HexToColor("FFFFFF"));
            if (b1Width >= 8 && b1Height >= 8 && original.CollideRect(new Rectangle((int)b1Pos.X, (int)b1Pos.Y, b1Width, b1Height))) Scene.Add(sjb1 = new StarJumpBlock(b1Pos, b1Width, b1Height, sinks));
            if (b2Width >= 8 && b2Height >= 8 && original.CollideRect(new Rectangle((int)b2Pos.X, (int)b2Pos.Y, b2Width, b2Height))) Scene.Add(sjb2 = new StarJumpBlock(b2Pos, b2Width, b2Height, sinks));
        }

        private void SliceDreamBlock(DreamBlock original)
        {
            Vector2[] resultArray = CalcCuts(original.Position, new Vector2(original.Width, original.Height), Entity.Center, Direction, CutSize);

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

            List<StaticMover> staticMovers = original.staticMovers;
            foreach (StaticMover mover in staticMovers)
            {
                HandleStaticMover(Scene, Direction, d1, d2, mover);
            }
            masterRemovedList.Add(original);
            Scene.Remove(original);
            AddParticles(original.Position, new Vector2(original.Width, original.Height), Calc.HexToColor("000000"));
        }


        //flips the slicer to face the new cutdirection. Can also supply a new directional collider.
        public void Flip(Vector2 cutDirection, Collider directionalCollider = null)
        {
            slicingCollider = directionalCollider;
            Direction = cutDirection;
        }

        public static Vector2 Vector2Int(Vector2 vector2)
        {
            return new Vector2((int)Math.Round(vector2.X), (int)Math.Round(vector2.Y));
        }

        private void SliceMoveBlock(MoveBlock original)
        {

            if (original.state == MoveBlock.MovementState.Breaking)
            {
                return;
            }

            Vector2[] resultArray = CalcCuts(original.Position, new Vector2(original.Width, original.Height), Entity.Center, Direction, CutSize);
            Vector2 b1Pos = resultArray[0];
            Vector2 b2Pos = resultArray[1];
            int b1Width = (int)resultArray[2].X;
            int b1Height = (int)resultArray[2].Y;

            int b2Width = (int)resultArray[3].X;
            int b2Height = (int)resultArray[3].Y;

            MoveBlock mb1 = null;
            MoveBlock mb2 = null;
            List<StaticMover> staticMovers = original.staticMovers;
            AddParticles(
            original.Position,
                new Vector2(original.Width, original.Height),
                Calc.HexToColor("111111"));
            Audio.Play("event:/game/general/wall_break_stone", original.Position);
            MoveBlock.Directions direction = original.direction;
            bool canSteer = original.canSteer;
            bool fast = original.fast;
            Vector2 startPosition = original.startPosition;

            bool vertical = direction == MoveBlock.Directions.Up || direction == MoveBlock.Directions.Down;

            masterRemovedList.Add(original);
            Scene.Remove(original);

            if (b1Width >= 16 && b1Height >= 16)
            {
                mb1 = new MoveBlock(b1Pos, b1Width, b1Height, direction, canSteer, fast);
                Scene.Add(mb1);
                intermediateFrameActivation.Add(mb1);
                mb1.startPosition = vertical ? new Vector2(b1Pos.X, startPosition.Y) : new Vector2(startPosition.X, b1Pos.Y);
            }
            if (b2Width >= 16 && b2Height >= 16)
            {
                mb2 = new MoveBlock(b2Pos, b2Width, b2Height, direction, canSteer, fast);
                Scene.Add(mb2);
                intermediateFrameActivation.Add(mb2);
                mb2.startPosition = vertical ? new Vector2(b2Pos.X, startPosition.Y) : new Vector2(startPosition.X, b2Pos.Y);
            }

            foreach (StaticMover mover in staticMovers)
            {
                HandleStaticMover(Scene, Direction, mb1, mb2, mover);
            }
        }

        private void SliceFallingBlock(FallingBlock original)
        {
            Vector2[] resultArray = CalcCuts(original.Position, new Vector2(original.Width, original.Height), Entity.Center, Direction, CutSize);
            Vector2 cb1Pos = resultArray[0];
            Vector2 cb2Pos = resultArray[1];
            int cb1Width = (int)resultArray[2].X;
            int cb1Height = (int)resultArray[2].Y;

            int cb2Width = (int)resultArray[3].X;
            int cb2Height = (int)resultArray[3].Y;

            List<StaticMover> staticMovers = original.staticMovers;
            char tileTypeChar = original.TileType;

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
                HandleStaticMover(Scene, Direction, fb1, fb2, mover);
            }
            AddParticles(
                original.Position,
                new Vector2(original.Width, original.Height),
                Calc.HexToColor("444444"));
            masterRemovedList.Add(original);
            Scene.Remove(original);
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
            if (CustomStaticHandlerNewEntityActions.TryGetValue(entity.GetType(), out Func<Scene, Entity, Vector2, int, string, Entity> customAction))
            {
                return customAction.Invoke(scene, entity, position, length, orientation.ToString());
            }
            //vanilla entity handling
            else if (entity is KnifeSpikes ks)
            {
                Type spikesType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.Spikes", true, true);
                string overrideType = ks.overrideType;

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
            }
            else if (entity is Spikes spikes)
            {
                Type spikesType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.Spikes", true, true);
                string overrideType = spikes.overrideType;
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
            }
            else if (entity is Spring spring)
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
            List<StaticMover> staticMovers = block.staticMovers;
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
        //scene = current map
        //direction = direction of the cut
        //cb1/cb2 = child blocks spawned
        //mover = static mover attempting to be handled

        //orientation = orientation of the static mover's Entity, only used with custom slicing actions on
        public static void HandleStaticMover(Scene Scene, Vector2 Direction, Solid cb1, Solid cb2, StaticMover mover)
        {

            bool cb1Added = cb1 != null;
            bool cb2Added = cb2 != null;
            if (cb1Added || cb2Added)
            {
                //modded entity handling
                if (StaticHandlerOrientationFunctions.TryGetValue(mover.Entity.GetType(), out Func<StaticMover, Solid, Solid, string> orientationFunc))
                {
                    string orientation = orientationFunc(mover, cb1, cb2);
                    switch (orientation.ToLower())
                    {
                        case "left":
                            HandleLeftSideSpikes(Scene, Direction, cb1, cb2, mover);
                            break;
                        case "right":
                            HandleRightSideSpikes(Scene, Direction, cb1, cb2, mover);
                            break;
                        case "top":
                        case "up":
                            HandleTopSideSpikes(Scene, Direction, cb1, cb2, mover);
                            break;
                        case "bottom":
                        case "down":
                            HandleBottomSideSpikes(Scene, Direction, cb1, cb2, mover);
                            break;
                        default:
                            break;
                    }
                }
                else if (CustomNonorientableStaticHandlerActions.TryGetValue(mover.Entity.GetType(), out Action<Scene, StaticMover, Vector2, Solid, Solid> nonoriFunc))
                {
                    nonoriFunc(Scene, mover, Direction, cb1, cb2);
                }
                //END: modded static mover handling
                //vanilla entity handling
                else if (mover.Entity is Spikes)
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
                else if (mover.Entity is Spring spring)
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
                    }
                    else
                    {
                        Scene.Remove(mover.Entity);
                    }
                }

            }
            else
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

        public static Vector2[] CalcCuts(Solid blockToBeCut, Vector2 cutPosition, Vector2 cutDir, int gapWidth, int cutsize = 8)
        {
            return CalcCuts(blockToBeCut.Position, new Vector2(blockToBeCut.Width, blockToBeCut.Height), cutPosition, cutDir, gapWidth, cutsize);
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

        //dictionary of functions on how to slice various types of entities. should return whether or not slicing is complete. The entity to be sliced and slicer in the form of DynamicData are provided.
        private static Dictionary<Type, Func<Entity, DynamicData, bool>> CustomSlicingActions = new Dictionary<Type, Func<Entity, DynamicData, bool>>();
        //dictionary of actions on how to activate various entities on their second frame (if needed)
        private static Dictionary<Type, Action<Entity, DynamicData>> CustomSecondFrameActions = new Dictionary<Type, Action<Entity, DynamicData>>();
        //dictionary of functions describing how to spawn a new static mover entity of a given Type for a specific position and required length and orientation. The old entity's static mover will be provided for convenience.
        private static Dictionary<Type, Func<Scene, Entity, Vector2, int, string, Entity>> CustomStaticHandlerNewEntityActions = new Dictionary<Type, Func<Scene, Entity, Vector2, int, string, Entity>>();
        //dictionary of functions describing which way a type of orientable entity is facing. The entity to be sliced, the sub solids it is attached to are provided
        //return value should be a string describing which way it is facing ("left", "right, "up", "down")
        private static Dictionary<Type, Func<StaticMover, Solid, Solid, string>> StaticHandlerOrientationFunctions = new Dictionary<Type, Func<StaticMover, Solid, Solid, string>>();
        //dictionary for static movable entities actions that don't fit the standard orientable criterion. 
        //the original static mover entity, the cut direction, the child solids and current scene are given. No return value, just do the job for me in this case.
        //have fun. this is basically the "other" catagory of static movers
        private static Dictionary<Type, Action<Scene, StaticMover, Vector2, Solid, Solid>> CustomNonorientableStaticHandlerActions = new Dictionary<Type, Action<Scene, StaticMover, Vector2, Solid, Solid>>();
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
            CustomStaticHandlerNewEntityActions.Remove(type);
        }

        public static void RegisterSecondFrameSlicerAction(Type type, Action<Entity, DynamicData> action)
        {
            CustomSecondFrameActions.Add(type, action);
        }

        public static void UnregisterSlicerSMEntityFunction(Type type)
        {
            CustomStaticHandlerNewEntityActions.Remove(type);
            StaticHandlerOrientationFunctions.Remove(type);
        }

        public static void RegisterSlicerSMEntityFunction(
            Type type,
            Func<StaticMover, Solid, Solid, string> orientationFunction,
            Func<Scene, Entity, Vector2, int, string, Entity> newEntityFunction)
        {
            StaticHandlerOrientationFunctions.Add(type, orientationFunction);
            CustomStaticHandlerNewEntityActions.Add(type, newEntityFunction);
        }

        public static void ModinteropHandleStaticMover(Scene scene, Vector2 Direction, Solid cb1, Solid cb2, StaticMover mover)
        {
            HandleStaticMover(scene, Direction, cb1, cb2, mover);
        }

        public static void ModinteropHandleStaticMovers(Scene scene, Vector2 Direction, Solid cb1, Solid cb2, List<StaticMover> staticMovers)
        {
            foreach (StaticMover mover in staticMovers) ModinteropHandleStaticMover(scene, Direction, cb1, cb2, mover);
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