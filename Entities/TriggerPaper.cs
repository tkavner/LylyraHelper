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

        public TriggerPaper(Vector2 position, int width, int height, bool safe, string triggerName, string texture = "objects/LylyraHelper/dashPaper/cloudblocknew", string[] triggerParams = null) : 
            base(position, width, height, safe, texture)
        {
            
        }
    }
}
