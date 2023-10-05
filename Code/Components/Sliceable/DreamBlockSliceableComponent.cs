using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.GaussianBlur;

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceables
{
    public class DreamBlockSliceableComponent : SliceableComponent
    {

        public DreamBlockSliceableComponent(bool active, bool visible) : base(active, visible)
        {
            

        }

        public override void Activate(Slicer slicer)
        {
        }

        public override void OnSliceStart(Slicer slicer)
        {
        }
        

        public override Entity[] Slice(Slicer slicer)
        {
            DreamBlock original = Entity as DreamBlock;
            Vector2[] resultArray = Slicer.CalcCuts(original.Position, new Vector2(original.Width, original.Height), slicer.Entity.Center, slicer.Direction, slicer.CutSize);

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

            List<StaticMover> staticMovers = original.staticMovers;
            foreach (StaticMover mover in staticMovers)
            {
                Slicer.HandleStaticMover(Scene, slicer.Direction, d1, d2, mover);
            }
            Scene.Remove(original);
            AddParticles(original.Position, new Vector2(original.Width, original.Height), Calc.HexToColor("000000"));

            return null;
        }
        //MOD INTEROP TESTING CODE
        /*
        public static void Activate(Entity Entity, DynamicData slicerData)
        {
        }
        public static void OnSliceStart(Entity Entity, DynamicData slicerData)
        {
        }

        public static Entity[] Slice(Entity Entity, DynamicData slicerData)
        {
            DreamBlock original = Entity as DreamBlock;
            Scene Scene = (Scene)slicerData.Get("Scene");
            
            Vector2[] resultArray = Slicer.CalcCuts(original.Position, new Vector2(original.Width, original.Height), ((Entity)slicerData.Get("Entity")).Center, ((Vector2) slicerData.Get("Direction")), (int)slicerData.Get("CutSize"));

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

            List<StaticMover> staticMovers = original.staticMovers;
            foreach (StaticMover mover in staticMovers)
            {
                Slicer.HandleStaticMover(Scene, (Vector2)slicerData.Get("Direction"), d1, d2, mover);
            }
            Scene.Remove(original);

            return null;
        }*/
    }
}
