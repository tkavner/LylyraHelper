using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Entities
{
    public class TriggerPaper : Paper
    {
        private Trigger trigger;

        public TriggerPaper(EntityData data, Vector2 offset, Vector2 position, int width, int height, string triggerName, string texture = "objects/LylyraHelper/dashPaper/cloudblocknew", string[] triggerParams = null) : 
            base(data, offset,  width, height, texture)
        {
            
        }
    }
}
