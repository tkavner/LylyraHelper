using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using FMOD.Studio;
using LylyraHelper;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Celeste.Mod.LylyraHelper.Code.Triggers
{
    [CustomEntity("LylyraHelper/MuteSoundSourceTrigger")]
    public class MuteSoundSourceTrigger : Trigger
    {
        private string mutedSound;
        private bool unmute;

        public MuteSoundSourceTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            mutedSound = data.Attr("sound", "");
            unmute = data.Bool("unmute", false);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (unmute)
            {
                LylyraHelperModule.Session.mutedSoundSources.Remove(mutedSound);
            }
            else
            {
                LylyraHelperModule.Session.mutedSoundSources.Add("event:/game/06_reflection/crushblock_move_loop");
            }
        }

        public static void Load()
        {
            IL.Celeste.SoundSource.Play += SoundSource_Play;
        }

        private static void SoundSource_Play(MonoMod.Cil.ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            //first we need to find the end of the method so we can have our code branch there if the sound source exists
            Instruction target = null;

            while (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchRet()))
            {
                while (cursor.TryGotoPrev(MoveType.Before, instr => instr.MatchLdloc(1)))
                {
                    target = cursor.Next;
                    break;
                }
                break;
            }
            if (target != null)
            {
                cursor = new ILCursor(il);
                while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<SoundSource>("Stop")) && cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(out int _)))
                {
                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.EmitDelegate((SoundSource soundsource) =>
                    {
                        return LylyraHelperModule.Session.mutedSoundSources.Contains(soundsource.EventName);
                    });
                    cursor.Emit(OpCodes.Brtrue_S, target.Offset);
                    break;
                }
            }
        }

        public static void Unload()
        {
            IL.Celeste.SoundSource.Play -= SoundSource_Play;
        }

    }
}
