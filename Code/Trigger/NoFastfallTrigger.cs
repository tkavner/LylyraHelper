using Celeste;
using MonoMod.Cil;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework;
using LylyraHelper;
using Celeste.Mod.Entities;

namespace Celeste.Mod.LylyraHelper.Triggers
{
    [CustomEntity("LylyraHelper/NoFastfallTrigger")]
    public class NoFastfallTrigger : Trigger
    {
        private static bool hooksLoaded;
        private bool invert;
        private LylyraHelperSession session => LylyraHelperModule.Session;

        public NoFastfallTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            invert = data.Bool("invert", false);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            session.NoFastfall = !invert;

        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            session.NoFastfall = invert;
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
                        cursor.Emit(OpCodes.Pop);
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.EmitDelegate((Player player) =>
                        {
                            return (LylyraHelperModule.Session.NoFastfall) ? 10000F : 1f;
                        });
                        break;
                    }
                    break;
                }
                break;
            }
        }
    }
}
