using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Entities
{

    [CustomEntity(new string[]
{
    "LylyraHelper/KnifeSpikesUp = LoadUp",
    "LylyraHelper/KnifeSpikesDown = LoadDown",
    "LylyraHelper/KnifeSpikesLeft = LoadLeft",
    "LylyraHelper/KnifeSpikesRight = LoadRight"
})]
    public class KnifeSpikes : Spikes
    {
        public bool sliceOnImpact { get; private set; }

        public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData data) => new KnifeSpikes(data, offset, Directions.Up);
        public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData data) => new KnifeSpikes(data, offset, Directions.Down);
        public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData data) => new KnifeSpikes(data, offset, Directions.Left);
        public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData data) => new KnifeSpikes(data, offset, Directions.Right);
        public KnifeSpikes(EntityData data, Vector2 offset, Directions dir) : 
            base(data, offset, dir)
        {
            sliceOnImpact = data.Bool("sliceOnImpact", false);
        }

        public KnifeSpikes(Vector2 position, int size, Directions direction, string type, bool sliceOnImpact) : base(position, size, direction, type)
        {
            this.sliceOnImpact = sliceOnImpact;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Slicer(VectorDirection(), (int) ((Direction == Directions.Up || Direction == Directions.Down) ? Width : Height), SceneAs<Level>(), 5, sliceOnImpact: sliceOnImpact));
        }

        private Vector2 VectorDirection()
        {
            switch (Direction)
            {
                case Directions.Right:
                    return Vector2.UnitX;
                case Directions.Left:
                    return -Vector2.UnitX;
                case Directions.Down:
                    return Vector2.UnitY;
                default:
                    return -Vector2.UnitY;
            }
        }
    }
}
