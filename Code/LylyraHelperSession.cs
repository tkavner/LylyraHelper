using Celeste.Mod;
using System.Collections.Generic;

namespace LylyraHelper;

public class LylyraHelperSession : EverestModuleSession
{
    public string[] defaultSlicerSettings; //this can be made a list so we dont convert between string[] and List<> if the game wont cause race conditions but idk if it does

    //cursed refill stuff
    public bool playerCursed { get; set; } = false;
    public bool ignoreDash { get; set; } = false;
    public bool killPlayerWhenSafe { get; set; } = false;

    public void ResetCurse()
    {
        playerCursed = false;
        ignoreDash = false;
        killPlayerWhenSafe = false;
    }

    public void SetCurse(bool ignoreDashNew)
    {
        this.ignoreDash = ignoreDashNew;
        this.playerCursed = playerCursed || !ignoreDashNew;
    }
    public bool NoFastfall { get; set; }

    public Dictionary<string, bool> respawnFlagMonitor = new();

    public List<string> mutedSoundSources = new();
    
    public bool DisableEntityPreservation { get; set; }
}