using Engine;
using Engine.ECS;
using Engine.Resource;
using PovertySTG.ECS.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PovertySTG.ECS.Systems
{
    public class RenderSystem : CSystem
    {
        SystemReferences sys;
        public RenderSystem(SystemReferences sys) { this.sys = sys; }
        Vector2 cameraPos = Vector2.Zero;
        List<RenderItem> renderList = new List<RenderItem>();

        public override void Update(GameTime gameTime)
        {
            int index = 0;
            renderList.Clear();

            if (sys.CameraComponents.TryGetFirstEnabled(out CameraComponent camera))
            {
                cameraPos = new Vector2(camera.X, camera.Y);
            }

            foreach (RenderComponent renderComponent in sys.RenderComponents.EnabledList)
            {
                renderComponent.DisplayX = renderComponent.X;
                renderComponent.DisplayY = renderComponent.Y;
                if (renderComponent.Camera != null)
                {
                    renderComponent.DisplayX += gs.DisplayManager.GameWidth / 2 - renderComponent.Camera.X;
                    renderComponent.DisplayY += gs.DisplayManager.GameHeight / 2 - renderComponent.Camera.Y;
                }
                if (sys.SpriteComponents.TryGetEnabled(renderComponent.Owner, out SpriteComponent spriteComponent))
                {
                    //RenderSpriteComponent(gameTime, scene, renderComponent, spriteComponent);
                    if (!OutOfScreen(renderComponent, spriteComponent))
                    {
                        renderList.Add(new RenderItem(renderComponent, spriteComponent, index));
                        index++;
                    }
                }
                else if (sys.TextComponents.TryGetEnabled(renderComponent.Owner, out TextComponent textComponent))
                {
                    // TODO: Cull offscreen text?
                    //RenderTextComponent(gameTime, scene, renderComponent, textComponent);
                    renderList.Add(new RenderItem(renderComponent, textComponent, index));
                    index++;
                }
            }

            renderList = renderList.OrderByDescending(o => o.layer).ThenByDescending(o => o.depth).ThenBy(o => o.index).ToList();

            foreach (RenderItem item in renderList)
            {
                if (item.spriteComponent != null)
                {
                    RenderSpriteComponent(gameTime, scene, item.renderComponent, item.spriteComponent);
                    continue;
                }
                if (item.textComponent != null)
                {
                    RenderTextComponent(gameTime, scene, item.renderComponent, item.textComponent);
                    continue;
                }
            }

#if DEBUG
            RenderDebugInfo();
#endif
        }

        public bool OutOfScreen(RenderComponent renderComponent, SpriteComponent spriteComponent)
        {
            Rectangle bounds = spriteComponent.CurrentAnimationObject.Bounds;
            if (renderComponent.DisplayX + bounds.X >= gs.DisplayManager.GameWidth) return true;
            if (renderComponent.DisplayX + bounds.X + bounds.Width < 0) return true;
            if (renderComponent.DisplayY + bounds.Y >= gs.DisplayManager.GameHeight) return true;
            if (renderComponent.DisplayY + bounds.Y + bounds.Height < 0) return true;
            return false;
        }

        public void RenderTextComponent(GameTime gameTime, Scene scene, RenderComponent renderComponent, TextComponent component)
        {
            if (component.Text == null) return;

            Font font = component.Font;
            int oldLineSpacing = font.LineSpacing;
            if (component.LineSpacing != 0) font.LineSpacing = component.LineSpacing;

            if (component.Width > 0)
            {
                string text2 = BuildString(component);
                Vector2 size = font.MeasureString(text2);
                //Vector2 position = GetDisplayPosition(size);
                //font.Render(text2, position.X, position.Y, component.Color);
                font.Render(text2, renderComponent.DisplayX, renderComponent.DisplayY, component.Color);
            }
            else
            {
                Vector2 size = font.MeasureString(component.Text);
                //Vector2 position = GetDisplayPosition(size);
                //font.Render(component.Text, position.X, position.Y, component.Color);
                font.Render(component.Text, renderComponent.DisplayX, renderComponent.DisplayY, component.Color);
            }

            if (component.LineSpacing != 0) font.LineSpacing = oldLineSpacing;
        }

        /*
        public Vector2 MeasureString(bool tight = false)
        {
            int oldLineSpacing = font.SpriteFont.LineSpacing;
            if (LineSpacing != 0) font.SpriteFont.LineSpacing = LineSpacing;

            Vector2 result;
            if (width <= 0) result = font.MeasureString(text);
            else
            {
                string text2 = BuildString();

                if (tight)
                {
                    int cutoff = text2.Length;
                    while (cutoff > 0)
                    {
                        char last = text2[cutoff - 1];
                        if (last != ' ' && last != '\n' && last != '\r') break;
                        cutoff--;
                    }
                    if (cutoff != text2.Length) text2 = text2.Substring(0, cutoff);
                }

                result = font.MeasureString(text2);
            }

            if (LineSpacing != 0) font.SpriteFont.LineSpacing = oldLineSpacing;
            return result;
        }
        */
        public string BuildString(TextComponent component)
        {
            Font font = component.Font;
            string text = component.Text;
            int width = component.Width;
            StringBuilder sb = new StringBuilder();
            float spaceWidth = font.MeasureString(" ").X;
            bool extraLine = false;
            if (text.Length > 0)
            {
                char lastChar = text[text.Length - 1];
                if (lastChar == '\n' || lastChar == '\r') extraLine = true;
            }

            using (StringReader sr = new StringReader(text))
            {
                string line;
                string nextLine = sr.ReadLine();

                while (true)
                {
                    line = nextLine;
                    nextLine = sr.ReadLine();
                    if (line == null) break;
                    string[] words = line.Split(' ');
                    float lineWidth = 0f;

                    for (int n = 0; n < words.Length; n++)
                    {
                        string word = words[n];
                        Vector2 wordSize = font.MeasureString(word);

                        if (lineWidth + wordSize.X < width)
                        {
                            sb.Append(word);
                            lineWidth += wordSize.X;
                        }
                        else
                        {
                            sb.Append("\n" + word);
                            lineWidth = wordSize.X;
                        }

                        if (lineWidth + spaceWidth < width && n < words.Length - 1)
                        {
                            sb.Append(" ");
                            lineWidth += spaceWidth;
                        }
                    }
                    if (nextLine == null) break;
                    sb.Append("\n");
                }
            }
            if (extraLine) sb.Append("\n");
            return sb.ToString();
        }

        // TODO: Should I really not just use X and Y from spriteComponent directly?
        public void RenderSpriteComponent(GameTime gameTime, Scene scene, RenderComponent renderComponent, SpriteComponent component)
        {
            Sprite sprite = component.Sprite;
            // TODO: Updating the animation here makes animations stall when invisible, and can make them less precise when Update() happens more often than Render(), which is significant when events are triggered by animation state. Consider adding capability of animation progress to be Update()-driven.
            //Animation animation = sprite.Animations[component.CurrentAnimation];
            Animation animation = component.CurrentAnimationObject;
            if (component.CurrentFrame >= animation.Frames.Count) component.CurrentFrame %= animation.Frames.Count;

            if (component.Stretched)
            {
                sprite.RenderStretched(renderComponent.DisplayX, renderComponent.DisplayY, component.Width, component.Height, animation, component.CurrentFrame, component.Color);
            }
            else if (component.Tiled)
            {
                sprite.RenderTiled(renderComponent.DisplayX, renderComponent.DisplayY, component.Width, component.Height, animation, component.CurrentFrame, component.Color);
            }
            else
            {
                sprite.Render(renderComponent.DisplayX, renderComponent.DisplayY, animation, component.CurrentFrame, component.Color, component.Rotation);
            }

            if (animation.Frames.Count > 1)
            {
                float frameTime = component.CurrentTime;
                float frameIncrement = (float)gameTime.ElapsedGameTime.TotalSeconds * animation.Speed;
                if (scene != null) frameIncrement *= scene.AnimationSpeed;
                frameTime += frameIncrement;
                float maxFrameTime = animation.Frames[component.CurrentFrame].Frametime;
                if (frameTime >= maxFrameTime)
                {
                    frameTime -= maxFrameTime;
                    if (frameTime >= maxFrameTime) frameTime = maxFrameTime; // to avoid issues at super fast animation speeds
                    component.CurrentFrame++;
                }
                component.CurrentTime = frameTime;
            }
        }

#if DEBUG
        public void RenderDebugInfo()
        {
            int entities = scene.EntityList.Count;
            int activeEntities = 0;
            int components = 0;
            int activeComponents = 0;
            int bulletComponents = sys.BulletComponents.List.Count;
            int activeBulletComponents = sys.BulletComponents.EnabledList.Count();
            foreach (Entity entity in scene.EntityList)
            {
                if (entity.Enabled) activeEntities++;
                components += entity.ComponentReferences.Count;
                foreach (Component component in entity.ComponentReferences)
                {
                    if (component.Enabled) activeComponents++;
                }
            }
            string text = "Entities: " + activeEntities + "/" + entities + " Free: " + scene.FreeEntities.Count + " Visible: " + renderList.Count;
            text += "\nComponents: " + activeComponents + "/" + components;
            text += "\nBullets: " + activeBulletComponents + "/" + bulletComponents;
            gs.ResourceManager.Fonts.Get("sysfont").Render(text, 0, 0, Color.White);
        }
#endif
    }

    /*
    public class RenderItem
    {
        public RenderComponent RenderComponent { get; set; }
        public SpriteComponent Sprite
    }
    */
    public struct RenderItem
    {
        public RenderComponent renderComponent;
        public SpriteComponent spriteComponent;
        public TextComponent textComponent;
        public int index;
        public int layer;
        public float depth;

        public RenderItem(RenderComponent renderComponent, SpriteComponent spriteComponent, int index)
        {
            this.renderComponent = renderComponent;
            this.spriteComponent = spriteComponent;
            this.textComponent = null;
            this.index = index;
            this.layer = renderComponent.Layer;
            this.depth = renderComponent.Depth;
        }

        public RenderItem(RenderComponent renderComponent, TextComponent textComponent, int index)
        {
            this.renderComponent = renderComponent;
            this.spriteComponent = null;
            this.textComponent = textComponent;
            this.index = index;
            this.layer = renderComponent.Layer;
            this.depth = renderComponent.Depth;
        }
    }
}
