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
    public class PlayerControlSystem : CSystem
    {
        SystemReferences sys;
        public PlayerControlSystem(SystemReferences sys) { this.sys = sys; }

        public override void Update(GameTime gameTime)
        {
            DanmakuUpdate(gameTime);
        }

        void DanmakuUpdate(GameTime gameTime)
        {
            foreach (PlayerComponent component in sys.PlayerComponents.EnabledList)
            {
                BodyComponent body = sys.BodyComponents.GetByOwner(component.Owner);
                Vector2 d = new Vector2(0, 0);
                if (InputManager.Held(GameCommand.Up)) d.Y -= 1;
                if (InputManager.Held(GameCommand.Down)) d.Y += 1;
                if (InputManager.Held(GameCommand.Left)) d.X -= 1;
                if (InputManager.Held(GameCommand.Right)) d.X += 1;
                if (d.Y != 0 || d.X != 0) d.Normalize();
                if (InputManager.Held(GameCommand.Action3)) d *= 2f;
                else d *= 6f;
                body.DX = d.X;
                body.DY = d.Y;
                SpriteComponent sprite = sys.SpriteComponents.GetByOwner(component.Owner);
                if (d.X < 0)
                {
                    sprite.CurrentAnimation = "left";
                }
                else if (d.X > 0)
                {
                    sprite.CurrentAnimation = "right";
                }
                else
                {
                    sprite.CurrentAnimation = "still";
                }

                if (component.Timer > 0)
                {
                    component.Timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                if (InputManager.Held(GameCommand.Action1) && component.Timer <= 0)
                {
                    DanmakuFactory.MakeBullet(scene, 0, body.X, body.Y - 30, 0, -7);
                    component.Timer = 0.16f;
                }
            }
        }
    }
}
