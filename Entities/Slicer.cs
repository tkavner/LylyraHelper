using Celeste.Mod.Helpers;
using Celeste.Mod.LylyraHelper.Entities;
using Celeste.Mod.LylyraHelper.Intefaces;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.LylyraHelper.Components
{
    //gives the entity this is added to the ability to "slice" (See Cutting Algorithm documentation). Entity must have a hitbox that is active.
    [Tracked(false)]
    public class Slicer : Component
    {

        private List<Entity> slicingEntities = new List<Entity>();
        //some entities take a frame advancement to activate properly (Such as Kevins). This list is for those entities.
        private List<Entity> secondFrameActivation = new List<Entity>();
        public Collider slicingCollider { get; set; }
        private Vector2 Direction;
        private int cutSize;
        private Dictionary<Entity, Vector2> sliceStartPositions = new Dictionary<Entity, Vector2>();
        private Level level;
        private int directionalOffset;
        private bool sliceOnImpact;
        private bool fragile;
        private Action entityCallback;

        public int entitiesCut { get; private set; }

        public Slicer(Vector2 Direction, int cutSize, Level level, int directionalOffset, Collider slicingCollider = null, bool active = true, bool sliceOnImpact = false, bool fragile = false) : base(active, false)
        {
            this.slicingCollider = slicingCollider;
            this.Direction = Direction;
            this.cutSize = cutSize;
            this.level = level;
            this.directionalOffset = directionalOffset;
            this.sliceOnImpact = sliceOnImpact;
            this.fragile = fragile;

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

            Collider tempHold = Entity.Collider;
            if (slicingCollider != null) Entity.Collider = slicingCollider;
            if(this.Entity.Collidable) CheckCollisions();
            
            Slice();
            Entity.Collider = tempHold;
        }

        private void CheckCollisions()
        {
            StaticMover sm = Entity.Get<StaticMover>();
            Vector2 Position = Entity.Position;
            //get dash paper, check if colliding, if so add to list (we need to check each type of DashPaper manually apparently for speed)
            foreach (Paper d in Scene.Tracker.GetEntities<DashPaper>())
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

            foreach (Paper d in base.Scene.Tracker.GetEntities<DeathNote>())
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

            foreach (Entity d in base.Scene.Entities)
            {
                if (d == Entity) continue;

                if (sm != null && sm.Entity != null && sm.Entity == d) continue;
                //custom entities from other mods
                if (d is ICuttable icut)
                {
                    if (!slicingEntities.Contains(d) && Entity.CollideCheck(d))
                    {
                        slicingEntities.Add(d);
                        sliceStartPositions.Add(d, Position);
                        continue;
                    }
                }
                else if (CustomSlicingActions.ContainsKey(d.GetType()))
                {
                    if (!slicingEntities.Contains(d) && Entity.CollideCheck(d))
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
                else if (d.GetType() == typeof(CrushBlock) || d.GetType() == typeof(FallingBlock) || d.GetType() == typeof(DreamBlock) || d is CrystalStaticSpinner)
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
                d.Awake(Scene);
                Type cbType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.CrushBlock", true, true);
                cbType.GetField("crushDir", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(d, -Direction);
                cbType.GetField("level", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(d, level);
                cbType.GetMethod("Attack", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(d, new object[] { -Direction });

                return true;
            });

            entitiesCut += slicingEntities.RemoveAll(d =>
            {
                if (!Scene.Contains(d))
                {
                    sliceStartPositions.Remove(d);
                    return true;
                }
                if (d is ICuttable icut)
                {
                    if ((!d.CollideCheck(Entity)) || collisionOverride || sliceOnImpact)
                    {
                        sliceStartPositions.TryGetValue(d, out Vector2 startPosition);
                        bool toReturn = icut.Cut(GetDirectionalPosition(), Direction, cutSize, startPosition);
                        sliceStartPositions.Remove(d);
                        return toReturn;
                    }
                    return false;
                }
                else if (((!d.CollideCheck(Entity)) || collisionOverride || sliceOnImpact) && Scene.Contains(d))
                {
                    if (d is Solid)
                    {
                        if (CustomSlicingActions.ContainsKey(d.GetType()))
                        {
                            CustomSlicingActions[d.GetType()].Invoke(d, new DynamicData(this));
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
            });
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
            Scene.Remove(original);
            sliceStartPositions.Remove(original);
            Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Entity.Center);
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

            Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Entity.Center);
            CrushBlock cb1 = null;
            bool cb1Added;
            if (cb1Added = (cb1Width >= 24 && cb1Height >= 24 && original.CollideRect(new Rectangle((int)cb1Pos.X, (int)cb1Pos.Y, cb1Width, cb1Height))))
            {
                cb1 = new CrushBlock(cb1Pos, cb1Width, cb1Height, axii, chillOut);
                cbType.GetField("returnStack", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(cb1, newReturnStack1);
                Scene.Add(cb1);
                secondFrameActivation.Add(cb1);
            }

            CrushBlock cb2 = null;
            bool cb2Added;
            if (cb2Added = (cb2Width >= 24 && cb2Height >= 24 && original.CollideRect(new Rectangle((int) cb2Pos.X, (int) cb2Pos.Y, cb2Width, cb2Height))))
            {
                cb2 = new CrushBlock(cb2Pos, cb2Width, cb2Height, axii, chillOut);
                cbType.GetField("returnStack", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(cb2, newReturnStack2);
                Scene.Add(cb2);
                secondFrameActivation.Add(cb2);
            }

            foreach (StaticMover mover in staticMovers)
            {
                HandleStaticMover(Scene, Direction, Entity, original, cb1, cb2, mover, 8);
            }
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
            Scene.Remove(original);
            sliceStartPositions.Remove(original);
        }

        //currently handles vanilla static movers (basically just spikes and springs)
        private static void HandleStaticMover(Scene Scene, Vector2 Direction, Entity Entity, Entity parent, Solid cb1, Solid cb2, StaticMover mover, int minLength, DynamicData dynData = null)
        {
            bool cb1Added = cb1 != null;
            bool cb2Added = cb2 != null;

            Vector2 cb1Pos = Vector2.Zero;
            Vector2 cb2Pos = Vector2.Zero;
            int cb1Width = 0;
            int cb1Height = 0;
            int cb2Width = 0;
            int cb2Height = 0;

            if (!cb1Added && !cb2Added)
            {

                Scene.Remove(mover.Entity);
                return;
            }

            if (cb1Added)
            {
                cb1Pos = cb1.Position;
                cb1Width = (int)cb1.Width;
                cb1Height = (int)cb1.Height;
            }
            if (cb2Added)
            {
                cb2Pos = cb2.Position;
                cb2Width = (int)cb2.Width;
                cb2Height = (int)cb2.Height;
            }


            float furthestLeft = cb1Added ? cb1Pos.X : cb2Pos.X;
            float furthestRight = cb2Added ? cb2Pos.X + cb2Width : cb1Pos.X + cb1Width;

            float furthestUp = cb1Added ? cb1Pos.Y : cb2Pos.Y;
            float furthestDown = cb2Added ? cb2Pos.Y + cb2Height : cb1Pos.Y + cb1Height;

            mover.Platform = null;
            Type cbType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.Solid", true, true);
            if (CustomStaticHandlerActions.ContainsKey(mover.Entity.GetType()))
            {
                CustomStaticHandlerActions[mover.Entity.GetType()].Invoke(mover.Entity, dynData);
            }
            else if (mover.Entity is Spikes || mover.Entity is KnifeSpikes)
            {
                //destroy all parts of spikes that aren't connected anymore.
                //switch depending on direction
                Spikes spike = mover.Entity as Spikes;
                Type spikesType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.Spikes", true, true);
                string overrideType = (string)spikesType?.GetField("overrideType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(spike);
                bool useYCoordinates = (spike.Direction == Spikes.Directions.Left || spike.Direction == Spikes.Directions.Right) || Direction.X != 0;
                
                bool spikesOnCB1 = cb1Added && (spike.Y < cb1Pos.Y + cb1Height && useYCoordinates) || (spike.X < cb1Pos.X + cb1Width && !useYCoordinates); //check if spikes start before the hole to see if part of them should be on cb1
                bool spikesOnCB2 = cb2Added && (spike.Y + spike.Height > cb2Pos.Y && useYCoordinates) || (spike.X + spike.Width > cb2Pos.X && !useYCoordinates); //check if the spikes extend past the hole to see if part of them should be on cb2
                int grace = 5;
                if (Spikes.Directions.Left == spike.Direction && furthestLeft > spike.Position.X + grace)
                {
                    Scene.Remove(spike);
                    return;
                }
                else if (Spikes.Directions.Right == spike.Direction && furthestRight < spike.Position.X - grace)
                {
                    Scene.Remove(spike);
                    return;
                }
                else if (Spikes.Directions.Up == spike.Direction && furthestUp > spike.Position.Y + grace)
                {
                    Scene.Remove(spike);
                    return;
                }
                else if (Spikes.Directions.Down == spike.Direction && furthestDown < spike.Position.Y - grace)
                {
                    Scene.Remove(spike);
                    return;
                }
                if (!spikesOnCB1 && !spikesOnCB2)
                {
                    Scene.Remove(spike);
                    return;
                }
                switch (spike.Direction)
                {
                    case Spikes.Directions.Left:
                    case Spikes.Directions.Right:
                        //clipping logic: in the case of left spikes, compare to left side edge (cb1Pos.X), then check for a hole on left side (cb1Pos.Y + cb1Height) to the top of the spikes. 
                        //if cb1Pos.X < parent.Position.X then the spikes are no longer attached and should be removed
                        if ((cb1Pos.Y + cb1Height < spike.Y + spike.Height && cb1Added) || (cb2Pos.Y > spike.Y && cb2Pos.Y < spike.Y + spike.Height && cb2Added)) //then the spikes intersect the hole. 
                        {
                            if (spikesOnCB1)
                            {
                                float spikePosY = spike.Y;
                                float spikePosX = spike.X;
                                switch (spike.Direction)
                                {
                                    case Spikes.Directions.Left:
                                        spikePosY = spike.Y;
                                        spikePosX = cb1Pos.X;
                                        break;
                                    case Spikes.Directions.Right:
                                        spikePosY = spike.Y;
                                        spikePosX = cb1Pos.X + cb1Width;
                                        break;
                                }
                                int spikeHeight = (int)(cb1Pos.Y + cb1Height - spike.Y);
                                if (spikeHeight >= minLength)
                                {
                                    if (spike is KnifeSpikes)
                                    {
                                        Spikes newSpike1 = new KnifeSpikes(new Vector2(spikePosX, spikePosY), spikeHeight, spike.Direction, overrideType, (spike as KnifeSpikes).sliceOnImpact);
                                        Scene.Add(newSpike1);
                                    }
                                    else
                                    {
                                        Spikes newSpike1 = new Spikes(new Vector2(spikePosX, spikePosY), spikeHeight, spike.Direction, overrideType);
                                        Scene.Add(newSpike1);

                                    }
                                }
                            }
                            if (spikesOnCB2)
                            {
                                float spikePosY = cb2Pos.Y;
                                int spikeHeight = (int)(spike.Y + spike.Height - cb2Pos.Y);
                                float spikePosX = spike.X;
                                switch (spike.Direction)
                                {
                                    case Spikes.Directions.Left:
                                        spikePosY = cb2Pos.Y;
                                        spikePosX = cb2Pos.X;
                                        break;
                                    case Spikes.Directions.Right:
                                        spikePosY = cb2Pos.Y;
                                        spikePosX = cb2Pos.X + cb2Width;

                                        break;
                                }
                                if (spikeHeight >= minLength)
                                {
                                    if (spike is KnifeSpikes)
                                    {
                                        Spikes newSpike1 = new KnifeSpikes(new Vector2(spikePosX, spikePosY), spikeHeight, spike.Direction, overrideType, (spike as KnifeSpikes).sliceOnImpact);
                                        Scene.Add(newSpike1);

                                    }
                                    else
                                    {
                                        Spikes newSpike1 = new Spikes(new Vector2(spikePosX, spikePosY), spikeHeight, spike.Direction, overrideType);
                                        Scene.Add(newSpike1);


                                    }
                                }
                            }

                            Scene.Remove(spike);
                        }
                        else //this means spikes do not intersect the hole but are attached to a block still. Update Spike positions so they do not desync from their object as it respawns
                        {
                            if (spikesOnCB1)
                            {
                                mover.Platform = cb1;
                                List<StaticMover> staticMovers = (List<StaticMover>)cbType.GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(cb1);
                                staticMovers.Add(mover);
                                switch (spike.Direction)
                                {
                                    case Spikes.Directions.Left:
                                        spike.Position = new Vector2(cb1Pos.X, spike.Y);

                                        break;
                                    case Spikes.Directions.Right:
                                        spike.Position = new Vector2(cb1Pos.X + cb1Width, spike.Y);
                                        break;
                                }
                            }
                            if (spikesOnCB2)
                            {
                                mover.Platform = cb2;
                                List<StaticMover> staticMovers = (List<StaticMover>)cbType.GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(cb2);

                                staticMovers.Add(mover);
                                switch (spike.Direction)
                                {
                                    case Spikes.Directions.Left:
                                        spike.Position = new Vector2(cb2Pos.X, spike.Y);
                                        break;
                                    case Spikes.Directions.Right:
                                        spike.Position = new Vector2(cb2Pos.X + cb2Width, spike.Y);
                                        break;
                                }
                            }
                        }
                        break;

                    case Spikes.Directions.Up:
                    case Spikes.Directions.Down:
                        //compare to right side edge (cb1Pos.X + cb1Width), then check for a hole on right side (cb1Pos.Y + cb1Height)

                        if (cb1Pos.Y > parent.Position.Y && spike.Direction == Spikes.Directions.Up)
                        {
                            Scene.Remove(spike);
                            break;
                        }
                        else if ((cb2Pos.Y + cb2Height < parent.Position.Y + Entity.Height) && spike.Direction == Spikes.Directions.Down)
                        {
                            Scene.Remove(spike);
                            break;
                        }
                        else if (cb1Pos.X + cb1Width < spike.X + spike.Width) //then the spikes intersect the hole. check if the spikes extend past the hole (cb2Pos.Y)
                        {
                            Scene.Remove(spike);

                            if (spikesOnCB1)
                            {
                                float spikePosX = spike.X;
                                int spikeWidth = (int)(cb1Pos.X + cb1Width - spike.X);
                                float spikePosY = cb1Pos.Y;
                                int spikeHeight = (int)(spike.Y + spike.Height - cb1Pos.Y);
                                switch (spike.Direction)
                                {
                                    case Spikes.Directions.Up:
                                        spikePosY = cb1Pos.Y;
                                        spikePosX = cb1Pos.X;
                                        break;
                                    case Spikes.Directions.Down:
                                        spikePosY = cb1Pos.Y + cb2Height;
                                        spikePosX = cb1Pos.X;
                                        break;
                                }
                                if (spikeWidth >= minLength)
                                {
                                    if (spike is KnifeSpikes)
                                    {
                                        Spikes newSpike1 = new KnifeSpikes(new Vector2(spikePosX, spikePosY), spikeWidth, spike.Direction, overrideType, (spike as KnifeSpikes).sliceOnImpact);
                                        Scene.Add(newSpike1);
                                    }
                                    else
                                    {
                                        Spikes newSpike1 = new Spikes(new Vector2(spikePosX, spikePosY), spikeWidth, spike.Direction, overrideType);
                                        Scene.Add(newSpike1);
                                    }
                                }
                            }
                            if (spikesOnCB2)
                            {
                                float spikePosX = cb2Pos.X;
                                int spikeWidth = (int)(spike.X + spike.Width - cb2Pos.X);
                                float spikePosY = cb2Pos.Y;
                                int spikeHeight = (int)(spike.Y + spike.Height - cb2Pos.Y);
                                switch (spike.Direction)
                                {
                                    case Spikes.Directions.Up:
                                        spikePosY = cb2Pos.Y;
                                        spikePosX = cb2Pos.X;
                                        break;
                                    case Spikes.Directions.Down:
                                        spikePosY = cb2Pos.Y + cb2Height;
                                        spikePosX = cb2Pos.X;
                                        break;
                                }
                                if (spikeWidth >= minLength)
                                {

                                    if (spike is KnifeSpikes)
                                    {

                                        Spikes newSpike1 = new KnifeSpikes(new Vector2(spikePosX, spikePosY), spikeWidth, spike.Direction, overrideType, (spike as KnifeSpikes).sliceOnImpact);
                                        Scene.Add(newSpike1);

                                    }
                                    else
                                    {
                                        Spikes newSpike1 = new Spikes(new Vector2(spikePosX, spikePosY), spikeWidth, spike.Direction, overrideType);
                                        Scene.Add(newSpike1);
                                    }
                                }
                            }
                        }
                        else //this means spikes do not intersect the hole but are attached to a block still. Update Spike positions so they do not desync from their object as it respawns
                        {
                            if (spikesOnCB1)
                            {
                                mover.Platform = cb1;
                                List<StaticMover> staticMovers = (List<StaticMover>)cbType.GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(cb1);
                                staticMovers.Add(mover);
                                switch (spike.Direction)
                                {
                                    case Spikes.Directions.Up:
                                        spike.Position = new Vector2(spike.X, cb1Pos.Y);
                                        break;
                                    case Spikes.Directions.Down:
                                        spike.Position = new Vector2(spike.X, cb1Pos.Y + cb1Height);
                                        break;
                                }
                            }
                            else
                            {
                                mover.Platform = cb2;
                                List<StaticMover> staticMovers = (List<StaticMover>)cbType.GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(cb2);
                                staticMovers.Add(mover);
                                switch (spike.Direction)
                                {
                                    case Spikes.Directions.Up:
                                        spike.Position = new Vector2(spike.X, cb2Pos.Y);
                                        break;
                                    case Spikes.Directions.Down:
                                        spike.Position = new Vector2(spike.X, cb2Pos.Y + cb2Height);
                                        break;
                                }
                            }
                        }
                        break;
                }
            }
            else if (mover.Entity is Spring)
            {
                Spring spring = mover.Entity as Spring;
                if (Spring.Orientations.WallLeft == spring.Orientation && furthestLeft > spring.Position.X)
                {
                    Scene.Remove(spring);
                    return;
                }
                else if (Spring.Orientations.WallRight == spring.Orientation && furthestRight < spring.Position.X)
                {
                    Scene.Remove(spring);
                    return;
                }
                else if (Spring.Orientations.Floor == spring.Orientation && furthestUp > spring.Position.Y)
                {
                    Scene.Remove(spring);
                    return;
                }
                //if the spring intersects the hole, delete it, else change its attached item
                bool springOnCB1 = cb1Added && ((spring.X < cb1Pos.X + cb1Width && Direction.X != 0) || (spring.Y < cb1Pos.Y + cb1Height && Direction.X == 0)); //check if spikes start before the hole to see if part of them should be on cb1
                bool springOnCB2 = cb2Added && ((spring.X < cb2Pos.X && Direction.X != 0) || (spring.Y < cb2Pos.Y && Direction.X == 0)); //check if spikes start before the hole to see if part of them should be on cb1

                if (Direction.X == 0) //y (up/down) movement
                {
                    if (spring.CollideRect(new Rectangle((int)(cb1Pos.X + cb1Width), (int)(cb1Pos.Y - 1), (int)(cb2Pos.X - (cb1Pos.X + cb1Width)), cb1Height + 2)))
                    {
                        Scene.Remove(spring);
                        return;
                    } else
                    {
                        if (cb1Added && springOnCB1)
                        {
                            mover.Platform = cb1;
                            List<StaticMover> staticMovers = (List<StaticMover>)cbType.GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(cb1);

                            spring.Position = new Vector2(cb1.X, cb1.Y);
                            staticMovers.Add(mover);
                        }
                        else if (cb2Added && springOnCB2)
                        {
                            mover.Platform = cb2;
                            List<StaticMover> staticMovers = (List<StaticMover>)cbType.GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(cb2);

                            spring.Position = new Vector2(cb2.X, cb2.Y);
                            staticMovers.Add(mover);
                        }
                        else
                        {
                            Scene.Remove(spring);
                            return;
                        }
                    }
                }
                else
                {
                    if (spring.CollideRect(new Rectangle((int)(cb1Pos.X - 1), (int)(cb1Pos.Y + cb1Height), cb1Width + 2, (int)(cb2Pos.Y - (cb1Pos.Y + cb1Height)))))
                    {
                        Scene.Remove(spring);
                        return;
                    }
                    else
                    {
                        if (cb1Added && springOnCB1)
                        {
                            mover.Platform = cb1;
                            List<StaticMover> staticMovers = (List<StaticMover>)cbType.GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).
                                GetValue(cb1);
                            staticMovers.Add(mover);
                        }
                        else if (cb2Added && springOnCB2)
                        {
                            mover.Platform = cb2;
                            List<StaticMover> staticMovers = (List<StaticMover>)cbType.GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).
                                GetValue(cb2);
                            staticMovers.Add(mover);
                        }
                        else
                        {
                            Scene.Remove(spring);
                            return;
                        }
                    }
                }
            }
            /*else if (mover.Entity is TriggerSpikes) //trigger spikes, incredibly enough, do not inheret from spikes, and contain a private array of a private struct type. this is what we call "awful"
            {
                //destroy all parts of spikes that aren't connected anymore.
                //switch depending on direction
                TriggerSpikes spike = mover.Entity as TriggerSpikes;
                Type spikesType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.TriggerSpikes", true, true);

                TriggerSpikes.Directions spikeDirection = (TriggerSpikes.Directions)spikesType.GetField("direction", BindingFlags.NonPublic | BindingFlags.Instance).
                                GetValue(spike);
                bool horizontal = spikeDirection == TriggerSpikes.Directions.Left || spikeDirection == TriggerSpikes.Directions.Right;

                bool spikesOnCB1 = cb1Added && (spike.Y < cb1Pos.Y + cb1Height && horizontal) || (spike.X < cb1Pos.X + cb1Width && !horizontal); //check if spikes start before the hole to see if part of them should be on cb1
                bool spikesOnCB2 = cb2Added && (spike.Y + spike.Height > cb2Pos.Y && horizontal) || (spike.X + spike.Width > cb2Pos.Y && !horizontal); //check if the spikes extend past the hole to see if part of them should be on cb2


                switch (spikeDirection)
                {
                    case TriggerSpikes.Directions.Left:
                    case TriggerSpikes.Directions.Right:
                        //clipping logic: in the case of left spikes, compare to left side edge (cb1Pos.X), then check for a hole on left side (cb1Pos.Y + cb1Height) to the top of the spikes. 
                        //if cb1Pos.X < parent.Position.X then the spikes are no longer attached and should be removed
                        if ((cb1Pos.Y + cb1Height < spike.Y + spike.Height && cb1Added) || (cb2Pos.Y < spike.Y + spike.Height && cb2Added)) //then the spikes intersect the hole. 
                        {
                            Logger.Log(LogLevel.Error, "LylyraHelper", "blarg");
                            if (spikesOnCB1)
                            {
                                float spikePosY = spike.Y;
                                float spikePosX = spike.X;
                                switch (spikeDirection)
                                {
                                    case TriggerSpikes.Directions.Left:
                                        spikePosY = spike.Y;
                                        spikePosX = cb1Pos.X;
                                        break;
                                    case TriggerSpikes.Directions.Right:
                                        spikePosY = spike.Y;
                                        spikePosX = cb1Pos.X + cb1Width;
                                        break;
                                }
                                int spikeHeight = (int)(cb1Pos.Y + cb1Height - spike.Y);
                                if (spikeHeight >= minLength)
                                {
                                    TriggerSpikes newSpike1 = new TriggerSpikes(new Vector2(spikePosX, spikePosY), spikeHeight, spikeDirection);
                                    Scene.Add(newSpike1);
                                    Logger.Log(LogLevel.Error, "LylyraHelper", "blarg2");
                                }
                            }
                            if (spikesOnCB2)
                            {
                                float spikePosY = cb2Pos.Y;
                                int spikeHeight = (int)(spike.Y + spike.Height - cb2Pos.Y);
                                float spikePosX = spike.X;
                                switch (spikeDirection)
                                {
                                    case TriggerSpikes.Directions.Left:
                                        spikePosY = cb2Pos.Y;
                                        spikePosX = cb2Pos.X;
                                        break;
                                    case TriggerSpikes.Directions.Right:
                                        spikePosY = cb2Pos.Y;
                                        spikePosX = cb2Pos.X + cb2Width;

                                        break;
                                }
                                if (spikeHeight >= minLength)
                                {
                                    TriggerSpikes newSpike1 = new TriggerSpikes(new Vector2(spikePosX, spikePosY), spikeHeight, spikeDirection);
                                    Scene.Add(newSpike1);

                                    Logger.Log(LogLevel.Error, "LylyraHelper", "blarg3");

                                }
                            }

                            Scene.Remove(spike);
                        }
                        else //this means spikes do not intersect the hole but are attached to a block still. Update Spike positions so they do not desync from their object as it respawns
                        {
                            if (spikesOnCB1)
                            {
                                mover.Platform = cb1;
                                List<StaticMover> staticMovers = (List<StaticMover>)cbType.GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(cb1);
                                staticMovers.Add(mover);
                                switch (spikeDirection)
                                {
                                    case TriggerSpikes.Directions.Left:
                                        spike.Position = new Vector2(cb1Pos.X, spike.Y);

                                        break;
                                    case TriggerSpikes.Directions.Right:
                                        spike.Position = new Vector2(cb1Pos.X + cb1Width, spike.Y);
                                        break;
                                }
                            }
                            if (spikesOnCB2)
                            {
                                mover.Platform = cb2;
                                List<StaticMover> staticMovers = (List<StaticMover>)cbType.GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(cb2);

                                staticMovers.Add(mover);
                                switch (spikeDirection)
                                {
                                    case TriggerSpikes.Directions.Left:
                                        spike.Position = new Vector2(cb2Pos.X, spike.Y);
                                        break;
                                    case TriggerSpikes.Directions.Right:
                                        spike.Position = new Vector2(cb2Pos.X + cb2Width, spike.Y);
                                        break;
                                }
                            }
                        }
                        break;

                    case TriggerSpikes.Directions.Up:
                    case TriggerSpikes.Directions.Down:
                        //compare to right side edge (cb1Pos.X + cb1Width), then check for a hole on right side (cb1Pos.Y + cb1Height)

                        if (cb1Pos.Y > parent.Position.Y && spikeDirection == TriggerSpikes.Directions.Up)
                        {
                            Scene.Remove(spike);
                            break;
                        }
                        else if ((cb2Pos.Y + cb2Height < parent.Position.Y + Entity.Height) && spikeDirection == TriggerSpikes.Directions.Down)
                        {
                            Scene.Remove(spike);
                            break;
                        }
                        else if (cb1Pos.X + cb1Width < spike.X + spike.Width) //then the spikes intersect the hole. check if the spikes extend past the hole (cb2Pos.Y)
                        {
                            Scene.Remove(spike);

                            if (spikesOnCB1)
                            {
                                float spikePosX = spike.X;
                                int spikeWidth = (int)(cb1Pos.X + cb1Width - spike.X);
                                float spikePosY = cb1Pos.Y;
                                int spikeHeight = (int)(spike.Y + spike.Height - cb1Pos.Y);
                                switch (spikeDirection)
                                {
                                    case TriggerSpikes.Directions.Up:
                                        spikePosY = cb1Pos.Y;
                                        spikePosX = cb1Pos.X;
                                        break;
                                    case TriggerSpikes.Directions.Down:
                                        spikePosY = cb1Pos.Y + cb2Height;
                                        spikePosX = cb1Pos.X;
                                        break;
                                }
                                if (spikeWidth >= minLength)
                                {
                                        TriggerSpikes newSpike1 = new TriggerSpikes(new Vector2(spikePosX, spikePosY), spikeWidth, spikeDirection);
                                        Scene.Add(newSpike1);
                                }
                            }
                            if (spikesOnCB2)
                            {
                                float spikePosX = cb2Pos.X;
                                int spikeWidth = (int)(spike.X + spike.Width - cb2Pos.X);
                                float spikePosY = cb2Pos.Y;
                                int spikeHeight = (int)(spike.Y + spike.Height - cb2Pos.Y);
                                switch (spikeDirection)
                                {
                                    case TriggerSpikes.Directions.Up:
                                        spikePosY = cb2Pos.Y;
                                        spikePosX = cb2Pos.X;
                                        break;
                                    case TriggerSpikes.Directions.Down:
                                        spikePosY = cb2Pos.Y + cb2Height;
                                        spikePosX = cb2Pos.X;
                                        break;
                                }
                                if (spikeWidth >= minLength)
                                {

                                    TriggerSpikes newSpike1 = new TriggerSpikes(new Vector2(spikePosX, spikePosY), spikeWidth, spikeDirection);
                                    Scene.Add(newSpike1);
                                }
                            }
                        }
                        else //this means spikes do not intersect the hole but are attached to a block still. Update Spike positions so they do not desync from their object as it respawns
                        {
                            if (spikesOnCB1)
                            {
                                mover.Platform = cb1;
                                List<StaticMover> staticMovers = (List<StaticMover>)cbType.GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(cb1);
                                staticMovers.Add(mover);
                                switch (spikeDirection)
                                {
                                    case TriggerSpikes.Directions.Up:
                                        spike.Position = new Vector2(spike.X, cb1Pos.Y);
                                        break;
                                    case TriggerSpikes.Directions.Down:
                                        spike.Position = new Vector2(spike.X, cb1Pos.Y + cb1Height);
                                        break;
                                }
                            }
                            else
                            {
                                mover.Platform = cb2;
                                List<StaticMover> staticMovers = (List<StaticMover>)cbType.GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(cb2);
                                staticMovers.Add(mover);
                                switch (spikeDirection)
                                {
                                    case TriggerSpikes.Directions.Up:
                                        spike.Position = new Vector2(spike.X, cb2Pos.Y);
                                        break;
                                    case TriggerSpikes.Directions.Down:
                                        spike.Position = new Vector2(spike.X, cb2Pos.Y + cb2Height);
                                        break;
                                }
                            }
                        }
                        break;
                }
            }*/
        }

        private void AddParticles(Vector2 position, Vector2 range, Color color)
        {
            int numParticles = (int)(range.X * range.Y) / 10; //proportional to the area to cover
            level.ParticlesFG.Emit(Cuttable.paperScraps, numParticles, position + new Vector2(range.X / 2, range.Y / 2), new Vector2(range.X / 2, range.Y / 2), color);
        }

        private Vector2 GetDirectionalPosition()
        {
            if (Direction.X > 0)
            {
                return Entity.Center + new Vector2(directionalOffset, 0);
            }
            else if (Direction.X < 0)
            {
                return Entity.Center + new Vector2(-directionalOffset, 0);
            }
            else if (Direction.Y > 0)
            {
                return Entity.Center + new Vector2(0, directionalOffset);
            }
            else
            {
                return Entity.Center + new Vector2(0, -directionalOffset);
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

        

        private static Dictionary<Type, Action<Entity, DynamicData>> CustomSlicingActions = new Dictionary<Type, Action<Entity, DynamicData>>();
        private static Dictionary<Type, Action<Entity, DynamicData>> CustomStaticHandlerActions = new Dictionary<Type, Action<Entity, DynamicData>>();
        public static void UnregisterSlicerAction(Type type, Action<Entity, DynamicData> action)
        {
            CustomSlicingActions.Add(type, action);
        }

        public static void RegisterSlicerAction(Type type, Action<Entity, DynamicData> action)
        {
            CustomSlicingActions.Add(type, action);
        }

        public static void UnregisterSlicerStaticHandler(Type type, Action<Entity, DynamicData> action)
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
    }
}
