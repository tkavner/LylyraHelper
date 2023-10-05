using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Components.Sliceables
{
    public class AttachedSpikesSliceable : AttachedSliceableComponent
    {
        public AttachedSpikesSliceable() : base()
        {

        }

        public override Entity GetNewEntity(Scene scene, Entity original, Vector2 position, int desiredLength, string orientation)
        {
            Spikes spikes = original as Spikes;
            string overrideType = spikes.overrideType;
            switch (spikes.Direction)
            {
                case Spikes.Directions.Right:
                    return new Spikes(position, desiredLength, Spikes.Directions.Right, overrideType);
                case Spikes.Directions.Left:
                    return new Spikes(position, desiredLength, Spikes.Directions.Left, overrideType);
                case Spikes.Directions.Up:
                    return new Spikes(position, desiredLength, Spikes.Directions.Up, overrideType);
                case Spikes.Directions.Down:
                    return new Spikes(position, desiredLength, Spikes.Directions.Down, overrideType);

            }
            return null;
        }

        public override string GetOrientation(Entity entity)
        {
            return (entity as Spikes).Direction.ToString();
        }
    }
}
