using Engine.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Components
{
    public class VfxComponent : Component
    {
        public float Lifespan { get; set; }
        public float Age { get; set; }

        public VfxComponent() { }

        public VfxComponent(float lifespan)
        {
            Lifespan = lifespan;
        }

        public override void AttachTo(Entity entity) { entity.AddComponent(this); }
        public override void Remove() { Owner.RemoveComponent(this); }
    }
}
