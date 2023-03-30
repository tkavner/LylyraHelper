using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mods.LylyraHelper.Intefaces
{
    public interface Cuttable
    {
        bool Cut(Vector2 direction, Vector2 position, int width);


    }
}
