using Engine.Util;
using System.IO;

namespace Engine.Resource
{
    /// <summary>
    /// Collection of loaded level maps identified by string names.
    /// </summary>
    public class LevelMapLibrary : ResourceLibrary<LevelMap>
    {
        readonly string[] extensions = new string[] { ".level-json" };
        public override string[] Extensions => extensions;
        TileMapLibrary tileMapLibrary;

        public LevelMapLibrary(GameServices gs) : base(gs)
        {
            // For storing referenced tile maps.
            this.tileMapLibrary = new TileMapLibrary(gs);
        }

        protected override LevelMap InternalLoad(string path)
        {
            // Load level map definition from JSON file.
            if (!File.Exists(path))
            {
                gs.Error.LogErrorAndShutdown("Failed to find asset <" + path + ">.");
            }
            LevelMap levelMap = new LevelMap(gs, path, tileMapLibrary);
            return levelMap;
        }

        protected override void InternalUnload(LevelMap levelMap) { }

        public override void Unload()
        {
            tileMapLibrary.Unload();
            base.Unload();
        }

        /// <summary>
        /// Create an empty level map and return true if successful.
        /// </summary>
        /// <param name="levelName">Map name.</param>
        /// <param name="tileMapName">Name of tile map to use.</param>
        /// <param name="levelMap">The new level map.</param>
        public bool EmptyMap(string levelName, string tileMapName, out LevelMap levelMap)
        {
            // If a level map with the name already exists, re-use that level map.
            // TODO: Should it instead overwrite the existing map? Throw an error?
            if (!Get(levelName, out levelMap))
            {
                // Otherwise, create a new level map.
                levelMap = new LevelMap(gs);
                list.Add(levelName, levelMap);
            }

            // Find and load the specified tile map.
            if (gs.ResourceManager.LocateResource("levels", tileMapName, out string foundPath))
            {
                tileMapLibrary.Load(foundPath, out TileMap tileMap);
                levelMap.TileMap = tileMap;
            }
            return true;
        }
    }
}
