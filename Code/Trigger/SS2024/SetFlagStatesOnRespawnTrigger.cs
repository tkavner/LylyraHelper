using Celeste;
using MonoMod.Cil;
using Monocle;
using System;
using Celeste.Mod.Entities;
using LylyraHelper;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Code.Triggers.SS2024
{
    [CustomEntity("LylyraHelper/SS2024/SetFlagStatesOnRespawnTrigger")]
    public class SetFlagStatesOnRespawnTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
    {
        private List<string> flagWatch = [.. data.Attr("flags", "").Split(',')];
        private static LylyraHelperSession session => LylyraHelperModule.Session;

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            session.respawnFlagMonitor.Clear();
            foreach (var flag in flagWatch)
            {
                session.respawnFlagMonitor[flag] = SceneAs<Level>().Session.GetFlag(flag);
            }
        }

        public static void Load()
        {
            On.Celeste.Level.LoadLevel += Level_LoadLevel;
        }

        private static void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            orig(self, playerIntro, isFromLoader);
            foreach (KeyValuePair<string, bool> kvp in session.respawnFlagMonitor)
                self.Session.SetFlag(kvp.Key, kvp.Value);
        }


        public static void Unload()
        {
            On.Celeste.Level.LoadLevel -= Level_LoadLevel;
        }
    }
}
