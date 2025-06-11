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

namespace Celeste.Mod.LylyraHelper.Code.Triggers;

[CustomEntity("LylyraHelper/MuteSoundSourceTrigger")]
public class MuteSoundSourceTrigger : Trigger
{
    private string[] mutedSounds;
    private bool unmute;
    private bool muteActiveSourcesOnEnter;

    private static LylyraHelperSession session => LylyraHelperModule.Session;

    public MuteSoundSourceTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        mutedSounds = data.Attr("sound", "").Split(',');
        unmute = data.Bool("unmute", false);
        muteActiveSourcesOnEnter = data.Bool("muteActiveSourcesOnEnter", false);
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        if (unmute)
        {
            foreach (var sound in mutedSounds)
            {
                if (session.mutedSoundSources.Contains(sound))
                {
                    session.mutedSoundSources.Remove(sound);
                }
            }
        }
        else
        {
            foreach(var sound in mutedSounds)
            {
                if (!session.mutedSoundSources.Contains(sound))
                {
                    session.mutedSoundSources.Add(sound);
                }
                if (muteActiveSourcesOnEnter)
                {
                    foreach (SoundSource source in Scene.Tracker.GetComponents<SoundSource>())
                    {
                        if (sound == source.EventName)
                        {
                            source.Stop();
                        }
                    }
                }
            }
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
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdarg(1)) && cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(0)) && cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdloc(0)))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                // replace IL_0016: ldloc.0 with a null if the sound source is in the list of muted sources
                cursor.EmitDelegate(ReplaceEventDescription);
                while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdarg(0)) && cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<SoundSource>("instance")))
                {
                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.EmitDelegate(ReplaceEventInstance);
                    break;
                }
                break;
            }
        } else
        {
            Logger.Log("LylyraHelper", "MuteSoundSourceTrigger failed to find target for ILHook.");
        }
    }

    private static EventInstance ReplaceEventInstance(EventInstance passBack, SoundSource soundSource)
        => session.mutedSoundSources.Contains(soundSource.EventName) ? null : passBack;

    private static EventDescription ReplaceEventDescription(EventDescription passBack, SoundSource soundSource)
        => session.mutedSoundSources.Contains(soundSource.EventName) ? null : passBack;

    public static void Unload()
    {
        IL.Celeste.SoundSource.Play -= SoundSource_Play;
    }

}
