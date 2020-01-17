using Engine;
using Engine.ECS;
using Engine.Input;
using Engine.Resource;
using PovertySTG.ECS.Components;
using PovertySTG.Factories;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Systems
{
    public class MotionSystem : CSystem
    {
        SystemReferences sys;
        public MotionSystem(SystemReferences sys) { this.sys = sys; }

        public override void Update(GameTime gameTime)
        {
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
