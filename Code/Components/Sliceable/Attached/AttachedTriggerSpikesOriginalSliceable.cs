using Celeste.Mod.Entities;
using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceable.Attached
{
    public class AttachedTriggerSpikesOriginalSliceable : AttachedSliceableComponent
    {
        public override Entity GetNewEntity(Scene scene, Entity original, Vector2 position, int desiredLength, string orientation)
        {
            TriggerSpikesOriginal spikes = original as TriggerSpikesOriginal;
            TriggerSpikesOriginal.SpikeInfo[] triggerInfo = spikes.spikes;
            TriggerSpikesOriginal replacement;
            int start, end;
            TriggerSpikesOriginal.SpikeInfo[] newInfo;
            switch (spikes.direction)
            {
                case TriggerSpikesOriginal.Directions.Right:
                    replacement = new TriggerSpikesOriginal(position, desiredLength, TriggerSpikesOriginal.Directions.Right, spikes.overrideType);
                    start = (int)(position.Y - original.Position.Y) / 8;
                    end = start + desiredLength / 8;
                    newInfo = new TriggerSpikesOriginal.SpikeInfo[end - start];
                    Array.Copy(triggerInfo, newInfo, newInfo.Length);
                    for (int i = 0; i < newInfo.Length; i++)
                    {
                        newInfo[i].Parent = replacement;
                    }
                    replacement.spikes = newInfo;
                    return replacement;
                case TriggerSpikesOriginal.Directions.Left:
                    replacement = new TriggerSpikesOriginal(position, desiredLength, TriggerSpikesOriginal.Directions.Left, spikes.overrideType);
                    start = (int)(position.Y - original.Position.Y) / 8;
                    end = start + desiredLength / 8;
                    newInfo = new TriggerSpikesOriginal.SpikeInfo[end - start];
                    Array.Copy(triggerInfo, newInfo, newInfo.Length);
                    for (int i = 0; i < newInfo.Length; i++)
                    {
                        newInfo[i].Parent = replacement;
                    }
                    replacement.spikes = newInfo;
                    return replacement;
                case TriggerSpikesOriginal.Directions.Up:
                    replacement = new TriggerSpikesOriginal(position, desiredLength, TriggerSpikesOriginal.Directions.Up, spikes.overrideType);
                    start = (int)(position.X - original.Position.X) / 8;
                    end = start + desiredLength / 8;
                    newInfo = new TriggerSpikesOriginal.SpikeInfo[end - start];
                    Array.Copy(triggerInfo, newInfo, newInfo.Length);
                    for (int i = 0; i < newInfo.Length; i++)
                    {
                        newInfo[i].Parent = replacement;
                    }
                    replacement.spikes = newInfo;
                    return replacement;
                case TriggerSpikesOriginal.Directions.Down:
                    replacement = new TriggerSpikesOriginal(position, desiredLength, TriggerSpikesOriginal.Directions.Down, spikes.overrideType);
                    start = (int)(position.X - original.Position.X) / 8;
                    end = start + desiredLength / 8;
                    newInfo = new TriggerSpikesOriginal.SpikeInfo[end - start];
                    Array.Copy(triggerInfo, newInfo, newInfo.Length);
                    for (int i = 0; i < newInfo.Length; i++)
                    {
                        newInfo[i].Parent = replacement;
                    }
                    replacement.spikes = newInfo;
                    return replacement;

            }
            return null;
        }

        public override string GetOrientation(Entity orientableEntity)
        {
            return (orientableEntity as TriggerSpikesOriginal).direction.ToString();
        }
        //temporary workaround for Added() while i figure out what to do there
        public static void Load()
        {
            On.Celeste.Mod.Entities.TriggerSpikesOriginal.Added += TriggerSpikes_Added;
        }

        private static void TriggerSpikes_Added(On.Celeste.Mod.Entities.TriggerSpikesOriginal.orig_Added orig, TriggerSpikesOriginal self, Scene scene)
        {
            TriggerSpikesOriginal.SpikeInfo[] origInfo = self.spikes;
            orig(self, scene);
            if (origInfo != null) self.spikes = origInfo;
        }

        public static void Unload()
        {
            On.Celeste.Mod.Entities.TriggerSpikesOriginal.Added -= TriggerSpikes_Added;
        }

    }
}