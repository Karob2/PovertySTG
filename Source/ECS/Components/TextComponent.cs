using Engine;
using Engine.ECS;
using Engine.Resource;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Components
{
    public class TextComponent : Component
    {
        public Font Font { get; set; }
        public string Text { get; set; }
        public int LineSpacing { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Color Color { get; set; }

        public TextComponent() { }

        public TextComponent(GameServices gs, string fontName, string text, Color color)
        {
            Font = gs.ResourceManager.Fonts.Get(fontName);
            Text = text;
            Color = color;
        }

        public override void AttachTo(Entity entity) { entity.AddComponent(this); }
        public override void Remove() { Owner.RemoveComponent(this); }
    }
}
