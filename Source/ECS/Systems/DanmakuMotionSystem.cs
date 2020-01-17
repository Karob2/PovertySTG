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
            timer += gameTime.ElapsedGameTime.TotalSeconds;
            if (timer > 0.2)
            {
                timer -= 0.2;
                float x = (float)r.NextDouble() * Config.LevelWidth;
                float dy = (float)r.NextDouble() * 3 + 1;
                DanmakuFactory.MakeBullet(scene, -1, x, 0, 0, dy);
            }

            timer2 += gameTime.ElapsedGameTime.TotalSeconds;
            if (timer2 > 2)
            {
                timer2 -= 2;
                float x = (float)r.NextDouble() * Config.LevelWidth;
                float y = (float)r.NextDouble() * Config.LevelHeight / 2 + 20;
                DanmakuFactory.MakeEnemy(scene, 0, x * 1.5f, -32, x * 0.8f, y);
            }

            foreach (PlayerComponent component in sys.PlayerComponents.EnabledList)
            {
                component.X += component.DX;
                component.Y += component.DY;

                if (sys.RenderComponents.TryGetEnabled(component.Owner, out RenderComponent renderComponent))
                {
                    renderComponent.X = component.X;
                    renderComponent.Y = component.Y;
                }
            }

            foreach (EnemyComponent component in sys.EnemyComponents.EnabledList)
            {
                component.X += component.DX;
                component.Y += component.DY;

                int margin = 200;
                if (component.X < -margin || component.X > Config.LevelWidth + margin
                    || component.Y < -margin || component.Y > Config.LevelHeight + margin)
                {
                    component.Owner.Delete();
                }

                if (sys.RenderComponents.TryGetEnabled(component.Owner, out RenderComponent renderComponent))
                {
                    renderComponent.X = component.X;
                    renderComponent.Y = component.Y;
                }
            }

            foreach (BulletComponent component in sys.BulletComponents.EnabledList)
            {
                component.X += component.DX;
                component.Y += component.DY;

                // destroy bullet if far outside of game area
                int margin = 200;
                if (component.X < -margin || component.X > Config.LevelWidth + margin
                    || component.Y < -margin || component.Y > Config.LevelHeight + margin)
                {
                    component.Owner.Delete();
                    continue;
                }
                // Check for collisions.
                float radius = sys.SpriteComponents.GetByOwner(component.Owner).Sprite.CollisionBox.Radius;
                if (component.X > -radius && component.X < Config.LevelWidth + radius
                    && component.Y > -radius && component.Y < Config.LevelHeight + radius)
                {
                    if (component.Type == 0)
                    {
                        foreach (EnemyComponent enemy in sys.EnemyComponents.EnabledList)
                        {
                            float targetRadius = sys.SpriteComponents.GetByOwner(enemy.Owner).Sprite.CollisionBox.Radius;
                            float dx = enemy.X - component.X;
                            float dy = enemy.Y - component.Y;
                            float dd = radius + targetRadius;
                            float distanceSquared = dx * dx + dy * dy;
                            if (distanceSquared < dd * dd)
                            {
                                component.Owner.Delete();
                                //enemy.Owner.Delete();
                                continue;
                            }
                        }
                    }
                    if (component.Type == 1)
                    {
                        foreach (PlayerComponent enemy in sys.PlayerComponents.EnabledList)
                        {
                            float targetRadius = sys.SpriteComponents.GetByOwner(enemy.Owner).Sprite.CollisionBox.Radius;
                            float dx = enemy.X - component.X;
                            float dy = enemy.Y - component.Y;
                            float dd = radius + targetRadius;
                            float distanceSquared = dx * dx + dy * dy;
                            if (distanceSquared < dd * dd)
                            {
                                component.Owner.Delete();
                                continue;
                            }
                        }
                    }
                }
                // sync render location
                if (sys.RenderComponents.TryGetEnabled(component.Owner, out RenderComponent renderComponent))
                {
                    renderComponent.X = component.X;
                    renderComponent.Y = component.Y;
                }
            }
        }
    }
}
