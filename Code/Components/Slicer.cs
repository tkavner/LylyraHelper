using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Celeste.Mod.LylyraHelper.Code.Components.Sliceable;
using Celeste.Mod.LylyraHelper.Code.Components.Sliceable.Attached;
using Celeste.Mod.LylyraHelper.Code.Components.Sliceables;
using Celeste.Mod.LylyraHelper.Components;
using Celeste.Mod.LylyraHelper.Components.Sliceables;
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
            secondFrameActivation.RemoveAll(secondFrameEntity =>
            {

                secondFrameEntity.Get<SliceableComponent>().Activate(this);
                return true;
            });

            entitiesCut += slicingEntities.RemoveAll(d => 
            {
                if (FinishedCutting(d, collisionOverride))
                {
                    masterRemovedList.Add(d.Entity);
                    return true;
                }
                return false; 
            }
            );
        }

        //directional position is used with instant slicing because normally slicers depend on movement to slice
        public Vector2 GetDirectionalPosition()
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
                Entity[] children = sliceableComp.Slice(this);
                if (children != null)
                {
                    foreach (Entity child in children)
                    {
                        if (child != null)
                        {
                            secondFrameActivation.Add(child);
                        }
                    }
                }
                return true;
            }


            //else this item should not be in the list because cutting it is not supported. Warn and have it removed.

            return false;
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

        private static void HandleBottomSideMover(
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

        private static void HandleTopSideMover(
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

        private static void HandleLeftSideMover(
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

        private static void HandleRightSideMover(
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
            AttachedSliceableComponent comp = entity.Get<AttachedSliceableComponent>();

            if (comp != null)
            {
                return comp.GetNewEntity(scene, entity, position, length, orientation.ToString().ToLower());
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
        public static void HandleStaticMover(Scene scene, Vector2 direction, Solid cb1, Solid cb2, StaticMover mover)
        {

            bool cb1Added = cb1 != null;
            bool cb2Added = cb2 != null;
            if (cb1Added || cb2Added)
            {

                AttachedSliceableComponent comp = mover.Entity.Get<AttachedSliceableComponent>();
                if (comp != null)
                {
                    if (comp.isDIY()) comp.DIY(scene, mover, direction, cb1, cb2);
                    else
                    {
                        switch (comp.GetOrientation(mover.Entity).ToLower())
                        {
                            case "left":
                                HandleLeftSideMover(scene, direction, cb1, cb2, mover);
                                break;
                            case "right":
                                HandleRightSideMover(scene, direction, cb1, cb2, mover);
                                break;
                            case "top":
                            case "up":
                                HandleTopSideMover(scene, direction, cb1, cb2, mover);
                                break;
                            case "bottom":
                            case "down":
                                HandleBottomSideMover(scene, direction, cb1, cb2, mover);
                                break;
                            default:
                                break;
                        }
                    }
                } 
                else
                {
                    scene.Remove(mover.Entity);
                }

            } else
            {
                scene.Remove(mover.Entity);
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

        public class CustomSlicingActionHolder
        {  
            public Func<Entity, DynamicData, Entity[]> firstFrameSlice;
            public Action<Entity, DynamicData> secondFrameSlice;
            public Action<Entity, DynamicData> onSliceStart;
        }

        public class CustomAttachedSlicingActionHolder
        {
            public Func<Entity, string> getOrientation;
            public Func<Scene, Entity, Vector2, int, string, Entity> getNewEntity;
            public Action<Scene, StaticMover, Vector2, Solid, Solid> diy; //scene, mover in question, slicer direction, block1, block2

        }
        //dictionary of functions on how to slice various types of entities. should return whether or not slicing is complete. The entity to be sliced and slicer in the form of DynamicData are provided.
        private static Dictionary<Type, CustomSlicingActionHolder> CustomSlicingActions = new Dictionary<Type, CustomSlicingActionHolder>();

        private static Dictionary<Type, CustomSlicingActionHolder> CustomAttachedSlicingActions = new Dictionary<Type, CustomSlicingActionHolder>();
        public static void UnregisterSlicerAction(Type type)
        {
            CustomSlicingActions.Remove(type);
        }

        public static void RegisterSlicerAction(Type type, Dictionary<string, Delegate> actions)
        {
            bool contains = CustomSlicingActions.TryGetValue(type, out CustomSlicingActionHolder holder);
            if (holder == null) holder = new();
            foreach (string key in actions.Keys)
            {
                var action = actions[key];
                if (key == "slice")
                {
                    holder.firstFrameSlice = (Func<Entity, DynamicData, Entity[]>)action;
                }
                else if (key == "activate")
                {
                    holder.secondFrameSlice = (Action<Entity, DynamicData>)action;
                }
                else if (key == "onSliceStart")
                {
                    holder.onSliceStart = (Action<Entity, DynamicData>)action;
                }
            }
            if (!contains) CustomSlicingActions.Add(type, holder);
        }

        public static void UnregisterSlicerSMEntityFunction(Type type)
        {
            //CustomStaticHandlerNewEntityActions.Remove(type);
            //StaticHandlerOrientationFunctions.Remove(type);
        }

        public static void RegisterSlicerSMEntityFunction(
            Type type,
            Func<StaticMover, Solid, Solid, string> orientationFunction,
            Func<Scene, Entity, Vector2, int, string, Entity> newEntityFunction)
        {
            //StaticHandlerOrientationFunctions.Add(type, orientationFunction);
            //CustomStaticHandlerNewEntityActions.Add(type, newEntityFunction);
        }

        public static void ModinteropHandleStaticMover(Scene scene, Vector2 Direction, Solid cb1, Solid cb2, StaticMover mover)
        {
            HandleStaticMover(scene, Direction, cb1, cb2, mover);
        }

        public static void ModinteropHandleStaticMovers(Scene scene, Vector2 Direction, Solid cb1, Solid cb2, List<StaticMover> staticMovers)
        {
            foreach (StaticMover mover in staticMovers) ModinteropHandleStaticMover(scene, Direction, cb1, cb2, mover);
        }

        public static void Load()
        {
            On.Monocle.Entity.Awake += Entity_Awake;
        }

        public static void Unload()
        {
            On.Monocle.Entity.Awake -= Entity_Awake;
        }

        private static void Entity_Awake(On.Monocle.Entity.orig_Awake orig, Entity entity, Scene self)
        {
            //standard item handling
            if (CustomSlicingActions.TryGetValue(entity.GetType(), out var action))
            {
                entity.Add(new ModItemSliceableComponent(action));
            }
            //vanilla entity handling
            else if (entity is Booster)
            {
                entity.Add(new BoosterSliceableComponent(true, true));
            }
            else if (entity is BounceBlock)
            {
                entity.Add(new BounceBlockSliceableComponent(true, true));
            }
            else if (entity is CassetteBlock)
            {
                entity.Add(new CassetteBlockSliceableComponent(true, true));
            }
            else if (entity is CrushBlock)
            {
                entity.Add(new CrushBlockSliceableComponent(true, true));
            }
            else if (entity is CrystalStaticSpinner)
            {
                entity.Add(new CrystalStaticSpinnerSliceableComponent(true, true));
            }
            else if (entity is DashBlock)
            {
                entity.Add(new DashBlockSliceableComponent(true, true));
            }
            else if (entity is StarJumpBlock)
            {
                entity.Add(new StarJumpBlockSliceableComponent(true, true));
            }
            else if (entity is MoveBlock)
            {
                entity.Add(new MoveBlockSliceableComponent(true, true));
            }
            else if (entity is FloatySpaceBlock)
            {
                entity.Add(new FloatySpaceBlockSliceableComponent(true, true));
            }
            else if (entity is FallingBlock)
            {
                entity.Add(new FallingBlockSliceableComponent(true, true));
            }
            else if (entity is Paper)
            {
                entity.Add(new PaperSliceableComponent(true, true));
            }
            else { }

            //attached entity handling. seperate from normal entity handling. Items can be both.
            if (CustomAttachedSlicingActions.TryGetValue(entity.GetType(), out var attachedAction))
            {
                entity.Add(new ModItemSliceableComponent(attachedAction));
            }
            else if (entity is Spikes)
            {
                entity.Add(new AttachedSpikesSliceable());
            }
            else if (entity is TriggerSpikes)
            {
                entity.Add(new AttachedTriggerSpikesSliceable());
            }
            else if (entity is Spring)
            {
                entity.Add(new AttachedSpringSliceable());
            }
            else if (entity is KnifeSpikes)
            {
                entity.Add(new AttachedKnifeSpikesSliceable());
            }
            else { }
            orig(entity, self);
        }


    }
}
