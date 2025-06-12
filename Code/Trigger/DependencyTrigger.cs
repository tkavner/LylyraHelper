using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.LylyraHelper.Code.Triggers;

[CustomEntity("LylyraHelper/DependencyTrigger")]
public class DependencyTrigger : Trigger
{
    private string dependency;
    private string flagName;
    private bool invert;

    public DependencyTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        dependency = data.Attr("dependency", "");
        flagName = data.Attr("flag", "");
        invert = data.Bool("invert", false);
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        if (Everest.Modules.Any(m => m.Metadata.Name == dependency))
        {
            SceneAs<Level>().Session.SetFlag(flagName, !invert);
        }
    }

}