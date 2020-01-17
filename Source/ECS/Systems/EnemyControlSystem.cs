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
                BodyComponent body = sys.BodyComponents.GetByOwner(component.Owner);
                if (component.Phase == 0)
                {
                    float speed = 3.8f;
                    float dx = component.TargetX - body.X;
                    float dy = component.TargetY - body.Y;
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
                    body.DX = d.X;
                    body.DY = d.Y;
                }
                else if (component.Phase > 0 && component.Phase < 6)
                {
                    body.DX = 0;
                    body.DY = 0;
                    component.Timer += gameTime.ElapsedGameTime.TotalSeconds;
                    if (component.Timer > 1)
                    {
                        component.Timer -= 1;
                        DanmakuFactory.MakeBullet(scene, 1, body.X, body.Y, 0, 3);
                        component.Phase++;
                    }
                }
                else if (component.Phase >= 5)
                {
                    body.DY = -2.2f;
                }
            }
        }
    }
}
