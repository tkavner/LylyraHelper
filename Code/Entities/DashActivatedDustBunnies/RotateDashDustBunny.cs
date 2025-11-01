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
        Pivot = data.Position + offset;
        Radius = data.Float("Radius", 10f);
        DistancePerDash = 2 * (float) Math.PI * data.Float("DashesPerFullCycle", 1f);
        InitialAngle = data.Float("InitialAngle", 0f);
        
    }

    public override Vector2 GetTarget(int index)
    {
        return Pivot + Radius * Vector2.UnitX.Rotate(InitialAngle + index * DistancePerDash);
    }

    public override void GoToNextPosition()
    {
        CurrentAngle = float.Lerp(StartAngle, EndAngle, tween.Eased);
        Position = Pivot + Radius * Vector2.UnitX.Rotate(CurrentAngle);
    }

}