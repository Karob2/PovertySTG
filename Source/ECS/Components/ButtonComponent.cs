using Engine.ECS;
using Engine.Resource;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Components
{
    public class ButtonComponent : Component
    {
        // TODO: Are these worth keeping? I think they're only needed during scene construction.
        public int X { get; set; }
        public int Y { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
        public Action Action { get; set; }

        public bool Initialized { get; set; }
        public bool Focused { get; set; }
        public bool Disabled { get; set; }

        TextComponent textComponent;
        SpriteComponent spriteComponent;
        public TextComponent TextComponent => textComponent;
        public SpriteComponent SpriteComponent => spriteComponent;

        //public string Name { get; set; }
        public string PressCommand { get; set; }
        public string LeftCommand { get; set; }
        public string RightCommand { get; set; }

        public Color DefaultTextColor { get; set; } = Color.DarkCyan;
        public Color FocusedTextColor { get; set; } = Color.White;
        public Color PressedTextColor { get; set; } = Color.Blue;
        public Color DisabledTextColor { get; set; } = Color.Gray;
        public Color DefaultColor { get; set; } = Color.White;
        public Color FocusedColor { get; set; } = Color.White;
        public Color PressedColor { get; set; } = Color.White;
        public Color DisabledColor { get; set; } = Color.White;
        public Animation DefaultAnimation { get; set; }
        public Animation FocusedAnimation { get; set; }
        public Animation PressedAnimation { get; set; }
        public Animation DisabledAnimation { get; set; }

        public ButtonComponent Up { get; set; }
        public ButtonComponent Down { get; set; }
        public ButtonComponent Left { get; set; }
        public ButtonComponent Right { get; set; }

        /*
        public ButtonComponent(string name = null)
        {
            Name = name;
        }

        public ButtonComponent(TextComponent textComponent, string name = null)
        {
            Name = name;
            AddTextFeedback(textComponent);
        }

        public ButtonComponent(SpriteComponent spriteComponent, string name = null)
        {
            Name = name;
            AddSpriteFeedback(spriteComponent);
        }
        */

        public ButtonComponent(Entity entity, string pressCommand = null, string leftCommand = null, string rightCommand = null)
        {
            //Name = name;
            PressCommand = pressCommand;
            LeftCommand = leftCommand;
            RightCommand = rightCommand;

            ComponentGroup<TextComponent> textComponents = entity.Scene.GetComponentGroup<TextComponent>();
            TextComponent textComponent;
            if (textComponents.TryGetByOwner(entity, out textComponent))
            {
                AddTextFeedback(textComponent);
            }

            ComponentGroup<SpriteComponent> spriteComponents = entity.Scene.GetComponentGroup<SpriteComponent>();
            SpriteComponent spriteComponent;
            if (spriteComponents.TryGetByOwner(entity, out spriteComponent))
            {
                AddSpriteFeedback(spriteComponent);
            }
        }

        public void AddTextFeedback(TextComponent textComponent)
        {
            this.textComponent = textComponent;

            Vector2 buttonArea = textComponent.Font.MeasureString(textComponent.Text);
            Width = (int)buttonArea.X;
            Height = (int)buttonArea.Y;
        }

        public void AddSpriteFeedback(SpriteComponent spriteComponent)
        {
            this.spriteComponent = spriteComponent;

            this.DefaultAnimation = spriteComponent.CurrentAnimationObject;
            string name = spriteComponent.CurrentAnimation;
            Dictionary<string, Animation> animations = spriteComponent.Sprite.Animations;
            Animation animation;

            if (animations.TryGetValue(name + "_focused", out animation))
                FocusedAnimation = animation;
            else
                FocusedAnimation = DefaultAnimation;

            if (animations.TryGetValue(name + "_pressed", out animation))
                PressedAnimation = animation;
            else
                PressedAnimation = DefaultAnimation;

            if (animations.TryGetValue(name + "_disabled", out animation))
                DisabledAnimation = animation;
            else
                DisabledAnimation = DefaultAnimation;

            Rectangle buttonArea = spriteComponent.CurrentAnimationObject.Bounds;
            X = buttonArea.X;
            Y = buttonArea.Y;
            Width = buttonArea.Width;
            Height = buttonArea.Height;
        }

        public override void AttachTo(Entity entity) { entity.AddComponent(this); }
        public override void Remove() { Owner.RemoveComponent(this); }
    }
}
