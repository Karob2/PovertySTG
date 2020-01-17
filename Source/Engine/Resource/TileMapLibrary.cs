using Engine.Util;
using System.IO;

namespace Engine.Resource
{
    /// <summary>
    /// Collection of loaded tile maps identified by string names.
    /// </summary>
    public class TileMapLibrary : ResourceLibrary<TileMap>
    {
        readonly string[] extensions = new string[] { ".json" };
        public override string[] Extensions => extensions;
        TextureLibrary textureLibrary;

        public TileMapLibrary(GameServices gs) : base(gs)
        {
            // For storing referenced image files.
            this.textureLibrary = new TextureLibrary(gs);
        }

        protected override TileMap InternalLoad(string path)
        {
            if (!File.Exists(path))
            {
                gs.Error.LogErrorAndShutdown("Failed to find tile map <" + path + ">.");
            }
            TileMap tileMap = new TileMap(path, textureLibrary);
            return tileMap;
        }

        protected override void InternalUnload(TileMap tileMap) { }

        public override void Unload()
        {
            textureLibrary.Unload();
            base.Unload();
        }
    }
}
