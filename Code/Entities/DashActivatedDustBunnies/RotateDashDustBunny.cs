using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.LylyraHelper.Entities.DashActivatedDustBunnies;

[CustomEntity("LylyraHelper/DashActivatedDustBunnies/Rotate")]
public class RotateDashDustBunny : DashActivatedDustBunny
{
    private Vector2 Pivot;
    private float Radius;

    private float StartAngle => (Start - Pivot).Angle();
    private float EndAngle => (GetTarget(TargetIndex) - Pivot).Angle();
    public float CurrentAngle { get; set; }
    private float InitialAngle;
    private float DistancePerDash;
    
    public RotateDashDustBunny(EntityData data, Vector2 offset) : base(data, offset)
    {
        var nodes = data.NodesWithPosition(offset);
        Radius = (nodes[0] - nodes[1]).Length(); //position - pivot
        DistancePerDash = 2 * (float) Math.PI / data.Float("DashesPerFullCycle", 1f);
        InitialAngle = (nodes[0] - nodes[1]).Angle();//position - pivot
        Pivot = nodes[1];
    }

    public override Vector2 GetTarget(int index)
    {
        return Pivot + Radius * Vector2.UnitX.Rotate(InitialAngle + index * DistancePerDash);
    }

    public override void GoToNextPosition()
    {
        var end = EndAngle;
        if (EndAngle < StartAngle) end += (float) Math.PI * 2; //stops reversing due to swapping branch
        CurrentAngle = float.Lerp(StartAngle, end, tween.Eased);
        Position = Pivot + Radius * Vector2.UnitX.Rotate(CurrentAngle);
    }

}