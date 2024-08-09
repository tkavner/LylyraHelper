using Celeste.Mod.Helpers;
using Celeste.Mod.LylyraHelper.Components.Sliceables;
using Celeste.Mod.LylyraHelper.Other;
using FMOD;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Components
{
    public class SimpleModItemSliceableComponent(Slicer.CustomSlicingActionHolder action) : ModItemSliceableComponent(action)
    {
        private Func<Entity, DynamicData, EntityData> GetEntityData = action.GetEntityData;
        private Func<Entity, DynamicData, Color> GetParticleColor = action.GetParticleColor;
        private Func<Entity, DynamicData, ParticleType> GetParticleType = action.GetParticleType;
        private Func<Entity, DynamicData, int> GetMinWidth = action.GetMinWidth;
        private Func<Entity, DynamicData, int> GetMinHeight = action.GetMinHeight;

        public override Entity[] Slice(Slicer slicer)
        {
            Solid original = Entity as Solid;

            Vector2[] resultArray = Slicer.CalcCuts(original.Position, new Vector2(original.Width, original.Height), slicer.Entity.Center, slicer.Direction, slicer.CutSize);

            Vector2 b1Pos = resultArray[0];
            Vector2 b2Pos = resultArray[1];
            int b1Width = (int)resultArray[2].X;
            int b1Height = (int)resultArray[2].Y;

            int b2Width = (int)resultArray[3].X;
            int b2Height = (int)resultArray[3].Y;

            Solid b1 = null;
            Solid b2 = null;

            var dynamicData = new DynamicData(slicer);

            int minWidth = GetMinWidth?.Invoke(original, dynamicData) ?? 8;
            int minHeight = GetMinHeight?.Invoke(original, dynamicData) ?? 8;
            Color color = GetParticleColor?.Invoke(original, dynamicData) ?? Color.White;

            AddParticles(original.Position, new Vector2(original.Width, original.Height), color);

            Type theirEntityType = FakeAssembly.GetFakeEntryAssembly().GetType(original.GetType().FullName);
            if (b1Width >= minWidth && b1Height >= minHeight)
            {
                EntityData clonedData = LyraUtils.CloneEntityData(GetEntityData(Entity, dynamicData), b1Pos, b1Width, b1Height);
                Entity theirEntity = (Entity)theirEntityType.
                    GetConstructor(new Type[] { typeof(EntityData), typeof(Vector2) }).Invoke(new object[] { clonedData, Vector2.Zero });
                Scene.Add(theirEntity);
            }
            if (b2Width >= minWidth && b2Height >= minHeight)
            {
                EntityData clonedData = LyraUtils.CloneEntityData(GetEntityData(Entity, dynamicData), b2Pos, b2Width, b2Height);
                Entity theirEntity = (Entity)theirEntityType.
                    GetConstructor(new Type[] { typeof(EntityData), typeof(Vector2) }).Invoke(new object[] { clonedData, Vector2.Zero });
                Scene.Add(theirEntity);
            }
            Scene.Remove(original);
            foreach (StaticMover mover in original.staticMovers)
            {
                Slicer.HandleStaticMover(Scene, slicer.Direction, b1, b2, mover);
            }
            postSlice?.Invoke([b1, b2], Entity, dynamicData);
            return [b1, b2];
        }
    }
}
