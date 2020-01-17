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
    public class VfxSystem : CSystem
    {
        SystemReferences sys;
        public VfxSystem(SystemReferences sys) { this.sys = sys; }

        public override void Update(GameTime gameTime)
        {
            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (VfxComponent vfx in sys.VfxComponents.EnabledList)
            {
                //BodyComponent body = sys.BodyComponents.GetByOwner(vfx.Owner);
                SpriteComponent sprite = sys.SpriteComponents.GetByOwner(vfx.Owner);
                sprite.Scale += seconds * vfx.ScaleSpeed;
                sprite.Alpha += seconds * vfx.ScaleSpeed;
                sprite.Rotation += seconds * vfx.RotateSpeed;
            }
        }
    }
}
