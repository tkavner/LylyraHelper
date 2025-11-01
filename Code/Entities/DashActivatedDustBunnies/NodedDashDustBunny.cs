using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.LylyraHelper.Entities.DashActivatedDustBunnies;

[CustomEntity("LylyraHelper/DashActivatedDustBunnies/Noded", "LylyraHelper/DashActivatedDustBunny")]
public class NodedDashDustBunny : DashActivatedDustBunny
{
    private Vector2[] Nodes;
    public NodedDashDustBunny(EntityData data, Vector2 offset) : base(data, offset)
    {
        Nodes = data.NodesWithPosition(offset);
    }
    public override Vector2 GetTarget(int index)
    {
        return Nodes[index % Nodes.Length];
    }

    public override void GoToNextPosition()
    {
        Position = Vector2.Lerp(Start, GetTarget(TargetIndex), tween.Eased);
    }
}