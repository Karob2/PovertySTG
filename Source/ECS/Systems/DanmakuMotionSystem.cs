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
        }
    }
}
