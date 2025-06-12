using Celeste.Mod.Entities;
using Celeste.Mod.LylyraHelper.Entities;
using LylyraHelper;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Code.Triggers
{
    [CustomEntity("LylyraHelper/DisableEntityPreservationTrigger")]
    public class DisableEntityPreservationTrigger : Trigger
    {
        private bool Active;
        private static LylyraHelperSession Session => LylyraHelperModule.Session;

        public DisableEntityPreservationTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Active = data.Bool("activate", true);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Session.DisableEntityPreservation = Active;
        }

        public static void Load()
        {
            On.Celeste.Level.UnloadLevel += UnloadLevel;
        }

        public static void Unload()
        {
            On.Celeste.Level.UnloadLevel -= UnloadLevel;
        }
        
        
        private static void UnloadLevel(On.Celeste.Level.orig_UnloadLevel orig, Level self)
        {
            
            if (Session != null && Session.DisableEntityPreservation)
            {
                
                self.Entities.ToAdd.Clear();
            }
            orig(self);
        }

    }
}
