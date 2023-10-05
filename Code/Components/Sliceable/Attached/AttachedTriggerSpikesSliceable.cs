using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceable.Attached
{
    internal class AttachedTriggerSpikesSliceable : AttachedSliceableComponent
    {
        public override Entity GetNewEntity(Scene scene, Entity original, Vector2 position, int desiredLength, string orientation)
        {
            TriggerSpikes spikes = original as TriggerSpikes;
            TriggerSpikes.SpikeInfo[] triggerInfo = spikes.spikes;
            switch (spikes.direction)
            {
                case TriggerSpikes.Directions.Right:
                    TriggerSpikes replacement = new TriggerSpikes(position, desiredLength, TriggerSpikes.Directions.Right);
                    int start = (int) (position.X - original.Position.X) / 4;
                    int end = start + desiredLength / 4;
                    TriggerSpikes.SpikeInfo[] newInfo = new TriggerSpikes.SpikeInfo[end - start];
                    Array.Copy(triggerInfo, newInfo, newInfo.Length);
                    for (int i = 0; i < newInfo.Length; i++)
                    {
                        newInfo[i].Parent = replacement;
                    }
                    replacement.spikes = newInfo;
                    return replacement;
                case TriggerSpikes.Directions.Left:
                    return new TriggerSpikes(position, desiredLength, TriggerSpikes.Directions.Left);
                case TriggerSpikes.Directions.Up:
                    return new TriggerSpikes(position, desiredLength, TriggerSpikes.Directions.Up);
                case TriggerSpikes.Directions.Down:
                    return new TriggerSpikes(position, desiredLength, TriggerSpikes.Directions.Down);

            }
            return null;
        }

        public override string GetOrientation(Entity orientableEntity)
        {
            return (orientableEntity as TriggerSpikes).direction.ToString();
        }
    }
}