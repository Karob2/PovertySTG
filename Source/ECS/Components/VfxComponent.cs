using Engine.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Components
{
    public class VfxComponent : Component
    {
        public float AlphaSpeed { get; set; }
        public float ScaleSpeed { get; set; }
        public float RotateSpeed { get; set; }

        public VfxComponent() { }

        /*
        public VfxComponent(float rotateSpeed, float alphaSpeed, float scaleSpeed)
        {
            AlphaSpeed = alphaSpeed;
            ScaleSpeed = scaleSpeed;
        }
        */

        public override void AttachTo(Entity entity) { entity.AddComponent(this); }
        public override void Remove() { Owner.RemoveComponent(this); }
    }
}
