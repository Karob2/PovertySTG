using Engine.ECS;
using Engine.Resource;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Components
{
    public class CameraComponent : Component
    {
        public RenderComponent Target { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float DX { get; set; }
        public float DY { get; set; }

        public CameraComponent() { }

        public CameraComponent(RenderComponent target)
        {
            Target = target;
            X = target.X;
            Y = target.Y;
        }

        public override void AttachTo(Entity entity) { entity.AddComponent(this); }
        public override void Remove() { Owner.RemoveComponent(this); }
    }
}
