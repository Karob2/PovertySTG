using Engine;
using Engine.ECS;
using Engine.Resource;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Components
{
    public class SpriteComponent : Component
    {
        Sprite sprite;
        public Sprite Sprite
        {
            get => sprite;
            set
            {
                sprite = value;
                CurrentAnimation = value.DefaultAnimation;
            }
        }
        public string CurrentAnimation
        {
            get => CurrentAnimationObject.Name;
            //set => CurrentAnimationObject = sprite.Animations[value];
            set
            {
                if (value == null)
                {
                    CurrentAnimationObject = sprite.Animations[sprite.DefaultAnimation];
                }
                else if (sprite.Animations.TryGetValue(value, out Animation animationObject))
                {
                    CurrentAnimationObject = animationObject;
                }
                else
                {
                    CurrentAnimationObject = sprite.Animations[sprite.DefaultAnimation];
                }
            }
        }
        public Animation CurrentAnimationObject { get; set; }
        public int CurrentFrame { get; set; }
        public float CurrentTime { get; set; }
        public Color Color { get; set; } = Color.White;
        public float Rotation { get; set; } = 0f;
        public float Scale { get; set; } = 1f;
        public float Alpha { get; set; } = 1f;
        // TODO: Would it be acceptible to use public fields for components?

        public float X { get; set; }
        public float Y { get; set; }

        public bool Tiled { get; set; }
        public bool Stretched { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public SpriteComponent() { }

        public SpriteComponent(GameServices gs, string spriteName, string animationName = null)
        {
            sprite = gs.ResourceManager.Sprites.Get(spriteName);
            if (animationName != null) CurrentAnimation = animationName;
            else CurrentAnimation = null;
        }

        public override void AttachTo(Entity entity) { entity.AddComponent(this); }
        public override void Remove() { Owner.RemoveComponent(this); }
    }
}
