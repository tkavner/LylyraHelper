using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceable.Attached
{
    public class AttachedTriggerSpikesSliceable : AttachedSliceableComponent
    {
        public override Entity GetNewEntity(Scene scene, Entity original, Vector2 position, int desiredLength, string orientation)
        {
            TriggerSpikes spikes = original as TriggerSpikes;
            TriggerSpikes.SpikeInfo[] triggerInfo = spikes.spikes;
            TriggerSpikes replacement;
            int start, end;
            TriggerSpikes.SpikeInfo[] newInfo;
            switch (spikes.direction)
            {
                case TriggerSpikes.Directions.Right:
                    replacement = new TriggerSpikes(position, desiredLength, TriggerSpikes.Directions.Right);
                    start = (int)(position.Y - original.Position.Y) / 4;
                    end = start + desiredLength / 4;
                    newInfo = new TriggerSpikes.SpikeInfo[end - start];
                    Array.Copy(triggerInfo, newInfo, newInfo.Length);
                    for (int i = 0; i < newInfo.Length; i++)
                    {
                        newInfo[i].Parent = replacement;
                    }
                    replacement.spikes = newInfo;
                    return replacement;
                case TriggerSpikes.Directions.Left:
                    replacement = new TriggerSpikes(position, desiredLength, TriggerSpikes.Directions.Left);
                    start = (int)(position.Y - original.Position.Y) / 4;
                    end = start + desiredLength / 4;
                    newInfo = new TriggerSpikes.SpikeInfo[end - start];
                    Array.Copy(triggerInfo, newInfo, newInfo.Length);
                    for (int i = 0; i < newInfo.Length; i++)
                    {
                        newInfo[i].Parent = replacement;
                    }
                    replacement.spikes = newInfo; 
                    return replacement;
                case TriggerSpikes.Directions.Up:
                    replacement = new TriggerSpikes(position, desiredLength, TriggerSpikes.Directions.Up);
                    start = (int)(position.X - original.Position.X) / 4;
                    end = start + desiredLength / 4;
                    newInfo = new TriggerSpikes.SpikeInfo[end - start];
                    Array.Copy(triggerInfo, newInfo, newInfo.Length);
                    for (int i = 0; i < newInfo.Length; i++)
                    {
                        newInfo[i].Parent = replacement;
                    }
                    replacement.spikes = newInfo;
                    return replacement;
                case TriggerSpikes.Directions.Down:
                    replacement = new TriggerSpikes(position, desiredLength, TriggerSpikes.Directions.Down);
                    start = (int)(position.X - original.Position.X) / 4;
                    end = start + desiredLength / 4;
                    newInfo = new TriggerSpikes.SpikeInfo[end - start];
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
            return (orientableEntity as TriggerSpikes).direction.ToString();
        }
        //temporary workaround for Added() while i figure out what to do there
        public static void Load()
        {
            On.Celeste.TriggerSpikes.Added += TriggerSpikes_Added;
        }
        public static void Unload()
        {
            On.Celeste.TriggerSpikes.Added -= TriggerSpikes_Added;
        }

        private static void TriggerSpikes_Added(On.Celeste.TriggerSpikes.orig_Added orig, TriggerSpikes self, Scene scene)
        {
            TriggerSpikes.SpikeInfo[] origInfo = self.spikes;
            orig(self, scene);
            if (origInfo != null) self.spikes = origInfo;
        }
    }
}