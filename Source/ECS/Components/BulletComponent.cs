using Engine.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Components
{
    public class BulletComponent : Component
    {
        public int Type { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float DX { get; set; }
        public float DY { get; set; }

        public BulletComponent() { }

        public BulletComponent(int type, float x, float y, float dx, float dy)
        {
            Type = type;
            X = x;
            Y = y;
            DX = dx;
            DY = dy;
        }

        public override void AttachTo(Entity entity) { entity.AddComponent(this); }
        public override void Remove() { Owner.RemoveComponent(this); }
    }
}
