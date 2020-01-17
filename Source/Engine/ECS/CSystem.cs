using Engine;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.ECS
{
    public abstract class CSystem
    {
        protected GameServices gs;
        protected Scene scene;
        public bool FirstRun { get; set; } = true;

        public void Initialize(GameServices gs, Scene scene)
        {
            this.gs = gs;
            this.scene = scene;
        }

        public virtual void Start() { }

        public virtual void Update(GameTime gameTime) { }
    }
}
