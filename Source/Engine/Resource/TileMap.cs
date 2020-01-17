using Engine.Util;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Engine.Resource
{
    /// <summary>
    /// Holds tile map data, such as tile locations in the tilesheet and collision information.
    /// </summary>
    public class TileMap
    {
        // The tilesheet texture.
        Texture2D tileMapImage;
        public Texture2D TileMapImage { get { return tileMapImage; } set { tileMapImage = value; } }
        // List of tile definitions.
        Tile[] tiles;
        public Tile[] Tiles { get { return tiles; } set { tiles = value; } }
        // Size of each tile.
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        // Dictionary to look up tile definitions by their names.
        Dictionary<string, int> tilesByName;

        public TileMap() { }

        /// <summary>
        /// Load a tile map from a file.
        /// </summary>
        /// <param name="path">Path to JSON level map file.</param>
        /// <param name="textureLibrary">ResourceLibrary for storing textures.</param>
        public TileMap(string path, TextureLibrary textureLibrary)
        {
            _TileMap _tileMap = JsonHelper<_TileMap>.Load(path);
            _tileMap.Solidify(this, Path.GetDirectoryName(path), textureLibrary);
            //TODO: Will _tileMap be automatically garbage collected?
        }

        /*
        public static TileMap NewTileMap(string path, TextureLibrary textureLibrary)
        {
            _TileMap _tileMap = JsonHelper<_TileMap>.Load(path);
            //_levelMap.Finalize(textureLibrary, Path.GetDirectoryName(path));
            TileMap tileMap = _tileMap.Solidify(Path.GetDirectoryName(path), textureLibrary);
            return tileMap;
        }
        */

        /// <summary>
        /// Make a Dictionary to look up tile definitions by their names.
        /// </summary>
        public void MakeLookup()
        {
            tilesByName = new Dictionary<string, int>();
            for (int i = 0; i < Tiles.Length; i++)
            {
                tilesByName.Add(Tiles[i].Name, i);
            }
        }

        /// <summary>
        /// Look up tile definition by name.
        /// </summary>
        public int GetTileByName(string name)
        {
            return tilesByName[name];
        }

        /*
        public void Render(float x, float y)
        {
            ScreenManager.SpriteBatch.Draw(
                tileMapImage,
                new Vector2(x, y),
                new Rectangle(0, 0, TileWidth, TileHeight),
                Color.White,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                0f
                );
        }
        */
    }

    /// <summary>
    /// JSON-serializable version of TileMap data.
    /// </summary>
    [JsonObject]
    public class _TileMap
    {
        [JsonProperty]
        public string TileMapImage { get; set; }
        [JsonProperty]
        public int[] TileSize { get; set; }
        [JsonProperty]
        public List<Tile> Tiles { get; set; }

        public _TileMap()
        {
        }

        /// <summary>
        /// Convert JSON-serializable _TileMap format to the more usable TileMap format.
        /// </summary>
        /// <param name="tileMap">Pre-existing TileMap object to use.</param>
        /// <param name="path">Path that contains the tile map and tilesheet (texture) files.</param>
        public TileMap Solidify(string path, TextureLibrary textureLibrary)
        {
            TileMap tileMap = new TileMap();
            Solidify(tileMap, path, textureLibrary);
            return tileMap;
        }

        /// <summary>
        /// Convert JSON-serializable _TileMap format to the more usable TileMap format.
        /// </summary>
        /// <param name="tileMap">Pre-existing TileMap object to use.</param>
        /// <param name="path">Path that contains the tile map and tilesheet (texture) files.</param>
        /// <param name="textureLibrary">ResourceLibrary for storing textures.</param>
        public void Solidify(TileMap tileMap, string path, TextureLibrary textureLibrary)
        {
            textureLibrary.Load(Path.Combine(path, TileMapImage), out Texture2D item);
            tileMap.TileMapImage = item;
            tileMap.TileWidth = TileSize[0];
            tileMap.TileHeight = TileSize[1];
            Tile[] tiles = new Tile[Tiles.Count];
            for (int i = 0; i < Tiles.Count; i++)
            {
                tiles[i] = Tiles[i];
                tiles[i].Position = new int[2];
                tiles[i].Position[0] = Tiles[i].Index[0] * TileSize[0];
                tiles[i].Position[1] = Tiles[i].Index[1] * TileSize[1];
            }
            tileMap.Tiles = tiles;
            tileMap.MakeLookup();
        }
    }

    /// <summary>
    /// JSON-serializable Tile data.
    /// </summary>
    [JsonObject]
    public class Tile
    {
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Type { get; set; }
        [JsonProperty]
        public int[] Index { get; set; }
        [JsonIgnore]
        public int[] Position { get; set; }
        [JsonProperty]
        public bool Collision { get; set; }
    }
}
