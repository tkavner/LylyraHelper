using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod.LylyraHelper.Code.Components;
using Celeste.Mod.LylyraHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LylyraHelper.Entities
{
    [CustomEntity("LylyraHelper/SlicerController")]
    public class SlicerController : Entity
    {
        private string settings;
        private bool limitToController;

        public SlicerController(EntityData data, Vector2 offset) {
            settings = data.Attr("sliceableEntityTypes", "");


        }

        public override void Awake(Scene scene)
        {
            Slicer.SlicerSettings.DefaultSettings = new Slicer.SlicerSettings(settings);
            scene.Remove(this);
        }
    }
}
