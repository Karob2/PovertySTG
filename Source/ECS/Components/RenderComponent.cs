using Engine.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Components
{
    public class RenderComponent : Component
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int Layer { get; set; }
        public float Depth { get; set; }
        public bool Camera { get; set; }
        public float DisplayX { get; set; }
        public float DisplayY { get; set; }
        public RenderComponent Leader { get; set; }
        // render bounding box?
        // spacial collection?
        // visible bool?
        // Camera

        public RenderComponent() { }

        public RenderComponent(int layer, float depth, bool camera = false)
        {
            Camera = camera;
            Layer = layer;
            Depth = depth;
        }

        public RenderComponent(float x, float y, int layer, float depth, bool camera = false)
        {
            Camera = camera;
            X = x;
            Y = y;
            Layer = layer;
            Depth = depth;
        }

        public override void AttachTo(Entity entity) { entity.AddComponent(this); }
        public override void Remove() { Owner.RemoveComponent(this); }
    }
}
