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
    public class LevelScriptLibrary : ResourceLibrary<LevelScript>
    {
        readonly string[] extensions = new string[] { ".json" };
        public override string[] Extensions => extensions;
        ContentManager contentManager;

        public LevelScriptLibrary(GameServices gs) : base(gs)
        {
        }

        protected override LevelScript InternalLoad(string path)
        {
            // Load font definition from JSON file.
            if (!File.Exists(path))
            {
                gs.Error.LogErrorAndShutdown("Failed to find level script JSON <" + path + ">.");
            }
            LevelScript levelScript = JsonHelper<LevelScript>.Load(path);
            return levelScript;
        }

        protected override void InternalUnload(LevelScript levelScript) { }

        public override void Unload()
        {
            base.Unload();
        }
    }
}
