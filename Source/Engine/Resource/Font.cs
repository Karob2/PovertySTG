using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Engine.Resource
{
    /// <summary>
    /// JSON-serializable font class. Can configure and render fonts.
    /// </summary>
    [JsonObject]
    public class Font
    {
        GameServices gs;

        [JsonProperty]
        public string FontFile { get; set; }
        [JsonProperty]
        public float Scale { get; set; }

        SpriteFont spriteFont;
        [JsonIgnore]
        public SpriteFont SpriteFont { get { return spriteFont; } set { spriteFont = value; } }

        public Font() { }

        public void SetGameServices(GameServices gs)
        {
            this.gs = gs;
        }

        public void SetFont(SpriteFont spriteFont)
        {
            this.spriteFont = spriteFont;
            this.spriteFont.DefaultCharacter = '?';
        }

        /// <summary>
        /// Renders text using this font and the main SpriteBatch.
        /// </summary>
        public void Render(string message, float x, float y, Color color, float scale = 1f, float depth = 1f)
        {
            gs.DisplayManager.SpriteBatch.DrawString(spriteFont, message, gs.DisplayManager.ToPixel(x, y),
                color, 0f, new Vector2(0f, 0f), Scale * scale, SpriteEffects.None, depth);
        }

        /// <summary>
        /// Convenience function to render centered text using this font and the main SpriteBatch.
        /// </summary>
        public void RenderCentered(string message, float x, float y, Color color, float scale = 1f, float depth = 1f)
        {
            Vector2 size = spriteFont.MeasureString(message) * Scale * scale;
            gs.DisplayManager.SpriteBatch.DrawString(spriteFont, message, gs.DisplayManager.ToPixel(x - size.X / 2, y - size.Y / 2),
                color, 0f, new Vector2(0f, 0f), Scale * scale, SpriteEffects.None, depth);
        }

        /// <summary>
        /// Returns the size of a string when rendered in this font.
        /// </summary>
        public Vector2 MeasureString(string message)
        {
            Vector2 result = spriteFont.MeasureString(message) * Scale;
            return result;
        }
    }
}
