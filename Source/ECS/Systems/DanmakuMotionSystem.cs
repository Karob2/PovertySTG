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
                DanmakuFactory.MakeBullet(scene, -1, x, 0, 0, dy);
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
                if (body.X > -radius && body.X < Config.LevelWidth + radius
                    && body.Y > -radius && body.Y < Config.LevelHeight + radius)
                {
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
                                component.Owner.Delete();
                                enemy.Health -= component.Power;
                                if (enemy.Health <= 0)
                                {
                                    enemy.Owner.Delete();
                                    player.Score += 50;
                                }
                                else
                                {
                                    player.Score += 1;
                                }
                                continue;
                            }
                        }
                    }
                    if (component.Type == 1)
                    {
                        foreach (PlayerComponent enemy in sys.PlayerComponents.EnabledList)
                        {
                            BodyComponent targetBody = sys.BodyComponents.GetByOwner(enemy.Owner);
                            float targetRadius = sys.SpriteComponents.GetByOwner(enemy.Owner).Sprite.CollisionBox.Radius;
                            float dx = targetBody.X - body.X;
                            float dy = targetBody.Y - body.Y;
                            float dd = radius + targetRadius;
                            float distanceSquared = dx * dx + dy * dy;
                            if (distanceSquared < dd * dd)
                            {
                                DanmakuFactory.MakeSlash(scene, 1, body.X, body.Y);
                                component.Owner.Delete();
                                player.Lives--;
                                continue;
                            }
                        }
                    }
                }
            }
        }
    }
}
