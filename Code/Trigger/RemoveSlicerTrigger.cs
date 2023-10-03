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

namespace Celeste.Mod.LylyraHelper.Triggers
{
    [Tracked(false)]
    [CustomEntity("LylyraHelper/RemoveSlicerTrigger")]
    public class RemoveSlicerTrigger : Trigger
    {
        private bool oneUse;
        private string[] entityTypes;
        private bool roomwide;
        private bool onLoad;
        private bool used;
        private bool invert;
        private string flag;

        public RemoveSlicerTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            oneUse = data.Bool("singleUse", false);
            entityTypes = data.Attr("entityTypes", "").Split(',');
            roomwide = data.Bool("roomwide", false);
            onLoad = data.Bool("onLoadOnly", false);
            flag = data.Attr("flag", "");
            invert = data.Bool("invert", false);
        }


        public override void Update()
        {
            base.Update();
            if (!used && CheckFlag())
            {
                foreach (Slicer slicer in Scene.Tracker.GetComponents<Slicer>())
                {
                    TryRemoveSlicer(slicer.Entity, slicer);
                }
            }
            if (onLoad) Scene.Remove(this);
        }


        public bool CheckFlag()
        {
            return flag == "" || (SceneAs<Level>().Session.GetFlag(flag) ^ invert);
        }


        private void TryRemoveSlicer(Entity entity, Slicer slicer)
        {
            string entityName = entity.GetType().FullName;
            bool flag0 = entityTypes.Length == 0;
            flag0 = flag0 || entityTypes.Contains(entityName);
            if (flag0)
            {
                bool tempCollide = this.Collidable;
                this.Collidable = true;
                bool collide = this.CollideCheck(entity);
                this.Collidable = tempCollide;
                if (collide || roomwide)
                {
                    entity.Remove(slicer);
                    if (oneUse)
                    {
                        Scene.Remove(this);
                    }
                }
            }
        }
    }
}
