using Celeste.Mod.Helpers;
using Celeste.Mod.LylyraHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Components
{
    //gives the entity this is added to the ability to "slice" (See Cutting Algorithm documentation). Entity must have a hitbox that is active.
    public class Slicer : Component
    {

        private List<Entity> slicingEntities = new List<Entity>();
        //some entities take a frame advancement to activate properly (Such as Kevins). This list is for those entities.
        private List<Entity> secondFrameActivation = new List<Entity>();
        private Collider slicingCollider;
        private Vector2 Direction;
        private int cutSize;
        private Dictionary<Entity, Vector2> sliceStartPositions = new Dictionary<Entity, Vector2>();
        private Level level;
        private int directionalOffset;
        private bool sliceOnImpact;

        public int entitiesCut { get; private set; }

        public Slicer(Vector2 Direction, int cutSize, Level level, int directionalOffset, Collider slicingCollider = null, bool active = true, bool sliceOnImpact = false) : base(active, false)
        {
            this.slicingCollider = slicingCollider;
            this.Direction = Direction;
            this.cutSize = cutSize;
            this.level = level;
            this.directionalOffset = directionalOffset;
            this.sliceOnImpact = sliceOnImpact;

            if (CuttablePaper.paperScraps == null)
            {
                Chooser<MTexture> sourceChooser = new Chooser<MTexture>(
                    GFX.Game["particles/LylyraHelper/dashpapershard00"],
                    GFX.Game["particles/LylyraHelper/dashpapershard01"],
                    GFX.Game["particles/LylyraHelper/dashpapershard02"]);
                CuttablePaper.paperScraps = new ParticleType()
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
            CheckCollisions();

            Slice();
            Entity.Collider = tempHold;
        }

        private void CheckCollisions()
        {

            Vector2 Position = Entity.Position;
            //get dash paper, check if colliding, if so add to list (we need to check each type of DashPaper manually apparently for speed)
            foreach (CuttablePaper d in base.Scene.Tracker.GetEntities<DashPaper>())
            {
                if (d == Entity) continue;

                if (!slicingEntities.Contains(d))
                    if (d.CollidePaper(Entity))
                    {
                        slicingEntities.Add(d);
                        sliceStartPositions.Add(d, Position);
                    }
            }

            foreach (CuttablePaper d in base.Scene.Tracker.GetEntities<DeathNote>())
            {
                if (d == Entity) continue;

                if (!slicingEntities.Contains(d))
                    if (d.CollidePaper(Entity))
                    {
                        slicingEntities.Add(d);
                        sliceStartPositions.Add(d, Position);
                    }
            }
            //we can fix this later I guess? I'm not sure the best way to get all these entity types.
            foreach (DreamBlock d in base.Scene.Tracker.GetEntities<DreamBlock>())
            {
                if (d == Entity) continue;
                if (!slicingEntities.Contains(d) && d.CollideCheck(Entity))
                {
                    slicingEntities.Add(d);
                    sliceStartPositions.Add(d, Position);
                }
            }
            foreach (Solid d in base.Scene.Tracker.GetEntities<Solid>())
            {
                if (d == Entity) continue;

                if (d.GetType() == typeof(CrushBlock) || d.GetType() == typeof(FallingBlock))
                {

                    Logger.Log(LogLevel.Error, "awe11111", "" + slicingEntities.Count);
                    if (!slicingEntities.Contains(d) && d.CollideCheck(Entity))
                    {

                        Logger.Log(LogLevel.Error, "awefawef", "" + slicingEntities.Count);
                        slicingEntities.Add(d);
                        sliceStartPositions.Add(d, Position);
                    }
                }
                else
                {
                    /*if (d is SolidTiles)
                    {
                        if (d.CollideCheck(this))
                        {
                            breaking = true;
                        }
                    }*///reimplement on scissors
                }
            }


        }
        //welcome to hell. Basically all cutting requires a wild amount of differing requirements to cut in half. Have fun.
        public void Slice(bool collisionOverride = false)
        {
            Vector2 Position = Entity.Center;
            float Width = Entity.Width;
            float Height = Entity.Width;

            secondFrameActivation.RemoveAll(d =>
            {
                Type cbType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.CrushBlock", true, true);
                cbType.GetField("crushDir", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(d, -Direction);
                cbType.GetMethod("Attack", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(d, new object[] { -Direction });
                return true;
            });

            entitiesCut += slicingEntities.RemoveAll(d =>
            {
                if (!Scene.Contains(d))
                {
                    return true;
                }
                //paper
                if (d is CuttablePaper)
                {
                    CuttablePaper paper = d as CuttablePaper;
                    if (!paper.CollidePaper(Entity) || collisionOverride || sliceOnImpact)
                    {
                        sliceStartPositions.TryGetValue(d, out Vector2 startPosition);
                        bool toReturn = paper.Cut(GetDirectionalPosition(), Direction, cutSize, startPosition);
                        sliceStartPositions.Remove(d);
                        return toReturn;
                    }
                    return false;
                }
                if (d is DreamBlock)
                {
                    if ((!d.CollideCheck(Entity) || collisionOverride || sliceOnImpact) && Scene.Contains(d))
                    {
                        Vector2[] resultArray = CalcCuts(d.Position, new Vector2(d.Width, d.Height), Entity.Position, Direction, cutSize);

                        Vector2 db1Pos = resultArray[0];
                        Vector2 db2Pos = resultArray[1];
                        int db1Width = (int)resultArray[2].X;
                        int db1Height = (int)resultArray[2].Y;

                        int db2Width = (int)resultArray[3].X;
                        int db2Height = (int)resultArray[3].Y;
                        DreamBlock d1 = new DreamBlock(db1Pos, db1Width, db1Height, null, false, false);
                        DreamBlock d2 = new DreamBlock(db2Pos, db2Width, db2Height, null, false, false);
                        if (db1Width >= 8 && db1Height >= 8)
                        {

                            Scene.Add(d1);
                        }
                        if (db2Width >= 8 && db2Height >= 8)
                        {

                            Scene.Add(d2);
                        }
                        Scene.Remove(d);
                        sliceStartPositions.Remove(d);
                        Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Entity.Position);
                        Logger.Log(LogLevel.Error, "Lylyra", "Got this far");
                        AddParticles(d.Position, new Vector2(d.Width, d.Height), Calc.HexToColor("000000"));
                        return true;
                    }
                    return false;
                }
                if (d is CrushBlock)
                {
                    if ((!d.CollideCheck(Entity) || collisionOverride || sliceOnImpact))
                    {
                        if (!Scene.Contains(d))
                        {
                            sliceStartPositions.Remove(d);

                            return true;
                        }
                        //get private fields
                        Type cbType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.CrushBlock", true, true);
                        bool canMoveVertically = (bool)cbType?.GetField("canMoveVertically", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(d);
                        bool canMoveHorizontally = (bool)cbType?.GetField("canMoveHorizontally", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(d);
                        bool chillOut = (bool)cbType.GetField("chillOut", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(d);

                        var returnStack = cbType.GetField("returnStack", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(d);
                        List<StaticMover> staticMovers = (List<StaticMover>)cbType.GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(d);
                        var newReturnStack1 = Activator.CreateInstance(returnStack.GetType(), returnStack);
                        var newReturnStack2 = Activator.CreateInstance(returnStack.GetType(), returnStack);

                        Vector2 crushDir = (Vector2)cbType?.GetField("crushDir", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(d);

                        //Process private fields

                        CrushBlock.Axes axii = (canMoveVertically && canMoveHorizontally) ? CrushBlock.Axes.Both : canMoveVertically ? CrushBlock.Axes.Vertical : CrushBlock.Axes.Horizontal;

                        Vector2[] resultArray = CalcCuts(d.Position, new Vector2(d.Width, d.Height), Position, Direction, cutSize);
                        Vector2 cb1Pos = resultArray[0];
                        Vector2 cb2Pos = resultArray[1];
                        int cb1Width = (int)resultArray[2].X;
                        int cb1Height = (int)resultArray[2].Y;

                        int cb2Width = (int)resultArray[3].X;
                        int cb2Height = (int)resultArray[3].Y;

                        //create cloned crushblocks + set data
                        Scene.Remove(d);

                        Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
                        bool completelyRemoved = true;
                        if (cb1Width >= 24 && cb1Height >= 24)
                        {
                            CrushBlock cb1 = new CrushBlock(cb1Pos, cb1Width, cb1Height, axii, chillOut);
                            cbType.GetField("returnStack", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(cb1, newReturnStack1);
                            Scene.Add(cb1);
                            secondFrameActivation.Add(cb1);
                            completelyRemoved = false;
                        }
                        if (cb2Width >= 24 && cb2Height >= 24)
                        {
                            CrushBlock cb2 = new CrushBlock(cb2Pos, cb2Width, cb2Height, axii, chillOut);
                            cbType.GetField("returnStack", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(cb2, newReturnStack2);
                            Scene.Add(cb2);
                            secondFrameActivation.Add(cb2);
                            completelyRemoved = false;
                        }

                        foreach (StaticMover mover in staticMovers)
                        {
                            if (completelyRemoved)
                            {
                                Scene.Remove(mover.Entity);
                                continue;
                            }
                            mover.Platform = null;
                            if (mover.Entity is Spikes)
                            {
                                //destroy all parts of spikes that aren't connected anymore.
                                //switch depending on direction
                                Spikes spike = mover.Entity as Spikes;
                                switch (spike.Direction)
                                {
                                    case Spikes.Directions.Left:
                                    case Spikes.Directions.Right:
                                        //compare to left side edge (cb1Pos.X), then check for a hole on left side (cb1Pos.Y + cb1Height) to the top of the spikes
                                        if (cb1Pos.X < d.Position.X && spike.Direction == Spikes.Directions.Left)
                                        {
                                            Scene.Remove(spike);
                                        }
                                        else if (cb2Pos.X + Width > d.Position.X + Width && spike.Direction == Spikes.Directions.Right)
                                        {
                                            Scene.Remove(spike);
                                        }
                                        else if (cb1Pos.Y + cb1Height < spike.Y + spike.Height) //then the spikes intersect the hole. check if the spikes extend past the hole (cb2Pos.Y)
                                        {

                                            Scene.Remove(spike);

                                            bool spikesOnCB1 = spike.Y < cb1Pos.Y + cb1Height;
                                            bool spikesOnCB2 = spike.Y + spike.Height > cb2Pos.Y;
                                            if (spikesOnCB1)
                                            {
                                                float spikePosY = spike.Y;
                                                int spikeHeight = (int)(cb1Pos.Y + cb1Height - spike.Y);
                                                if (spikeHeight >= 24)
                                                {
                                                    Spikes newSpike1 = new Spikes(new Vector2(spike.X, spikePosY), spikeHeight, spike.Direction, "default");
                                                    Scene.Add(newSpike1);
                                                }
                                            }
                                            if (spikesOnCB2)
                                            {
                                                float spikePosY = cb2Pos.Y;
                                                int spikeHeight = (int)(spike.Y + spike.Height - cb2Pos.Y);
                                                if (spikeHeight >= 24)
                                                {
                                                    Spikes newSpike1 = new Spikes(new Vector2(spike.X, spikePosY), spikeHeight, spike.Direction, "default");
                                                    Scene.Add(newSpike1);
                                                }
                                            }

                                        }
                                        break;

                                    case Spikes.Directions.Up:
                                    case Spikes.Directions.Down:
                                        //compare to right side edge (cb1Pos.X + cb1Width), then check for a hole on right side (cb1Pos.Y + cb1Height)
                                        if (cb1Pos.Y > d.Position.Y)
                                        {
                                            Scene.Remove(spike);
                                        }
                                        else if (cb2Pos.Y + cb2Height < d.Position.Y + Height)
                                        {
                                            Scene.Remove(spike);
                                        }
                                        else if (cb1Pos.X + cb1Width < spike.X + spike.Width) //then the spikes intersect the hole. check if the spikes extend past the hole (cb2Pos.Y)
                                        {
                                            Scene.Remove(spike);

                                            bool spikesOnCB1 = spike.X < cb1Pos.X + cb1Width;
                                            bool spikesOnCB2 = spike.X + spike.Width > cb2Pos.X;
                                            if (spikesOnCB1)
                                            {
                                                float spikePosX = spike.X;
                                                int spikeWidth = (int)(cb1Pos.X + cb1Width - spike.X);
                                                if (spikeWidth >= 24)
                                                {

                                                    Spikes newSpike = new Spikes(new Vector2(spikePosX, spike.Y), spikeWidth, spike.Direction, "default");
                                                    Scene.Add(newSpike);
                                                }
                                            }
                                            if (spikesOnCB2)
                                            {
                                                float spikePosX = cb2Pos.X;
                                                int spikeWidth = (int)(spike.X + spike.Width - cb2Pos.X);
                                                if (spikeWidth >= 24)
                                                {
                                                    Spikes newSpike = new Spikes(new Vector2(spikePosX, spike.Y), spikeWidth, spike.Direction, "default");
                                                    Scene.Add(newSpike);
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        AddParticles(d.Position, new Vector2(d.Width, d.Height), Calc.HexToColor("62222b"));

                        sliceStartPositions.Remove(d);
                        return true;
                    }
                    return false;
                }
                if (d is FallingBlock)
                {
                    if ((!d.CollideCheck(Entity) || collisionOverride || sliceOnImpact))
                    {
                        Vector2[] resultArray = CalcCuts(d.Position, new Vector2(d.Width, d.Height), Position, Direction, cutSize);
                        Vector2 fb1Pos = resultArray[0];
                        Vector2 fb2Pos = resultArray[1];
                        int fb1Width = (int)resultArray[2].X;
                        int fb1Height = (int)resultArray[2].Y;

                        int fb2Width = (int)resultArray[3].X;
                        int fb2Height = (int)resultArray[3].Y;

                        var tileTypeField = d.GetType().GetField("TileType", BindingFlags.NonPublic | BindingFlags.Instance);
                        List<StaticMover> staticMovers = (List<StaticMover>)d.GetType().GetField("staticMovers", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(d);
                        char tileTypeChar = (char)tileTypeField.GetValue(d);

                        if (tileTypeChar == '1')
                        {
                            Audio.Play("event:/game/general/wall_break_dirt", Position);
                        }
                        else if (tileTypeChar == '3')
                        {
                            Audio.Play("event:/game/general/wall_break_ice", Position);
                        }
                        else if (tileTypeChar == '9')
                        {
                            Audio.Play("event:/game/general/wall_break_wood", Position);
                        }
                        else
                        {
                            Audio.Play("event:/game/general/wall_break_stone", Position);
                        }
                        bool completelyRemoved = true;
                        if (fb1Width >= 8 && fb1Height >= 8)
                        {
                            FallingBlock fb1 = new FallingBlock(fb1Pos, tileTypeChar, fb1Width, fb1Height, false, false, true);
                            Scene.Add(fb1);


                            completelyRemoved = false;
                            fb1.Triggered = true;
                            fb1.FallDelay = 0;
                        }
                        if (fb2Width >= 8 && fb2Height >= 8)
                        {
                            FallingBlock fb2 = new FallingBlock(fb2Pos, tileTypeChar, fb2Width, fb2Height, false, false, true);
                            Scene.Add(fb2);

                            completelyRemoved = false;
                            fb2.Triggered = true;
                            fb2.FallDelay = 0;
                        }
                        foreach (StaticMover mover in staticMovers)
                        {
                            if (completelyRemoved)
                            {
                                Scene.Remove(mover.Entity);
                                continue;
                            }
                            mover.Platform = null;
                            if (mover.Entity is Spikes)
                            {
                                //destroy all parts of spikes that aren't connected anymore.
                                //switch depending on direction
                                Spikes spike = mover.Entity as Spikes;
                                switch (spike.Direction)
                                {
                                    case Spikes.Directions.Left:
                                    case Spikes.Directions.Right:
                                        //compare to left side edge (cb1Pos.X), then check for a hole on left side (cb1Pos.Y + cb1Height) to the top of the spikes
                                        if (fb1Pos.X < d.Position.X && spike.Direction == Spikes.Directions.Left)
                                        {
                                            Scene.Remove(spike);
                                        }
                                        else if (fb2Pos.X + Width > d.Position.X + Width && spike.Direction == Spikes.Directions.Right)
                                        {
                                            Scene.Remove(spike);
                                        }
                                        else if (fb1Pos.Y + fb1Height < spike.Y + spike.Height) //then the spikes intersect the hole. check if the spikes extend past the hole (cb2Pos.Y)
                                        {

                                            Scene.Remove(spike);

                                            bool spikesOnCB1 = spike.Y < fb1Pos.Y + fb1Height;
                                            bool spikesOnCB2 = spike.Y + spike.Height > fb2Pos.Y;
                                            if (spikesOnCB1)
                                            {
                                                float spikePosY = spike.Y;
                                                int spikeHeight = (int)(fb1Pos.Y + fb1Height - spike.Y);
                                                if (spikeHeight >= 24)
                                                {
                                                    Spikes newSpike1 = new Spikes(new Vector2(spike.X, spikePosY), spikeHeight, spike.Direction, "default");
                                                    Scene.Add(newSpike1);
                                                }
                                            }
                                            if (spikesOnCB2)
                                            {
                                                float spikePosY = fb2Pos.Y;
                                                int spikeHeight = (int)(spike.Y + spike.Height - fb2Pos.Y);
                                                if (spikeHeight >= 24)
                                                {
                                                    Spikes newSpike1 = new Spikes(new Vector2(spike.X, spikePosY), spikeHeight, spike.Direction, "default");
                                                    Scene.Add(newSpike1);
                                                }
                                            }

                                        }
                                        break;

                                    case Spikes.Directions.Up:
                                    case Spikes.Directions.Down:
                                        //compare to right side edge (cb1Pos.X + cb1Width), then check for a hole on right side (cb1Pos.Y + cb1Height)
                                        if (fb1Pos.Y > d.Position.Y)
                                        {
                                            Scene.Remove(spike);
                                        }
                                        else if (fb2Pos.Y + fb2Height < d.Position.Y + Height)
                                        {
                                            Scene.Remove(spike);
                                        }
                                        else if (fb1Pos.X + fb1Width < spike.X + spike.Width) //then the spikes intersect the hole. check if the spikes extend past the hole (cb2Pos.Y)
                                        {
                                            Scene.Remove(spike);

                                            bool spikesOnCB1 = spike.X < fb1Pos.X + fb1Width;
                                            bool spikesOnCB2 = spike.X + spike.Width > fb2Pos.X;
                                            if (spikesOnCB1)
                                            {
                                                float spikePosX = spike.X;
                                                int spikeWidth = (int)(fb1Pos.X + fb1Width - spike.X);
                                                if (spikeWidth >= 24)
                                                {

                                                    Spikes newSpike = new Spikes(new Vector2(spikePosX, spike.Y), spikeWidth, spike.Direction, "default");
                                                    Scene.Add(newSpike);
                                                }
                                            }
                                            if (spikesOnCB2)
                                            {
                                                float spikePosX = fb2Pos.X;
                                                int spikeWidth = (int)(spike.X + spike.Width - fb2Pos.X);
                                                if (spikeWidth >= 24)
                                                {
                                                    Spikes newSpike = new Spikes(new Vector2(spikePosX, spike.Y), spikeWidth, spike.Direction, "default");
                                                    Scene.Add(newSpike);
                                                }

                                            }
                                        }
                                        break;

                                }
                            }
                        }
                        AddParticles(d.Position, new Vector2(d.Width, d.Height), Calc.HexToColor("444444"));
                        Scene.Remove(d);
                        sliceStartPositions.Remove(d);
                        return true;
                    }
                    return false;
                }
                //else this item should not be in the list because cutting it is not supported. Log as an error and have it removed.
                Logger.Log(LogLevel.Error, "LylyraHelper", String.Format("Slicer attempting to slice unsupported Type: {0}.", d.GetType().Name));
                return true;
            });
        }


        private void AddParticles(Vector2 position, Vector2 range, Color color)
        {
            int numParticles = (int)(range.X * range.Y) / 10; //proportional to the area to cover
            level.ParticlesFG.Emit(CuttablePaper.paperScraps, numParticles, position + new Vector2(range.X / 2, range.Y / 2), new Vector2(range.X / 2, range.Y / 2), color);

        }

        private Vector2 GetDirectionalPosition()
        {
            if (Direction.X > 0)
            {
                return Entity.Position + new Vector2(directionalOffset, 0);
            }
            else if (Direction.X < 0)
            {

                return Entity.Position + new Vector2(-directionalOffset, 0);
            }
            else if (Direction.Y > 0)
            {
                return Entity.Position + new Vector2(0, directionalOffset);
            }
            else
            {
                return Entity.Position + new Vector2(0, -directionalOffset);
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
            }
            else //cut vertical
            {
                float delX = blockPos.X + blockSize.X - (cutPos.X + gapWidth / 2);
                size2.X = delX - Mod(delX, cutSize);
                pos2.X = blockPos.X + blockSize.X - size2.X;
                size1.X = pos2.X - pos1.X - gapWidth;
            }

            return new Vector2[] { pos1, pos2, size1, size2 };
        }


        private static float Mod(float x, float m)
        {
            return (x % m + m) % m;
        }
    }
}
