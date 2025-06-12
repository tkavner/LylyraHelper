using MonoMod.Cil;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework;
using LylyraHelper;
using Celeste.Mod.Entities;

namespace Celeste.Mod.LylyraHelper.Triggers;

[CustomEntity("LylyraHelper/NoFastfallTrigger")]
public class NoFastfallTrigger : Trigger
{
    private static bool hooksLoaded;
    private bool invert;
    private bool persistent;
    private LylyraHelperSession session => LylyraHelperModule.Session;

    public NoFastfallTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        invert = data.Bool("invert", false);
        persistent = data.Bool("persistent", false);
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        session.NoFastfall = !invert;

    }
    public override void OnStay(Player player)
    {
        base.OnEnter(player);
        session.NoFastfall = !invert;

    }
    public override void OnLeave(Player player)
    {
        base.OnLeave(player);
        if (!persistent) session.NoFastfall = invert;
    }

    public static void Load()
    {
        if (hooksLoaded) return;
        hooksLoaded = true;

        IL.Celeste.Player.NormalUpdate += FastfallPatch;
    }
    public static void Unload()
    {
        if (!hooksLoaded) return;
        hooksLoaded = false;

        IL.Celeste.Player.NormalUpdate -= FastfallPatch;
    }

    private static void FastfallPatch(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f)))
        {
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld("Celeste.Input", "MoveY")))
            {
                while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(1)))
                {
                    cursor.EmitDelegate(OverrideFastFall);
                    break;
                }
                break;
            }
            break;
        }
    }
    //This trigger started as a port of an identical trigger in Santa's Gifts.
    //Both triggers work by setting the condition for the input of the move value to be equal to 10000 instead of the expected value of 1
    //We don't know which loads first, so both triggers grab the value from the stack and simply pass it back if it has been modified (that is, not equal to 1).
    //This way both triggers can exist in the same codespace.
    private static float OverrideFastFall(float f)
    {
        if (f > 1) return f;
        return LylyraHelperModule.Session.NoFastfall ? 10000F : f;
    } 
}
