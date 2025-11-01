using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.LylyraHelper.Entities.DashActivatedDustBunnies;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.LylyraHelper.Entities;

//adapted from DustTrackSpinner and TrackSpinner
public abstract class DashActivatedDustBunny : Entity
{
    private float TravelTime;
    public DustGraphic dusty;
    public Tween tween;
    private Vector2 outwards;
    private bool Up = false;
    private bool Moving 
    {
        get
        {
            return tween != null && !Hit;
        }
    }

    private bool Hit = false;

    protected Vector2 Start { get; private set; }

    private Vector2 End { get; set; }

    protected int TargetIndex { get; private set; }

    private int NextNode => (TargetIndex + 1);

    public DashActivatedDustBunny(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        this.Add((this.dusty = new DustGraphic(true)));
        this.dusty.EyeDirection = this.dusty.EyeTargetDirection = (this.End - this.Start).SafeNormalize();
        this.dusty.OnEstablish = new Action(this.Establish);
        this.Depth = -50;
        TravelTime = data.Float("TravelTime", 0.15f);
        Add(new PlayerCollider(OnPlayer));
        Add(new DashListener(OnDash));
        Active = true;
        Visible = true;
        this.Collider = new ColliderList(new Collider[2]
        {
            new Circle(6f),
            new Hitbox(16f, 4f, -8f, -3f)
        });
    }

    public override void Added(Scene scene)
    {
        this.dusty.EyeDirection = this.dusty.EyeTargetDirection = (GetTarget(1) - GetTarget(0)).SafeNormalize();
        Visible = true;
        Active = true;
    }

    public void OnPlayer(Player player)
    {
        if (player.Die((player.Position - this.Position).SafeNormalize()) == null)
            return;
        this.dusty.OnHitPlayer();
        Hit = true;
        Remove(tween);
        tween = null;
    }

    public override void Update()
    {
        base.Update();
        if (Moving)
        {
            GoToNextPosition();
            //this.SceneAs<Level>().ParticlesBG.Emit(DustStaticSpinner.P_Move, 1, this.Position, Vector2.One * 4f);
        }
    }

    public Ease.Easer MotionEaser = Ease.CubeInOut;
    public abstract Vector2 GetTarget(int index);

    public abstract void GoToNextPosition();

    private void OnDash(Vector2 direction)
    {
        Start = Position;
        TargetIndex++;
        End = GetTarget(TargetIndex);
        tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, TravelTime, false);
        tween.OnComplete = OnComplete;
        Add (tween);
        tween.Start();
    }

    private void OnComplete(Tween tween)
    {
        Remove(tween);
        tween = null;
        if (this.outwards != Vector2.Zero)
        {
            this.dusty.EyeDirection = this.dusty.EyeTargetDirection = (GetTarget(NextNode) - GetTarget(TargetIndex)).SafeNormalize();
        }
        else
        {
            this.dusty.EyeDirection = this.dusty.EyeTargetDirection = (GetTarget(NextNode) - GetTarget(TargetIndex)).SafeNormalize();
            this.dusty.EyeFlip = -this.dusty.EyeFlip;
        }
    }

    private void Establish()
    {
        Vector2 directionNormalized = (this.End - this.Start).SafeNormalize();
        Vector2 directionPerpendicular = new Vector2(-directionNormalized.Y, directionNormalized.X);
        bool smallSpace = this.Scene.CollideCheck<Solid>(new Rectangle((int)((double)this.X + (double)directionPerpendicular.X * 4.0) - 2,
            (int)((double)this.Y + (double)directionPerpendicular.Y * 4.0) - 2, 4, 4));
        if (!smallSpace)
        {
            directionPerpendicular = -directionPerpendicular;
            smallSpace = this.Scene.CollideCheck<Solid>(new Rectangle((int)((double)this.X + (double)directionPerpendicular.X * 4.0) - 2,
                (int)((double)this.Y + (double)directionPerpendicular.Y * 4.0) - 2, 4, 4));
        }

        if (!smallSpace)
            return;
        Vector2 direction = this.End - this.Start;
        float num = direction.Length();
        for (int index = 8; (double)index < (double)num & smallSpace; index += 8)
            smallSpace = smallSpace && this.Scene.CollideCheck<Solid>(new Rectangle(
                (int)((double)this.X + (double)directionPerpendicular.X * 4.0 + (double)directionNormalized.X * (double)index) - 2,
                (int)((double)this.Y + (double)directionPerpendicular.Y * 4.0 + (double)directionNormalized.Y * (double)index) - 2, 4, 4));
        if (!smallSpace)
            return;
        List<DustGraphic.Node> nodeList = (List<DustGraphic.Node>)null;
        if ((double)directionPerpendicular.X < 0.0)
            nodeList = this.dusty.LeftNodes;
        else if ((double)directionPerpendicular.X > 0.0)
            nodeList = this.dusty.RightNodes;
        else if ((double)directionPerpendicular.Y < 0.0)
            nodeList = this.dusty.TopNodes;
        else if ((double)directionPerpendicular.Y > 0.0)
            nodeList = this.dusty.BottomNodes;
        if (nodeList != null)
        {
            foreach (DustGraphic.Node node in nodeList)
                node.Enabled = false;
        }

        this.outwards = -directionPerpendicular;
        this.dusty.Position -= directionPerpendicular;
        DustGraphic dusty1 = this.dusty;
        DustGraphic dusty2 = this.dusty;
        dusty2.EyeTargetDirection = directionNormalized;
        dusty1.EyeDirection = directionNormalized;
    }
}