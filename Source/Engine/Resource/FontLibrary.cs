using Engine.Util;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

// TODO: Currently each Font object retains its own copy of the specified SpriteFont file.
//   This is wasteful if there are multiple font objects using the same spritefont.
//   Ideally, multiple font objects will not use the same spritefont.
// Actually, if the content manager caches like it should, this may not be an issue. I need to check this.

namespace Engine.Resource
{
    /// <summary>
    /// Collection of loaded fonts identified by string names.
    /// </summary>
    public class FontLibrary : ResourceLibrary<Font>
    {
        readonly string[] extensions = new string[] { ".json" };
        public override string[] Extensions => extensions;
        ContentManager contentManager;

        public FontLibrary(GameServices gs) : base(gs)
        {
            // For storing referenced SpriteFont files.
            contentManager = gs.ResourceManager.NewContentManager();
        }

        protected override Font InternalLoad(string path)
        {
            // Load font definition from JSON file.
            if (!File.Exists(path))
            {
                gs.Error.LogErrorAndShutdown("Failed to find font JSON <" + path + ">.");
            }
            Font font = JsonHelper<Font>.Load(path);
            font.SetGameServices(gs);

            // Load the SpriteFont specified in the JSON file.
            if (gs.ResourceManager.LocateResource(Path.GetDirectoryName(path), font.FontFile, out string foundPath))
            {
                contentManager.RootDirectory = Path.GetDirectoryName(Path.GetFullPath(foundPath));
                font.SetFont(contentManager.Load<SpriteFont>(Path.GetFileNameWithoutExtension(font.FontFile)));
            }
            else
            {
                gs.Error.LogErrorAndShutdown("Failed to find spritefont <" + font.FontFile + ">.");
            }

            return font;
        }

        protected override void InternalUnload(Font font)
        {
            font.SpriteFont.Texture.Dispose();
        }

        public override void Unload()
        {
            contentManager.Unload();
            base.Unload();
        }
    }
}
