using Engine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Engine.Resource
{
    /// <summary>
    /// Defines the properties and animations of a sprite, and can render the sprite.
    /// </summary>
    public class Sprite
    {
        GameServices gs;
        SpriteBatch spriteBatch;
        public CollisionBox CollisionBox { get; set; }
        //public Frame DefaultFrame { get; set; }
        public string DefaultAnimation { get; set; }
        public Animation DefaultAnimationObject => Animations[DefaultAnimation];
        public Dictionary<string, Animation> Animations { get; set; }

        //public Sprite() { }

        public Sprite(GameServices gs)
        {
            this.gs = gs;
            spriteBatch = gs.DisplayManager.SpriteBatch;
        }

        public Sprite(GameServices gs, string name, Texture2D texture)
        {
            this.gs = gs;
            spriteBatch = gs.DisplayManager.SpriteBatch;
            DefaultAnimation = name;

            Frame frame = new Frame();
            //frame.Spritesheet = texture.Name;
            frame.X = 0;
            frame.Y = 0;
            frame.Width = texture.Width;
            frame.Height = texture.Height;
            frame.AnchorX = 0;
            frame.AnchorY = 0;
            frame.Frametime = 1f;
            frame.Texture = texture;

            Animation animation = new Animation();
            animation.Name = DefaultAnimation;
            animation.Speed = 0;
            animation.Frames = new List<Frame> { frame };
            animation.CalculateBounds();

            Animations = new Dictionary<string, Animation> { { DefaultAnimation, animation } };
        }

        /// <summary>
        /// Load a sprite from a file.
        /// </summary>
        /// <param name="path">Path to JSON sprite file.</param>
        /// <param name="textureLibrary">ResourceLibrary for storing textures.</param>
        public Sprite(GameServices gs, string path, TextureLibrary textureLibrary)
        {
            this.gs = gs;
            spriteBatch = gs.DisplayManager.SpriteBatch;
            _Sprite _sprite = JsonHelper<_Sprite>.Load(path);
            _sprite.Finalize(Path.GetDirectoryName(path), textureLibrary);
            _sprite.Solidify(this);
            //TODO: Will _sprite be automatically garbage collected?
        }

        /*
        public static Sprite NewSprite(string path, TextureLibrary textureLibrary)
        {
            _Sprite _sprite = JsonHelper<_Sprite>.Load(path);
            _sprite.Finalize(Path.GetDirectoryName(path), textureLibrary);
            Sprite sprite = _sprite.Solidify();
            return sprite;
        }
        */

        /// <summary>
        /// Render the sprite without animation.
        /// </summary>
        public void Render(float x, float y)
        {
            Animation animation = Animations[DefaultAnimation];
            Render(x, y, animation, 0, Color.White);
        }

        /// <summary>
        /// Render a specific animation frame of the sprite.
        /// </summary>
        public void Render(float x, float y, string currentAnimation, int currentFrame)
        {
            // TODO: Throw error if CurrentAnimation does not exist in Animations?
            Animation animation = Animations[currentAnimation];
            Render(x, y, animation, currentFrame, Color.White);
        }

        /// <summary>
        /// Render a specific animation frame of the sprite.
        /// </summary>
        public void Render(float x, float y, Animation animation, int currentFrame)
        {
            Render(x, y, animation, currentFrame, Color.White);
        }
        public void Render(float x, float y, Animation animation, int currentFrame, Color color)
        {
            if (color == null) color = Color.White;
            Frame frame = animation.Frames[currentFrame];
            spriteBatch.Draw(
                frame.Texture,
                gs.DisplayManager.ToPixel(x - frame.AnchorX, y - frame.AnchorY),
                new Rectangle(frame.X, frame.Y, frame.Width, frame.Height),
                color,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                0f
                );
        }

        public void RenderStretched(float x, float y, float width, float height, Color color)
        {
            Animation animation = Animations[DefaultAnimation];
            RenderStretched(x, y, width, height, animation, 0, color);
        }

        public void RenderStretched(float x, float y, float width, float height, Animation animation, int currentFrame, Color color)
        {
            if (color == null) color = Color.White;
            Frame frame = animation.Frames[currentFrame];
            spriteBatch.Draw(
                frame.Texture,
                gs.DisplayManager.ToPixel(x - frame.AnchorX, y - frame.AnchorY, width, height), // TODO: The anchor point does not take scaling into account.
                new Rectangle(frame.X, frame.Y, frame.Width, frame.Height),
                color,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                0f
                );
        }

        public void RenderTiled(float x, float y, float width, float height, Animation animation, int currentFrame, Color color)
        {
            if (color == null) color = Color.White;
            Frame frame = animation.Frames[currentFrame];
            for (float dx = 0; dx < width; dx += frame.Width)
            {
                for (float dy = 0; dy < height; dy += frame.Height)
                {
                    spriteBatch.Draw(
                        frame.Texture,
                        gs.DisplayManager.ToPixel(x - frame.AnchorX + dx, y - frame.AnchorY + dy),
                        new Rectangle(frame.X, frame.Y, frame.Width, frame.Height),
                        color,
                        0f,
                        Vector2.Zero,
                        1f,
                        SpriteEffects.None,
                        0f
                        );
                }
            }
        }
    }

    /// <summary>
    /// The collision area relative to the sprite's anchor point.
    /// </summary>
    [JsonObject]
    public class CollisionBox
    {
        [JsonProperty]
        public float Left { get; set; }
        [JsonProperty]
        public float Right { get; set; }
        [JsonProperty]
        public float Top { get; set; }
        [JsonProperty]
        public float Bottom { get; set; }
    }

    /// <summary>
    /// A single frame of an animation.
    /// </summary>
    public class Frame
    {
        // TODO: String 'Spritesheet' may not be needed if texture is always loaded already.
        public string Spritesheet { get; set; } = null;
        public int X { get; set; } = -1;
        public int Y { get; set; } = -1;
        public int Width { get; set; } = -1;
        public int Height { get; set; } = -1;
        public float AnchorX { get; set; } = -2e38f;
        public float AnchorY { get; set; } = -2e38f;
        public float Frametime { get; set; } = 1f;
        //public int Count { get; set; } = -1;

        public Texture2D Texture { get; set; }

        public Frame Clone()
        {
            return (Frame)this.MemberwiseClone();
        }
    }

    /// <summary>
    /// An animation belonging to a sprite.
    /// </summary>
    public class Animation
    {
        public string Name { get; set; }
        public float Speed { get; set; } = 1f;
        public List<Frame> Frames { get; set; }
        public Rectangle Bounds { get; private set; }

        public void CalculateBounds()
        {
            float boundX1 = 0;
            float boundX2 = 0;
            float boundY1 = 0;
            float boundY2 = 0;
            foreach (Frame frame in Frames)
            {
                float minX = -frame.AnchorX;
                float maxX = minX + frame.Width;
                float minY = -frame.AnchorY;
                float maxY = minY + frame.Height;
                boundX1 = Math.Min(boundX1, minX);
                boundX2 = Math.Max(boundX2, maxX);
                boundY1 = Math.Min(boundY1, minY);
                boundY2 = Math.Max(boundY2, maxY);
            }
            int x = (int)Math.Floor(boundX1);
            int y = (int)Math.Floor(boundY1);
            int x2 = (int)Math.Floor(boundX2);
            int y2 = (int)Math.Floor(boundY2);
            Bounds = new Rectangle(x, y, x2 - x, y2 - y);
        }
    }

    /// <summary>
    /// JSON-serializable version of Sprite data.
    /// </summary>
    [JsonObject]
    public class _Sprite
    {
        [JsonProperty]
        public CollisionBox CollisionBox { get; set; }
        [JsonProperty]
        public _FrameRange DefaultFrame { get; set; }
        [JsonProperty]
        public string DefaultAnimation { get; set; }
        [JsonProperty]
        public List<_Animation> Animations { get; set; }

        /// <summary>
        /// Prepare _Sprite format for conversion to Sprite format.
        /// </summary>
        /// <param name="path">Path that contains the sprite and spritesheet (texture) files.</param>
        /// <param name="textureLibrary">ResourceLibrary for storing textures.</param>
        public void Finalize(string path, TextureLibrary textureLibrary)
        {
            if (CollisionBox == null) CollisionBox = new CollisionBox();
            if (DefaultFrame == null) DefaultFrame = new _FrameRange();
            if (DefaultFrame.Spritesheet != null)
            {
                textureLibrary.Load(Path.Combine(path, DefaultFrame.Spritesheet), out Texture2D item);
                DefaultFrame.Texture = item;
            }
            if (DefaultFrame.X == null) DefaultFrame.X = 0;
            if (DefaultFrame.Y == null) DefaultFrame.Y = 0;
            if (DefaultFrame.Texture != null)
            {
                if (DefaultFrame.Width == null) DefaultFrame.Width = DefaultFrame.Texture.Width;
                if (DefaultFrame.Height == null) DefaultFrame.Height = DefaultFrame.Texture.Height;
            }
            else
            {
                DefaultFrame.Width = 0;
                DefaultFrame.Height = 0;
            }
            if (DefaultFrame.AnchorX == null) DefaultFrame.AnchorX = 0f;
            if (DefaultFrame.AnchorY == null) DefaultFrame.AnchorY = 0f;
            if (DefaultFrame.Count == null) DefaultFrame.Count = 1;
            if (DefaultFrame.Frametime == null) DefaultFrame.Frametime = 1f;

            if (Animations == null)
            {
                Animations = new List<_Animation>(new _Animation[] { new _Animation() });
            }
            foreach (_Animation animation in Animations)
            {
                if (animation.Name == null) animation.Name = "default";
                if (animation.Speed == null) animation.Speed = 1f;
                if (animation.FrameRanges == null)
                {
                    animation.FrameRanges = new List<_FrameRange>();
                    animation.FrameRanges.Add(DefaultFrame);
                }
                else
                {
                    foreach (_FrameRange frame in animation.FrameRanges)
                    {
                        if (frame.Spritesheet == null)
                        {
                            frame.Spritesheet = DefaultFrame.Spritesheet;
                            frame.Texture = DefaultFrame.Texture;
                        }
                        else
                        {
                            textureLibrary.Load(Path.Combine(path, frame.Spritesheet), out Texture2D item);
                            frame.Texture = item;
                        }
                        if (frame.X == null) frame.X = DefaultFrame.X;
                        if (frame.Y == null) frame.Y = DefaultFrame.Y;
                        if (frame.Width == null) frame.Width = DefaultFrame.Width;
                        if (frame.Height == null) frame.Height = DefaultFrame.Height;
                        if (frame.AnchorX == null) frame.AnchorX = DefaultFrame.AnchorX;
                        if (frame.AnchorY == null) frame.AnchorY = DefaultFrame.AnchorY;
                        if (frame.Count == null) frame.Count = DefaultFrame.Count;
                        if (frame.Frametime == null) frame.Frametime = 1f; // Do not inherit this from DefaultFrame.
                    }
                }

                if (DefaultAnimation == null) DefaultAnimation = animation.Name;
            }
        }

        /// <summary>
        /// Convert JSON-serializable _Sprite format to the more usable Sprite format.
        /// </summary>
        public Sprite Solidify(GameServices gs)
        {
            Sprite sprite = new Sprite(gs);
            Solidify(sprite);
            return sprite;
        }

        /// <summary>
        /// Convert JSON-serializable _Sprite format to the more usable Sprite format.
        /// </summary>
        /// <param name="sprite">Pre-existing Sprite object to use.</param>
        public void Solidify(Sprite sprite)
        {
            sprite.CollisionBox = CollisionBox;
            //sprite.DefaultFrame = DefaultFrame.Solidify().First();
            sprite.DefaultAnimation = DefaultAnimation;

            sprite.Animations = new Dictionary<string, Animation>();
            foreach (_Animation _animation in Animations)
            {
                sprite.Animations.Add(_animation.Name, _animation.Solidify());
            }
        }
    }

    /// <summary>
    /// JSON-serializable version of frame data.
    /// Can specify a linear range of frames instead of just one.
    /// </summary>
    [JsonObject]
    public class _FrameRange
    {
        [JsonProperty]
        public string Spritesheet { get; set; }
        [JsonProperty]
        public int? X { get; set; }
        [JsonProperty]
        public int? Y { get; set; }
        [JsonProperty]
        public int? Width { get; set; }
        [JsonProperty]
        public int? Height { get; set; }
        [JsonProperty]
        public float? AnchorX { get; set; }
        [JsonProperty]
        public float? AnchorY { get; set; }
        [JsonProperty]
        public float? Frametime { get; set; }
        [JsonProperty]
        public int? Count { get; set; }

        [JsonIgnore]
        public Texture2D Texture { get; set; }

        public List<Frame> Solidify()
        {
            Frame frame = new Frame();
            frame.Spritesheet = Spritesheet;
            frame.X = X.Value;
            frame.Y = Y.Value;
            frame.Width = Width.Value;
            frame.Height = Height.Value;
            frame.AnchorX = AnchorX.Value;
            frame.AnchorY = AnchorY.Value;
            frame.Frametime = Frametime.Value;
            frame.Texture = Texture;

            List<Frame> frames = new List<Frame>();
            frames.Add(frame);
            for (int i = 1; i < Count.Value; i++)
            {
                frame = frame.Clone();
                frame.X += frame.Width;
                frames.Add(frame);
            }

            return frames;
        }
    }

    /// <summary>
    /// JSON-serializable version of animation data.
    /// </summary>
    [JsonObject]
    public class _Animation
    {
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public float? Speed { get; set; }
        [JsonProperty]
        public List<_FrameRange> FrameRanges { get; set; }

        public Animation Solidify()
        {
            Animation animation = new Animation();
            animation.Name = Name;
            animation.Speed = Speed.Value;
            if (FrameRanges != null)
            {
                animation.Frames = new List<Frame>();
                foreach (_FrameRange _frameRange in FrameRanges)
                {
                    List<Frame> frames = _frameRange.Solidify();
                    foreach (Frame frame in frames)
                    {
                        animation.Frames.Add(frame);
                    }
                }
            }
            animation.CalculateBounds();
            return animation;
        }
    }
}
