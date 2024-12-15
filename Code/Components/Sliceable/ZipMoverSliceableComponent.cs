using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceable
{
    public class ZipMoverSliceableComponent : SliceableComponent 
    {
        private static bool hooksLoaded;
        private static ILHook hook_zipmover_sequence;
        private bool shouldActivate = false;
        private Vector2 actualStartingPosition;
        private float origLerp;
        private bool useOrigLerp;

        public ZipMoverSliceableComponent(bool active, bool visible) : base(active, visible)
        {
        }

        public override void Activate(Slicer slicer, Slicer.NewlySlicedEntityWrapper data)
        {
            //newly created zip movers are activated by an ILHook. We simply set the shouldActivate attribute to true to trigger it.
            //Since this method is only called on newly sliced entities, it does not apply to zip movers by default.
            shouldActivate = true;
            var resultArray = data.Results.CollisionResults;
            ZipMover original = (data.Results.Parent as ZipMover);
            Vector2 b1Pos = resultArray[0];
            Vector2 b2Pos = resultArray[1];

            if (data.Results.Children[0] == data.child)
            {
                actualStartingPosition = b1Pos;
            } else
            {
                actualStartingPosition = b2Pos;
            }
            if ((original.target - original.start).Length() == 0) origLerp = original.percent;//fails on non moving zippers, use percent for a good estimation
            else origLerp = (original.Position - original.start).Length() / (original.target - original.start).Length(); 

            useOrigLerp = true;
            Entity.Position = actualStartingPosition;
            (Entity as ZipMover).start = original.start + actualStartingPosition - original.Position;
            (Entity as ZipMover).target = original.target + actualStartingPosition - original.Position;
            (Entity as ZipMover).percent = original.percent;
        }

        public override void OnSliceStart(Slicer slicer)
        {
        }

        public override SlicerCollisionResults Slice(Slicer slicer)
        {
            ZipMover original = Entity as ZipMover;
            Vector2[] resultArray = Slicer.CalcCuts(original.Position, new Vector2(original.Width, original.Height), slicer.Entity.Center, slicer.Direction, slicer.CutSize);

            Vector2 b1Pos = resultArray[0];
            Vector2 b2Pos = resultArray[1];
            int b1Width = (int)resultArray[2].X;
            int b1Height = (int)resultArray[2].Y;

            int b2Width = (int)resultArray[3].X;
            int b2Height = (int)resultArray[3].Y;

            ZipMover b1 = null;
            ZipMover b2 = null;


            Scene.Remove(original);
            if (b1Width >= 16 && b1Height >= 16)
            {
                b1 = new ZipMover(original.start + new Vector2(original.Width - b1Width, original.Height - b1Height) / 2, b1Width, b1Height, original.target + new Vector2(original.Width - b1Width, original.Height - b1Height) / 2, original.theme);
                Scene.Add(b1);
            }
            if (b2Width >= 16 && b2Height >= 16)
            {
                b2 = new ZipMover(original.start + new Vector2(original.Width - b2Width, original.Height - b2Height) / 2, b2Width, b2Height, original.target + new Vector2(original.Width - b2Width, original.Height - b2Height) / 2, original.theme);
                Scene.Add(b2);
            }
            foreach (StaticMover mover in original.staticMovers)
            {
                Slicer.HandleStaticMover(Scene, slicer.Direction, b1, b2, mover);
            }
            AddParticles(
                original.Position,
                new Vector2(original.Width, original.Height),
                Calc.HexToColor("444444"));
            return new SlicerCollisionResults([b1, b2], original, resultArray);
        }


        public static void Load()
        {
            if (hooksLoaded) return;
            hooksLoaded = true;
            hook_zipmover_sequence = new ILHook(typeof(ZipMover).GetMethod(nameof(ZipMover.Sequence), BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), ZipMoverPatch);

        }

        public static void Unload()
        {
            if (!hooksLoaded) return;
            hooksLoaded = false;

            hook_zipmover_sequence.Dispose();
        }
        private static void ZipMoverPatch(ILContext il)
        {
            
            ILCursor cursor = new ILCursor(il);
            FieldReference fieldLabel = null;

            while (cursor.TryGotoNext(MoveType.After, i1 => i1.MatchLdloc(1)))
            {
                while (cursor.TryGotoNext(instr => instr.MatchStfld(out fieldLabel)))
                {
                    break;
                }
                break;
            }

            Logger.Log(LogLevel.Error, "LylyraHelper", "got this far" + (fieldLabel != null) + fieldLabel.Name);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt("Celeste.Solid", "HasPlayerRider")))
            {
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloc_1);
                
                cursor.EmitDelegate<Func<ZipMover, Vector2>>((ZipMover zipMover) => {
                    ZipMoverSliceableComponent comp;
                    if ((comp = zipMover.Get<ZipMoverSliceableComponent>()) != null && comp.shouldActivate)
                    {
                        return zipMover.start;
                    } else
                    {
                        return zipMover.start;
                    }
                });
                cursor.Emit(OpCodes.Stfld, fieldLabel);


                cursor.Emit(OpCodes.Ldloc_1);
                cursor.EmitDelegate((ZipMover zipMover) =>
                {
                    ZipMoverSliceableComponent comp;
                    if ((comp = zipMover.Get<ZipMoverSliceableComponent>()) != null)
                    {
                        bool toReturn = comp.shouldActivate || zipMover.HasPlayerRider(); //we use a "not" on the should activate since the ILCode we are working with is optimized to brfalse
                        if (comp.shouldActivate) zipMover.Position = comp.actualStartingPosition;
                        comp.shouldActivate = false;
                        return toReturn ? true : false;
                    }
                    return zipMover.HasPlayerRider() ? true : false;
                });
                break;
            }

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Sprite>("SetAnimationFrame"))) 
            {
                while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0F)))
                {
                    cursor.Emit(OpCodes.Pop);
                    cursor.Emit(OpCodes.Ldloc_1);
                    cursor.EmitDelegate<Func<ZipMover, float>>((ZipMover zipMover) => {
                        ZipMoverSliceableComponent comp = zipMover.Get<ZipMoverSliceableComponent>();
                        if (comp != null && comp.useOrigLerp)
                        {
                            comp.useOrigLerp = false;
                            Logger.Log(LogLevel.Error, "LylyraHelper", "!!" + comp.useOrigLerp + "|" + comp.origLerp);
                            return comp.origLerp;
                        }
                        return 0f;

                    });
                    break;
                }
                break;
            }
        }
    }
}
