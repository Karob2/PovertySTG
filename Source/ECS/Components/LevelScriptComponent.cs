using Engine.ECS;
using Engine.Resource;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Components
{
    public class LevelScriptComponent : Component
    {
        public LevelScript Level { get; set; }
        public int Progress { get; set; }
        public float WaitTimer { get; set; }
        public LevelWaitMode WaitMode { get; set; }
        public int LoopPoint { get; set; }

        public LevelScriptComponent() { }

        public LevelScriptComponent(Scene scene, string level)
        {
            Level = scene.GS.ResourceManager.LevelScripts.Get(level);
        }

        public override void AttachTo(Entity entity) { entity.AddComponent(this); }
        public override void Remove() { Owner.RemoveComponent(this); }
    }
}
