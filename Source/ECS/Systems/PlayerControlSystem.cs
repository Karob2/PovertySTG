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
            if (scene.Name == "danmaku") DanmakuUpdate(gameTime);
            else PlatformerUpdate(gameTime);
        }

        void DanmakuUpdate(GameTime gameTime)
        {
            foreach (PlayerComponent component in sys.PlayerComponents.EnabledList)
            {
                Vector2 d = new Vector2(0, 0);
                if (InputManager.Held(GameCommand.Up)) d.Y -= 1;
                if (InputManager.Held(GameCommand.Down)) d.Y += 1;
                if (InputManager.Held(GameCommand.Left)) d.X -= 1;
                if (InputManager.Held(GameCommand.Right)) d.X += 1;
                if (d.Y != 0 || d.X != 0) d.Normalize();
                d *= 2.2f;
                component.DX = d.X;
                component.DY = d.Y;

                if (InputManager.JustPressed(GameCommand.Action1))
                {
                }
            }
        }

        void PlatformerUpdate(GameTime gameTime)
        {
            foreach (PlayerComponent component in sys.PlayerComponents.EnabledList)
            {
                float dx = 0;
                if (InputManager.Held(GameCommand.Left)) dx -= 1;
                if (InputManager.Held(GameCommand.Right)) dx += 1;
                dx *= 2.2f;
                float dy = component.DY;
                if (InputManager.Held(GameCommand.Action2)) dy = -4;
                component.DX = dx;
                component.DY = dy;
            }
        }
    }
}
