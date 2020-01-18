using Engine.ECS;
using Engine.Resource;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Components
{
    public class TalkComponent : Component
    {
        public Story Story { get; set; }
        public int Progress { get; set; }

        public TalkComponent() { }

        public override void AttachTo(Entity entity) { entity.AddComponent(this); }
        public override void Remove() { Owner.RemoveComponent(this); }
    }
}
