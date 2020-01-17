using Engine.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Components
{
    public class PlayerComponent : Component
    {
        public int Score { get; set; }
        public int Lives { get; set; }
        public int Bombs { get; set; }
        public float Power { get; set; }
        public double Timer { get; set; }

        public override void AttachTo(Entity entity) { entity.AddComponent(this); }
        public override void Remove() { Owner.RemoveComponent(this); }
    }
}
