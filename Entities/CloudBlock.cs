using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Entities
{
    class CloudBlock : JumpThru
    {

        //calling them trampolines to give them a unique name for sanity purposes
        class MiniTrampoline : JumpThru
        {

            Vector2 jumpPosition; //different from the cloud's position oddly enough
            private float timer;
            private float respawnTimer;
			private float endY;
            private float speed;
            private CloudBlock groupParent;
            private bool canRumble;
            private State state;
            private float startY;
            private MTexture sprite;

            public enum State
            {
                descending, springUp, returning
            }

            public MiniTrampoline(Vector2 position, int width, bool safe, CloudBlock cb)
                : base(position, width, safe)
            {
                groupParent = cb;
                state = State.descending;
                startY = Position.Y;

                sprite = GFX.Game["objects/LylyraHelper/cloudblock/minicloud"]);
            }

			public override void Added(Scene scene)
			{
				base.Added(scene);
			}

            public override void Update()
            {
                base.Update();
                Player p = GetPlayerRider();

                if (p != null)
                {
                    //calc speed
                    switch (state)
                    {
                        case State.descending:
                            speed = 180f;
                            Audio.Play("event:/game/04_cliffside/cloud_pink_boost", Position);
                            state = State.springUp;
                            return;
                        case State.springUp:
                            if (base.Y >= startY)
                            {
                                speed -= 1200f * Engine.DeltaTime;
                            }
                            else
                            {
                                speed += 1200f * Engine.DeltaTime;
                                if (speed >= -100f)
                                {
                                    Player playerRider2 = GetPlayerRider();
                                    if (playerRider2 != null && playerRider2.Speed.Y >= 0f)
                                    {
                                        playerRider2.Speed.Y = -200f;
                                    }
                                    Collidable = false;
                                    groupParent.RemoveTrampoline(this);
                                }
                            }
                            break;
                        case State.returning:
                            speed = Calc.Approach(speed, 180f, 600f * Engine.DeltaTime);
                            MoveTowardsY(startY, speed * Engine.DeltaTime);
                            if (base.ExactPosition.Y == startY)
                            {
                                speed = 0f;
                            }
                            return;

                    }
                }

                float num = speed;
                if (num < 0f)
                {
                    num = -220f;
                }
                MoveV(speed * Engine.DeltaTime, num);
            }

            public override void Render()
            {
                sprite.Draw(Position);
            }
        }

        private List<MiniTrampoline> trampolines;
        private Scene scene;
        private Vector2 groupOrigin;
        private List<CloudBlock> group;
        private bool groupLeader;

        private List<Image> cloudImage = new List<Image>();

        private List<Image> all = new List<Image>();


        public CloudBlock(Vector2 position, int width, int height, bool safe)
        : base(position, width, safe)
        {
            base.Collider = new Hitbox(width, height);
        }

        public CloudBlock(EntityData data, Vector2 vector2) : this(data.Position + vector2, data.Width, data.Height, false)
        {

        }

        private void SetImage(float x, float y, int tx, int ty)
        {
            cloudImage.Add(CreateImage(x, y, tx, ty, GFX.Game["objects/LylyraHelper/cloudblock/cloud"]));
        }

        private Image CreateImage(float x, float y, int tx, int ty, MTexture tex)
        {
            Vector2 vector = new Vector2(x - base.X, y - base.Y);
            Image image = new Image(tex.GetSubtexture(tx * 8, ty * 8, 8, 8));
            Vector2 vector2 = groupOrigin - Position;
            image.Origin = vector2 - vector;
            image.Position = vector2;
            Add(image);
            all.Add(image);
            return image;
        }

        private bool CheckForSame(float x, float y)
        {
            foreach (CloudBlock entity in base.Scene.Tracker.GetEntities<CassetteBlock>())
            {
                if (entity.Collider.Collide(new Rectangle((int)x, (int)y, 8, 8)))
                {
                    return true;
                }
            }
            return false;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            this.scene = scene;
            if (group == null)
            {
                groupLeader = true;
                group = new List<CloudBlock>();
                trampolines = new List<MiniTrampoline>();
                group.Add(this);
                FindInGroup(this);

                float num = float.MaxValue;
                float num2 = float.MinValue;
                float num3 = float.MaxValue;
                float num4 = float.MinValue;
                foreach (CloudBlock item in group)
                {
                    if (item.Left < num)
                    {
                        num = item.Left;
                    }
                    if (item.Right > num2)
                    {
                        num2 = item.Right;
                    }
                    if (item.Bottom > num4)
                    {
                        num4 = item.Bottom;
                    }
                    if (item.Top < num3)
                    {
                        num3 = item.Top;
                    }
                }

                groupOrigin = new Vector2((int)(num + (num2 - num) / 2f), (int)num4);

                foreach (CloudBlock item2 in group)
                {
                    item2.groupOrigin = groupOrigin;
                }
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            this.scene = null;
        }

        public override void Update()
        {
            base.Update();
            
            if (!groupLeader)
            {
                return;
            }

            if (CanCloudSpawn())
            {
                //check if player is there
                if (HasPlayerRider())
                {
                    SpawnCloud();
                }
            }
        }


        private void SpawnCloud()
        {
            trampolines.Add(new MiniTrampoline(GetPlayerRider().Position, 32, false, this));
        }

        //
        private void FindInGroup(CloudBlock block)
        {
            foreach (CloudBlock entity in base.Scene.Tracker.GetEntities<CloudBlock>())
            {
                if (entity != this && entity != block && (entity.CollideRect(new Rectangle((int)block.X - 1, (int)block.Y, (int)block.Width + 2, (int)block.Height)) || entity.CollideRect(new Rectangle((int)block.X, (int)block.Y - 1, (int)block.Width, (int)block.Height + 2))) && !group.Contains(entity))
                {
                    group.Add(entity);
                    FindInGroup(entity);
                    entity.group = group;
                }
            }
        }

        public bool CanCloudSpawn()
        {
            bool playerIn = false;

            foreach (CloudBlock item in group)
            {
                if (item.CollideCheck<Player>())
                {
                    playerIn = true;
                    break;
                }
            }

            return playerIn;
        }

        private void RemoveTrampoline(MiniTrampoline miniTrampoline)
        {
            if (groupLeader) {
                trampolines.Remove(miniTrampoline);
            }
        }
    }
}
