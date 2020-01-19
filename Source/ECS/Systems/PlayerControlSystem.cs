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
            sys.LevelScriptComponents.TryGetFirstEnabled(out LevelScriptComponent level);

            PlayerComponent component = sys.PlayerComponents.First();
            BodyComponent body = sys.BodyComponents.GetByOwner(component.Owner);
            SpriteComponent sprite = sys.SpriteComponents.GetByOwner(component.Owner);

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

            if (level.Story == null)
            {
                if (InputManager.Held(GameCommand.Action1) && component.Timer <= 0 && component.SpecialState < 3)
                {
                    DanmakuFactory.MakeBullet(scene, 0, body.X, body.Y - 30, 0, -7);
                    component.Timer = 0.16f;
                }

                if (component.SpecialState < 3)
                {
                    if (InputManager.JustPressed(GameCommand.Action1))
                    {
                        component.SpecialTimer = 0f;
                    }
                    else if (InputManager.Held(GameCommand.Action1))
                    {
                        component.SpecialTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                    {
                        if (component.SpecialTimer > 0f && component.SpecialTimer < 0.2f)
                        {
                            component.SpecialState++;
                            component.SpecialTimer2 = 0f;
                            if (component.SpecialState == 2 && component.Wealth >= 0.5f)
                            {
                                //DanmakuFactory.MakeBullet(scene, 0, body.X, body.Y - 30, 0, -14);
                                //component.SpecialState = 0;
                                component.SpecialState = 3;
                            }
                            component.SpecialTimer = 0f;
                        }
                    }
                }
            }
            if (component.SpecialState > 0 && component.SpecialState < 3)
            {
                component.SpecialTimer2 += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (component.SpecialTimer2 >= 0.2f)
                {
                    component.SpecialState = 0;
                }
            }
            if (component.SpecialState == 3)
            {
                component.SpecialTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (component.SpecialTimer > 0.1f)
                {
                    component.SpecialTimer = 0f;
                    component.Wealth -= 0.05f;
                    DanmakuFactory.MakeBullet(scene, 0, body.X, body.Y - 30, 0, -14);
                    if (component.Wealth <= 0f)
                    {
                        component.Wealth = 0f;
                        component.SpecialState = 0;
                        component.SpecialTimer = 0f;
                    }
                }
            }
        }
    }
}
