using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.ECS
{
    public abstract class Component
    {
        public Entity Owner { get; set; }
        public bool Enabled { get; set; }

        public abstract void AttachTo(Entity entity);
        public abstract void Remove();

        public Component Clone()
        {
            return (Component)this.MemberwiseClone();
        }
    }
}
