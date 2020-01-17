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
                float x = (float)r.NextDouble() * gs.DisplayManager.GameWidth;
                float dy = (float)r.NextDouble() * 3 + 1;
                DanmakuFactory.MakeBullet(gs, scene, -1, x, 0, 0, dy);
            }

            timer2 += gameTime.ElapsedGameTime.TotalSeconds;
            if (timer2 > 2)
            {
                timer2 -= 2;
                float x = (float)r.NextDouble() * gs.DisplayManager.GameWidth;
                float y = (float)r.NextDouble() * gs.DisplayManager.GameWidth / 2 + 20;
                DanmakuFactory.MakeEnemy(gs, scene, 0, x * 1.5f, -32, x * 0.8f, y);
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
                if (component.X < -margin || component.X > gs.DisplayManager.GameWidth + margin
                    || component.Y < -margin || component.Y > gs.DisplayManager.GameHeight + margin)
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
                if (component.X < -margin || component.X > gs.DisplayManager.GameWidth + margin
                    || component.Y < -margin || component.Y > gs.DisplayManager.GameHeight + margin)
                {
                    component.Owner.Delete();
                }
                // sync render location
                else if (sys.RenderComponents.TryGetEnabled(component.Owner, out RenderComponent renderComponent))
                {
                    renderComponent.X = component.X;
                    renderComponent.Y = component.Y;
                }
            }
        }
    }
}
