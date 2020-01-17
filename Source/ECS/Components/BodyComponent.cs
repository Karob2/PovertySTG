using Engine.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Components
{
    public class BodyComponent : Component
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float DX { get; set; }
        public float DY { get; set; }
        public float Lifespan { get; set; }
        public float Age { get; set; }
        public float DeathMargin { get; set; } = 100f;

        public BodyComponent() { }

        public BodyComponent(float x, float y)
        {
            X = x;
            Y = y;
        }

        public BodyComponent(float x, float y, float dx, float dy)
        {
            X = x;
            Y = y;
            DX = dx;
            DY = dy;
        }

        public override void AttachTo(Entity entity) { entity.AddComponent(this); }
        public override void Remove() { Owner.RemoveComponent(this); }
    }
}
