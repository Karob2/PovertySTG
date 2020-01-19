using Engine;
using Engine.ECS;
using Engine.Input;
using Engine.Resource;
using PovertySTG.ECS.Components;
using PovertySTG.Factories;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Text;
using System;

namespace PovertySTG.ECS.Systems
{
    public class MotionSystem : CSystem
    {
        SystemReferences sys;
        public MotionSystem(SystemReferences sys) { this.sys = sys; }

        double timer = 0;
        double timer2 = 0;
        Random r = new Random();

        public override void Update(GameTime gameTime)
        {
            sys.PlayerComponents.TryGetFirstEnabled(out PlayerComponent player);
            //BodyComponent playerBody = sys.BodyComponents.GetByOwner(player.Owner);

            timer += gameTime.ElapsedGameTime.TotalSeconds;
            if (timer > 0.2)
            {
                timer -= 0.2;
                float x = (float)r.NextDouble() * Config.LevelWidth;
                float dy = (float)r.NextDouble() * 3 + 1;
                DanmakuFactory.MakeBullet(scene, -1000, x, 0, 0, dy);
            }

            /*
            timer2 += gameTime.ElapsedGameTime.TotalSeconds;
            if (timer2 > 2)
            {
                timer2 -= 2;
                float x = (float)r.NextDouble() * Config.LevelWidth;
                float y = (float)r.NextDouble() * Config.LevelHeight / 2 + 20;
                DanmakuFactory.MakeEnemy(scene, 0, x * 1.5f, -32, x * 0.8f, y);
            }
            */

            foreach (BodyComponent component in sys.BodyComponents.EnabledList)
            {
                component.X += component.DX;
                component.Y += component.DY;
                component.DX += component.DDX;
                component.DY += component.DDY;

                if (component.Pen != Vector4.Zero)
                {
                    float x = component.Pen.X * Config.LevelWidth;
                    float y = component.Pen.Y * Config.LevelHeight;
                    float x2 = component.Pen.Z * Config.LevelWidth;
                    float y2 = component.Pen.W * Config.LevelHeight;
                    if (component.Owner.HasTag("player"))
                    {
                        if (component.X < x || component.X > x2)
                        {
                            sys.SpriteComponents.TryGetByOwner(component.Owner, out SpriteComponent sprite);
                            sprite.CurrentAnimation = "still";
                        }
                    }
                    if (component.X < x) component.X = x;
                    if (component.X > x2) component.X = x2;
                    if (component.Y < y) component.Y = y;
                    if (component.Y > y2) component.Y = y2;
                }

                float margin = component.DeathMargin;
                if (margin > 0)
                {
                    if (component.X < -margin || component.X > Config.LevelWidth + margin
                        || component.Y < -margin || component.Y > Config.LevelHeight + margin)
                    {
                        component.Owner.Delete();
                        continue;
                    }
                }

                if (component.Lifespan > 0)
                {
                    component.Age += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (component.Age >= component.Lifespan)
                    {
                        component.Owner.Delete();
                        continue;
                    }
                }

                if (sys.RenderComponents.TryGetEnabled(component.Owner, out RenderComponent renderComponent))
                {
                    renderComponent.X = component.X;
                    renderComponent.Y = component.Y;
                }
            }

            foreach (BulletComponent component in sys.BulletComponents.EnabledList)
            {
                BodyComponent body = sys.BodyComponents.GetByOwner(component.Owner);
                // Check for collisions.
                float radius = sys.SpriteComponents.GetByOwner(component.Owner).Sprite.CollisionBox.Radius;
                if (component.Type == -100 && body.Y > Config.LevelHeight && body.DY > 0)
                {
                    body.DY = -body.DY * 0.5f;
                    component.Type = -101;
                }
                if (body.X > -radius && body.X < Config.LevelWidth + radius
                    && body.Y > -radius && body.Y < Config.LevelHeight + radius)
                {
                    if (component.Type <= -100 && component.Type > -1000)
                    {
                        BodyComponent targetBody = sys.BodyComponents.GetByOwner(player.Owner);
                        float targetRadius = 100;
                        float targetRadius2 = sys.SpriteComponents.GetByOwner(player.Owner).Sprite.CollisionBox.Radius;
                        float dx = targetBody.X - body.X;
                        float dy = targetBody.Y - body.Y;
                        float dd = radius + targetRadius;
                        float dd2 = radius + targetRadius2;
                        float distanceSquared = dx * dx + dy * dy;
                        if (distanceSquared < dd2 * dd2)
                        {
                            component.Owner.Delete();
                            player.Power += 0.1f;
                            continue;
                        }
                        if (distanceSquared < dd * dd)
                        {
                            //body.DX = body.DX * 0.7f;
                            //body.DY = body.DY * 0.7f;
                            float speed = 8f;
                            float distance = (float)Math.Sqrt(distanceSquared) / speed;
                            body.DX = body.DX + (dx / distance - body.DX) * 0.3f;
                            body.DY = body.DY + (dy / distance - body.DY) * 0.3f;
                            continue;
                        }
                        else if (component.Type == -102 || component.Type == -103)
                        {
                            body.DX = body.DX * (1 - 0.08f);
                            body.DY = body.DY + (4f - body.DY) * 0.08f;
                        }
                    }
                    if (component.Type == 0)
                    {
                        foreach (EnemyComponent enemy in sys.EnemyComponents.EnabledList)
                        {
                            BodyComponent targetBody = sys.BodyComponents.GetByOwner(enemy.Owner);
                            float targetRadius = sys.SpriteComponents.GetByOwner(enemy.Owner).Sprite.CollisionBox.Radius;
                            float dx = targetBody.X - body.X;
                            float dy = targetBody.Y - body.Y;
                            float dd = radius + targetRadius;
                            float distanceSquared = dx * dx + dy * dy;
                            if (distanceSquared < dd * dd)
                            {
                                DanmakuFactory.MakeSlash(scene, 0, body.X, body.Y);
                                DanmakuFactory.MakeCoin(scene, body.X, body.Y);
                                component.Owner.Delete();
                                enemy.Health -= component.Power;
                                if (enemy.Health <= 0)
                                {
                                    DanmakuFactory.MakePointItem(scene, body.X, body.Y);
                                    DanmakuFactory.MakePointItem(scene, body.X, body.Y);
                                    DanmakuFactory.MakePointItem(scene, body.X, body.Y);
                                    DanmakuFactory.MakePowerItem(scene, body.X, body.Y);
                                    DanmakuFactory.MakePowerItem(scene, body.X, body.Y);
                                    DanmakuFactory.MakePowerItem(scene, body.X, body.Y);
                                    enemy.Owner.Delete();
                                    player.Score += 50;
                                }
                                else
                                {
                                    player.Score += 1;
                                    player.Power += 0.1f;
                                }
                                continue;
                            }
                        }
                    }
                    if (component.Type == 1)
                    {
                        BodyComponent targetBody = sys.BodyComponents.GetByOwner(player.Owner);
                        float targetRadius = sys.SpriteComponents.GetByOwner(player.Owner).Sprite.CollisionBox.Radius;
                        float dx = targetBody.X - body.X;
                        float dy = targetBody.Y - body.Y;
                        float dd = radius + targetRadius;
                        float distanceSquared = dx * dx + dy * dy;
                        if (distanceSquared < dd * dd)
                        {
                            DanmakuFactory.MakeSlash(scene, 1, body.X, body.Y);
                            component.Owner.Delete();
                            player.Lives--;
                            player.Power -= 0.1f;
                            if (player.Lives < 0) player.Lives = 8;
                            continue;
                        }
                    }
                }
            }

            player.Power = Math.Min(Math.Max(player.Power, 0), 1);
        }
    }
}
