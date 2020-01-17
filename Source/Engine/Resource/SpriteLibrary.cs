using Engine.Util;
using System.IO;

namespace Engine.Resource
{
    /// <summary>
    /// Collection of loaded sprites identified by string names.
    /// </summary>
    public class SpriteLibrary : ResourceLibrary<Sprite>
    {
        readonly string[] extensions = new string[] { ".json" };
        public override string[] Extensions => extensions;
        TextureLibrary textureLibrary;
        public TextureLibrary TextureLibrary => textureLibrary;


        public SpriteLibrary(GameServices gs) : base(gs)
        {
            // For storing referenced image files.
            this.textureLibrary = new TextureLibrary(gs);
        }

        protected override Sprite InternalLoad(string path)
        {
            if (!File.Exists(path))
            {
                gs.Error.LogErrorAndShutdown("Failed to find sprite <" + path + ">.");
            }
            Sprite sprite = new Sprite(gs, path, textureLibrary);
            return sprite;
        }

        protected override void InternalUnload(Sprite sprite) { }

        public override void Unload()
        {
            textureLibrary.Unload();
            base.Unload();
        }
    }
}
