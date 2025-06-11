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

namespace Celeste.Mod.LylyraHelper.Code.Components.Sliceable;

public class ZipMoverSliceableComponent : SliceableComponent
{
    private const string LogId = $"{nameof(LylyraHelper)}/{nameof(ZipMoverSliceableComponent)}";

    private static bool hooksLoaded;
    private static ILHook hook_zipmover_sequence;
    private bool shouldActivate = false;
    private Vector2 actualStartingPosition;
    private float origLerp;
    private bool artificial;
    private bool useOrigLerp;

    public ZipMoverSliceableComponent(bool active, bool visible) : base(active, visible)
    {
    }

    public override void Activate(Slicer slicer, Slicer.NewlySlicedEntityWrapper data)
    {
        //newly created zip movers are activated by an ILHook. We simply set the shouldActivate attribute to true to trigger it.
        //Since this method is only called on newly sliced entities, it does not apply to zip movers by default.
        shouldActivate = true;
        artificial = true;
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

        //attempt to recreate the variable "at2" from percent, which is given by percent = Ease.SineIn(at2) and Ease.SineIn is given by 0f - (float)Math.Cos((float)Math.PI / 2f * t) + 1f
        //percent = (0f - (float)Math.Cos((float)Math.PI / 2f * t) + 1f)
        //percent = (0f - (float)Math.Cos((float)Math.PI / 2f * t) + 1f)
        //percent = 1f - Math.Cos((float)Math.PI / 2f * t)
        //1f - percent = Math.Cos((float)Math.PI / 2f * t)
        //arccos(1f - percent) = math.pi / 2f * t

        origLerp = (float) (Math.Acos(1 - original.percent) * 2 / Math.PI);

        useOrigLerp = true;

        Entity.Position = actualStartingPosition;

        //copy of the StaticMover attach code from Solid.Awake
        //normally not needed, but since zipmovers have some creative placement to keep the ZipTrackRenderer in place, we have to manually do it here
        Solid solid = Entity as Solid;
        foreach (StaticMover component in Entity.Scene.Tracker.GetComponents<StaticMover>())
        {
            component.Platform = null;
        }
        solid.staticMovers.Clear();
        foreach (StaticMover component in Entity.Scene.Tracker.GetComponents<StaticMover>())
        {
            if (component.Platform == null && component.IsRiding(solid))
            {
                (Entity as Solid).staticMovers.Add(component);
                component.Platform = solid;
                if (component.OnAttach != null)
                {
                    component.OnAttach(solid);
                }
            }
        }

        (Entity as ZipMover).start = original.start + actualStartingPosition - original.Position;
        (Entity as ZipMover).target = original.target + actualStartingPosition - original.Position;
        (Entity as ZipMover).percent = original.percent;
    }

    private enum Orientation
    {
        Up, Down, Left, Right
    }


    public override void OnSliceStart(Slicer slicer)
    {
    }

    public override SlicerCollisionResults Slice(Slicer slicer)
    {
        if (artificial && shouldActivate) return null; //Recently sliced zip movers need immunity to slicing because of their abnormal position set up logic. Basically, since sliced zip movers aren't in their proper position until shouldActivate is set to false, we don't attempt to slice until we are in our proper position
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
            b1 = new ZipMover(original.pathRenderer.from + new Vector2( - b1Width,  - b1Height) / 2, b1Width, b1Height, original.pathRenderer.to + new Vector2( - b1Width,  - b1Height) / 2, original.theme);
            Scene.Add(b1);
        }
        if (b2Width >= 16 && b2Height >= 16)
        {
            b2 = new ZipMover(original.pathRenderer.from + new Vector2( - b2Width,  - b2Height) / 2, b2Width, b2Height, original.pathRenderer.to + new Vector2( - b2Width,  - b2Height) / 2, original.theme);
            Scene.Add(b2);
        }
        foreach (StaticMover mover in original.staticMovers)
        {
            if (b1 != null) b1.Position = b1Pos;
            if (b2 != null) b2.Position = b2Pos;
            Slicer.HandleStaticMover(Scene, slicer.Direction, b1, b2, mover);

            if (b1 != null)
            {
                b1.Position = original.pathRenderer.from + new Vector2(original.Width - b1Width, original.Height - b1Height) / 2;
            }
            if (b2 != null)
            {
                b2.Position = original.pathRenderer.from + new Vector2(original.Width - b2Width, original.Height - b2Height) / 2;
            }
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
        ILCursor cursor = new(il);

        FieldReference zipMoverStartField = null;
        if (!cursor.TryGotoNext(MoveType.After,
            static instr => instr.MatchLdloc1(),
            static instr => instr.MatchLdfld<Entity>(nameof(Entity.Position)),
            instr => instr.MatchStfld(out zipMoverStartField)))
        {
            Logger.Error(LogId, $"Could not find \"start = Position\" variable assignment in {il.Method.FullName}!");
            return;
        }

        if (!cursor.TryGotoNext(MoveType.After,
            static instr => instr.MatchCallvirt<Solid>(nameof(Solid.HasPlayerRider))))
        {
            Logger.Error(LogId, $"Could not find {nameof(Solid)}.{nameof(Solid.HasPlayerRider)} in {il.Method.FullName}!");
            return;
        }

        // position the cursor first, so that we don't suddenly abort without emitting all the IL
        ILCursor useOrigLerpCursor = cursor.Clone();
        if (!useOrigLerpCursor.TryGotoNext(MoveType.After,
            static instr => instr.MatchCallvirt<Sprite>(nameof(Sprite.SetAnimationFrame))))
        {
            Logger.Error(LogId, $"Could not find {nameof(Sprite)}.{nameof(Sprite.SetAnimationFrame)} in {il.Method.FullName}!");
            return;
        }

        if (!useOrigLerpCursor.TryGotoNext(MoveType.After,
            static instr => instr.MatchLdcR4(0f)))
        {
            Logger.Error(LogId, $"Could not find \"at = 0f\" variable assignment in {il.Method.FullName}!");
            return;
        }

        cursor.EmitPop();
        cursor.EmitLdarg0();
        cursor.EmitLdloc1();
        cursor.EmitDelegate(GetZipMoverStart);
        cursor.EmitStfld(zipMoverStartField);

        cursor.EmitLdloc1();
        cursor.EmitDelegate(HasPlayerRiderHook);

        useOrigLerpCursor.EmitPop();
        useOrigLerpCursor.EmitLdloc1();
        useOrigLerpCursor.EmitDelegate(UseOrigLerp);
    }

    private static float UseOrigLerp(ZipMover zipMover)
    {
        if (zipMover.Get<ZipMoverSliceableComponent>() is not { useOrigLerp: true } comp)
            return 0f;

        comp.useOrigLerp = false;
        return comp.origLerp;
    }

    private static bool HasPlayerRiderHook(ZipMover zipMover)
    {
        if (zipMover.Get<ZipMoverSliceableComponent>() is not { } comp)
            return zipMover.HasPlayerRider();

        bool toReturn = comp.shouldActivate || zipMover.HasPlayerRider();
        if (comp.shouldActivate)
            zipMover.Position = comp.actualStartingPosition;
        comp.shouldActivate = false;
        return toReturn;
    }

    private static Vector2 GetZipMoverStart(ZipMover zipMover)
        => zipMover.start;
}
