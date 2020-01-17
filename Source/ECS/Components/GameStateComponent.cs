using Engine.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Components
{
    public class GameStateComponent : Component
    {
        public string Command { get; set; }
        public int Priority { get; set; }

        public void Request(string name, int priority = 0)
        {
            if (Command == null || priority > Priority)
            {
                Command = name;
                Priority = priority;
            }
        }

        public override void AttachTo(Entity entity) { entity.AddComponent(this); }
        public override void Remove() { Owner.RemoveComponent(this); }
    }
}
