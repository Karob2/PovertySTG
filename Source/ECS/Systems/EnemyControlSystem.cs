using Engine;
using Engine.ECS;
using Engine.Input;
using Engine.Resource;
using PovertySTG.ECS.Components;
using PovertySTG.Factories;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Systems
{
    public class EnemyControlSystem : CSystem
    {
        SystemReferences sys;
        public EnemyControlSystem(SystemReferences sys) { this.sys = sys; }

        public override void Update(GameTime gameTime)
        {
            foreach (EnemyComponent component in sys.EnemyComponents.EnabledList)
            {
                if (component.Phase == 0)
                {
                    float speed = 2.2f;
                    float dx = component.TargetX - component.X;
                    float dy = component.TargetY - component.Y;
                    Vector2 d = new Vector2(dx, dy);
                    if (d.LengthSquared() > speed * speed)
                    {
                        d.Normalize();
                        d *= speed;
                    }
                    else
                    {
                        component.Phase = 1;
                    }
                    component.DX = d.X;
                    component.DY = d.Y;
                }
                else if (component.Phase > 0 && component.Phase < 5)
                {
                    component.DX = 0;
                    component.DY = 0;
                    component.Timer += gameTime.ElapsedGameTime.TotalSeconds;
                    if (component.Timer > 1)
                    {
                        component.Timer -= 1;
                        DanmakuFactory.MakeBullet(gs, scene, 1, component.X, component.Y, 0, 3);
                        component.Phase++;
                    }
                }
                else if (component.Phase >= 5)
                {
                    component.DY = -2.2f;
                }
            }
        }
    }
}
