using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.LylyraHelper.Entities;

[CustomEntity("LylyraHelper/Dev/EntitySpammer")]
public class EntitySpammer : Entity
{
    public EntitySpammer(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        
    }

    private int offset = 0;
    private int increment = 8;
    private string flag = "spam";

    public override void Update()
    {
        base.Update();
        if (SceneAs<Level>().Session.GetFlag(flag))
        {
            Scene.Add(new DreamBlock(Position + new Vector2(offset % 300, -8 * (offset / 300f)), 8, 8, null, false, false, false));
        }
    }
}