using Celeste.Mod.LylyraHelper.Components;
using Celeste.Mod.LylyraHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceable.Attached
{
    public class AttachedKnifeSpikesSliceable : AttachedSliceableComponent
    {
        public override Entity GetNewEntity(Scene scene, Entity original, Vector2 position, int desiredLength, string orientation)
        {
            KnifeSpikes spikes = original as KnifeSpikes;
            string overrideType = spikes.overrideType;
            bool sliceOnImpact = spikes.sliceOnImpact;
            switch (spikes.Direction)
            {
                case Spikes.Directions.Right:
                    return new KnifeSpikes(position, desiredLength, Spikes.Directions.Right, overrideType, sliceOnImpact);
                case Spikes.Directions.Left:
                    return new KnifeSpikes(position, desiredLength, Spikes.Directions.Left, overrideType, sliceOnImpact);
                case Spikes.Directions.Up:
                    return new KnifeSpikes(position, desiredLength, Spikes.Directions.Up, overrideType, sliceOnImpact);
                case Spikes.Directions.Down:
                    return new KnifeSpikes(position, desiredLength, Spikes.Directions.Down, overrideType, sliceOnImpact);
            }
            return null;
        }

        public override string GetOrientation(Entity orientableEntity)
        {
            return (orientableEntity as Spikes).Direction.ToString();
        }
    }
}
