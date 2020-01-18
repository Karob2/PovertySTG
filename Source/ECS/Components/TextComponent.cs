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
        public float Border { get; set; } = 0.8f;
        public Alignment Align { get; set; }

        public TextComponent() { }

        public TextComponent(GameServices gs, string fontName, string text, Color color, Alignment align = Alignment.Left)
        {
            Font = gs.ResourceManager.Fonts.Get(fontName);
            Text = text;
            Color = color;
            Align = align;
        }

        public override void AttachTo(Entity entity) { entity.AddComponent(this); }
        public override void Remove() { Owner.RemoveComponent(this); }
    }
}
