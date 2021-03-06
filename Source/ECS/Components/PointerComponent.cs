﻿using Engine.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Components
{
    public class PointerComponent : Component
    {
        public override void AttachTo(Entity entity) { entity.AddComponent(this); }
        public override void Remove() { Owner.RemoveComponent(this); }
    }
}
