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
        public float Point { get; set; }
        public float Power { get; set; }
        public float Wealth { get; set; }
        public double Timer { get; set; }
        public float SpecialTimer { get; set; }
        public float SpecialTimer2 { get; set; }
        public int SpecialState { get; set; }
        //public bool SpecialActive { get; set; }
        public int Alternate { get; set; }
        public int Alternate2 { get; set; }
        public double Timer2 { get; set; }
        public float Invuln { get; set; }

        public override void AttachTo(Entity entity) { entity.AddComponent(this); }
        public override void Remove() { Owner.RemoveComponent(this); }
    }
}
