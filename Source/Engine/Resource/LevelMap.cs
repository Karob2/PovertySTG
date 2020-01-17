using Engine.ECS;
using Engine.Input;
using Engine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using TileGame.Objects;

namespace Engine.Resource
{
    /// <summary>
    /// Holds level data, such as tiles, and can render the tiles.
    /// </summary>
    public class LevelMap
    {
        GameServices gs;
        // Path to level file on disk.
        public string FilePath { get; set; }
        // Level name.
        public string Name { get; set; }
        // Tile map with tile graphics and collision definitions.
        TileMap tileMap;
        TileMap altTileMap;
        // Needed for saving and reloading.
        public string TileMapName { get; set; }
        public string AltTileMapName { get; set; }
        public TileMapLibrary TileMapLibrary { get; set; }
        // Layout of tiles in the level.
        int[,] grid;
        int[,] entityGrid;
        Tag[,] tagGrid;
        List<Region> regions;
        // Size of level, in tiles.
        private int gridWidth, gridHeight;
        // TODO: Consider changing this from a property to a method.
        public float Width => gridWidth * tileMap.TileWidth;
        public float Height => gridHeight * tileMap.TileHeight;

        Dictionary<string, List<Tag>> tagSearch;
        List<Entity> barriers; // Mobile entities that block the player.

        // Public property access.
        public TileMap TileMap { get => tileMap; set => tileMap = value; }
        public TileMap AltTileMap { get => altTileMap; set => altTileMap = value; }
        public int[,] Grid { get => grid; set => grid = value; }
        public int GridWidth { get => gridWidth; set => gridWidth = value; }
        public int GridHeight { get => gridHeight; set => gridHeight = value; }
        public int[,] EntityGrid { get => entityGrid; set => entityGrid = value; }
        public Tag[,] TagGrid { get => tagGrid; set => tagGrid = value; }
        public List<Region> Regions { get => regions; set => regions = value; }

        public Dictionary<string, List<Tag>> TagSearch { get => tagSearch; set => tagSearch = value; }
        public List<Entity> Barriers { get => barriers; set => barriers = value; }

        public LevelMap(GameServices gs)
        {
            this.gs = gs;
            barriers = new List<Entity>();
        }

        /// <summary>
        /// Load a level map from a file.
        /// </summary>
        /// <param name="path">Path to JSON level map file.</param>
        /// <param name="tileMapLibrary">ResourceLibrary for storing tile maps.</param>
        public LevelMap(GameServices gs, string path, TileMapLibrary tileMapLibrary)
        {
            this.gs = gs;
            barriers = new List<Entity>();

            _LevelMap _levelMap = JsonHelper<_LevelMap>.Load(path);
            _levelMap.Solidify(this, Path.GetDirectoryName(path), tileMapLibrary);
            //TODO: Will _levelMap be automatically garbage collected?
            FilePath = path;
            TileMapLibrary = tileMapLibrary;
        }

        /*
        public static LevelMap NewLevelMap(string path, TileMapLibrary tileMapLibrary)
        {
            _LevelMap _levelMap = JsonHelper<_LevelMap>.Load(path);
            LevelMap levelMap = _levelMap.Solidify(Path.GetDirectoryName(path), tileMapLibrary);
            return levelMap;
        }
        */

        /// <summary>
        /// Render the tilemap at the specified coordinates, using the main SpriteBatch.
        /// </summary>
        public void Render(float x, float y)
        {
            Render(x, y, tileMap);
        }
        public void RenderAlt(float x, float y)
        {
            Render(x, y, altTileMap);
        }
        public void Render(float x, float y, TileMap tileMap)
        {
            float firstTileX = x;
            int firstTileI = (int)(-firstTileX / tileMap.TileWidth);
            float lastTileX = x - gs.DisplayManager.GameWidth;
            int lastTileI = (int)(-lastTileX / tileMap.TileWidth);

            float firstTileY = y;
            int firstTileJ = (int)(-firstTileY / tileMap.TileHeight);
            float lastTileY = y - gs.DisplayManager.GameHeight;
            int lastTileJ = (int)(-lastTileY / tileMap.TileHeight);

            if (firstTileI < 0) firstTileI = 0;
            if (firstTileI >= grid.GetLength(0)) firstTileI = grid.GetLength(0) - 1;
            if (lastTileI < 0) lastTileI = 0;
            if (lastTileI >= grid.GetLength(0)) lastTileI = grid.GetLength(0) - 1;

            if (firstTileJ < 0) firstTileJ = 0;
            if (firstTileJ >= grid.GetLength(1)) firstTileJ = grid.GetLength(1) - 1;
            if (lastTileJ < 0) lastTileJ = 0;
            if (lastTileJ >= grid.GetLength(1)) lastTileJ = grid.GetLength(1) - 1;

            for (int i = firstTileI; i <= lastTileI; i++)
            {
                for (int j = firstTileJ; j <= lastTileJ; j++)
                {
                    int id = grid[i, j];
                    Tile tile = tileMap.Tiles[id];
                    float renderX = i * tileMap.TileWidth + x;
                    //if (x < 0) renderX--; //correct rounding error
                    float renderY = j * tileMap.TileHeight + y;
                    //if (y < 0) renderY--; //correct rounding error
                    gs.DisplayManager.SpriteBatch.Draw(
                        tileMap.TileMapImage,
                        gs.DisplayManager.ToPixel(renderX, renderY),
                        new Rectangle(tile.Position[0], tile.Position[1], tileMap.TileWidth, tileMap.TileHeight),
                        Color.White,
                        0f,
                        Vector2.Zero,
                        1f,
                        SpriteEffects.None,
                        0f
                        );
                }
            }
        }

        public int GetTileByName(string name)
        {
            return TileMap.GetTileByName(name);
        }

        /// <summary>
        /// Check if a sprite has collided with a solid tile, using the sprite's collision box information.
        /// </summary>
        public bool IsCollision(Sprite sprite, float x, float y)
        {
            CollisionBox cb = sprite.CollisionBox;
            // Just check the four corner pixels.
            float cbLeft, cbRight, cbTop, cbBottom;
            cbLeft = x + cb.Left;
            cbRight = x + cb.Right;
            cbTop = y + cb.Top;
            cbBottom = y + cb.Bottom;
            // First check for collision with solid tiles.
            if (IsPointCollision(cbLeft, cbTop)) return true;
            if (IsPointCollision(cbLeft, cbBottom)) return true;
            if (IsPointCollision(cbRight, cbTop)) return true;
            if (IsPointCollision(cbRight, cbBottom)) return true;
            // If there was no solid tile collision, then identify all solid entity collisions.
            // (Need to check all of them for block pushing to happen correctly.)
            //return IsBarrierCollision(entity, cbLeft, cbRight, cbTop, cbBottom);
            return false;
        }

        /// <summary>
        /// Check if coordinates are inside of a solid tile.
        /// </summary>
        public bool IsPointCollision(float x, float y)
        {
            int tileI = (int)(x / tileMap.TileWidth);
            int tileJ = (int)(y / tileMap.TileHeight);
            // Check if the coordinates are inside of the level area.
            // NOTE: tileI >= 0 would behave poorly due to negative number rounding, so using x >= 0 instead.
            if (x >= 0 && tileI < gridWidth && y >= 0 && tileJ < gridHeight)
            {
                // TODO: Optimize this and similar lines to only do one array lookup.
                //     Example: create a collision map directly within LevelMap.
                Tile tile = tileMap.Tiles[grid[tileI, tileJ]];
                if (tile.Collision) return true;
                // Pushable blocks cannot cross snow piles.
                /*
                int id = entityGrid[tileI, tileJ];
                // Treat solid decorative entities like solid tiles.
                // (Trees and lamps.)
                if (id == 4 || id == 10 || id == 11 || id == 14 || id == 15 || id == 17 || id == 19) return true;
                */
            }
            return false;
        }

        /*
        public bool IsBarrierCollision(Entity entity, float x1, float x2, float y1, float y2)
        {
            bool isCollision = false;
            // Check for collision with moveable solid entity.
            foreach (Entity barrier in barriers)
            {
                if (barrier == entity) continue;
                if (!barrier.Solid) continue;
                if (!barrier.Visible) continue;
                float bx = barrier.X;
                float by = barrier.Y;
                CollisionBox cb = barrier.Actor.Sprite.CollisionBox;
                //barrier.Pushing = false;
                if (x2 >= bx + cb.Left && x1 <= bx + cb.Right && y2 >= by + cb.Top && y1 <= by + cb.Bottom)
                {
                    if (entity.Label == "player") barrier.Pushing = true;
                    isCollision = true;
                }
            }
            // The area outside of the level has no collision.
            return isCollision;
        }
    */

        /*
        public bool CheckSave(string key)
        {
            string foundPath;
#if DEBUG
            foundPath = Path.Combine(ResourceManager.ContentDLDirectory, "levels", key);
            if (File.Exists(foundPath)) return true;
#else
            foundPath = Path.Combine(ResourceManager.ContentDirectory, "levels", key);
            if (File.Exists(foundPath)) return true;
#endif
            return false;
        }
        */

        /// <summary>
        /// Save level map changes to original file;
        /// </summary>
        public void Save()
        {
            // TODO: FilePath, TileMapName, and TileMapLibrary may be unassigned for generated (not loaded) levels.

            // Backup old level.
            string backup = FilePath + ".bak";
            string backup2 = FilePath + ".bak2";
            if (File.Exists(backup2)) File.Delete(backup2);
            if (File.Exists(backup)) File.Move(backup, backup2);
            if (File.Exists(FilePath)) File.Move(FilePath, backup);

            // Create serializable _LevelMap object.
            _LevelMap lm = new _LevelMap();
            lm.Name = Name;
            lm.TileMap = TileMapName;
            lm.AltTileMap = AltTileMapName;
            int[,] _grid = new int[gridWidth, gridHeight];
            int newId = 0;
            List<string> legend = new List<string>();
            Dictionary<int, int> reverseLegend = new Dictionary<int, int>();
            for (int i = 0; i < gridWidth; i++)
            {
                for (int j = 0; j < gridHeight; j++)
                {
                    int id = grid[i, j];
                    if (!reverseLegend.TryGetValue(id, out int _id))
                    {
                        _id = newId;
                        newId++;
                        reverseLegend.Add(id, _id);
                        legend.Add(tileMap.Tiles[id].Name);
                    }
                    _grid[i, j] = _id;
                }
            }
            lm.Grid = _grid;
            lm.Legend = new string[legend.Count];
            for (int i = 0; i < legend.Count; i++)
            {
                lm.Legend[i] = legend[i];
            }
            lm.EntityGrid = EntityGrid;
            List<Tag> tl = new List<Tag>();
            for (int i = 0; i < gridWidth; i++)
            {
                for (int j = 0; j < gridHeight; j++)
                {
                    Tag tag = tagGrid[i, j];
                    if (tag != null)
                    {
                        //tl.Add(new Tag(tag, i, j));
                        // Correct corrdinate in case the level was resized.
                        tag.I = i;
                        tag.J = j;
                        tl.Add(tag);
                    }
                }
            }
            lm.TagList = tl;
            lm.Regions = regions;

            // Serialize _LevelMap to json file.
            JsonHelper<_LevelMap>.SaveCompact(FilePath, lm);
        }

        /// <summary>
        /// Reload level map from original file;
        /// </summary>
        public void Reload()
        {
            barriers.Clear();
            // TODO: FilePath, TileMapName, and TileMapLibrary may be unassigned for generated (not loaded) levels.
            _LevelMap _levelMap = JsonHelper<_LevelMap>.Load(FilePath);
            _levelMap.Solidify(this, Path.GetDirectoryName(FilePath), TileMapLibrary);
        }

        public void New(int gridWidth, int gridHeight)
        {
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
            grid = new int[gridWidth, gridHeight];
            entityGrid = new int[gridWidth, gridHeight];
            tagGrid = new Tag[gridWidth, gridHeight];
            regions = new List<Region>();
            tagSearch = new Dictionary<string, List<Tag>>();
        }

        /*
        public int EntityAt(float x, float y)
        {
            if (ToTileCoords((int)x, (int)y, out int i, out int j))
            {
                return entityGrid[i, j];
            }
            return 0;
        }

        public int TileAt(float x, float y)
        {
            if (ToTileCoords((int)x, (int)y, out int i, out int j))
            {
                return Grid[i, j];
            }
            return 0;
        }
        */

        public bool ToTileCoords(float x, float y, out int i, out int j)
        {
            bool inBounds = true;
            i = (int)x / tileMap.TileWidth;
            j = (int)y / tileMap.TileHeight;
            if (x < 0)
            {
                i = 0;
                inBounds = false;
            }
            if (i >= gridWidth)
            {
                i = gridWidth - 1;
                inBounds = false;
            }
            if (y < 0)
            {
                j = 0;
                inBounds = false;
            }
            if (j >= gridHeight)
            {
                j = gridHeight - 1;
                inBounds = false;
            }
            return inBounds;
        }
    }

    /// <summary>
    /// JSON-serializable version of LevelMap data.
    /// </summary>
    [JsonObject]
    public class _LevelMap
    {
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string TileMap { get; set; }
        [JsonProperty]
        public string AltTileMap { get; set; }
        [JsonProperty]
        public int[,] Grid { get; set; }
        [JsonProperty]
        public string[] Legend { get; set; }
        [JsonProperty]
        public int[,] EntityGrid { get; set; }
        //[JsonProperty]
        //public int[] EntityLegend { get; set; }
        [JsonProperty]
        public List<Tag> TagList { get; set; }
        [JsonProperty]
        public List<Region> Regions { get; set; }

        public _LevelMap()
        {
        }

        /*
        /// <summary>
        /// Convert JSON-serializable _LevelMap format to the more usable LevelMap format.
        /// </summary>
        /// <param name="path">Path that contains the level map and tile map files.</param>
        /// <param name="tileMapLibrary">ResourceLibrary for storing tile maps.</param>
        public LevelMap Solidify(string path, TileMapLibrary tileMapLibrary)
        {
            LevelMap levelMap = new LevelMap();
            Solidify(levelMap, path, tileMapLibrary);
            return levelMap;
        }
        */

        /// <summary>
        /// Convert JSON-serializable _LevelMap format to the more usable LevelMap format.
        /// </summary>
        /// <param name="levelMap">Pre-existing LevelMap object to use.</param>
        /// <param name="path">Path that contains the level map and tile map files.</param>
        /// <param name="tileMapLibrary">ResourceLibrary for storing tile maps.</param>
        public void Solidify(LevelMap levelMap, string path, TileMapLibrary tileMapLibrary)
        {
            levelMap.Name = Name;
            levelMap.TileMapName = TileMap;
            tileMapLibrary.Load(Path.Combine(path, TileMap), out TileMap tileMap);
            levelMap.TileMap = tileMap;
            levelMap.AltTileMapName = AltTileMap;
            tileMapLibrary.Load(Path.Combine(path, AltTileMap), out TileMap altTileMap);
            levelMap.AltTileMap = altTileMap;
            levelMap.GridWidth = Grid.GetLength(0);
            levelMap.GridHeight = Grid.GetLength(1);
            //levelMap.Tiles = Tiles;
            int[,] tiles = new int[levelMap.GridWidth, levelMap.GridHeight];
            for (int i = 0; i < levelMap.GridWidth; i++)
            {
                for (int j = 0; j < levelMap.GridHeight; j++)
                {
                    tiles[i, j] = tileMap.GetTileByName(Legend[Grid[i, j]]);

                    // backward compatibility
                    if (EntityGrid[i, j] == 5)
                    {
                        tiles[i, j] = tileMap.GetTileByName("hole");
                        EntityGrid[i, j] = 0;
                    }
                    if (tiles[i, j] == tileMap.GetTileByName("barrier"))
                    {
                        tiles[i, j] = 0;
                        EntityGrid[i, j] = 17;
                    }
                }
            }
            levelMap.Grid = tiles;
            levelMap.EntityGrid = EntityGrid;

            // temporary measures for backward compatibility
            if (TagList == null) TagList = new List<Tag>();
            if (Regions == null) Regions = new List<Region>();

            Tag[,] tags = new Tag[levelMap.GridWidth, levelMap.GridHeight];
            Dictionary<string, List<Tag>> ts = new Dictionary<string, List<Tag>>();
            foreach (Tag tag in TagList)
            {
                tags[tag.I, tag.J] = tag;

                // Determine the tag's name and commands.
                tag.Update();
                /*
                tag.Commands = new List<TagCommand>();
                string[] parts = tag.Text.Split(' ');
                int pi = 0;
                if (parts.Length > 0)
                {
                    if (!parts[0].Contains('('))
                    {
                        tag.Name = parts[0];
                        pi = 1;
                    }
                }
                while (pi < parts.Length)
                {
                    TagCommand tc = new TagCommand();
                    string[] cmdParts = parts[pi].Split('(');
                    tc.Name = cmdParts[0];
                    if (cmdParts.Length > 1)
                    {
                        tc.Parameters = new List<string>();
                        tc.ParameterInts = new List<int>();
                        string[] paraParts = cmdParts[1].Replace(")", "").Split(',');
                        foreach (string param in paraParts)
                        {
                            tc.Parameters.Add(param);
                            tc.ParameterInts.Add(int.TryParse(param, out int val)?val:0);
                        }
                    }
                    tag.Commands.Add(tc);
                    pi++;
                }
                */
                
                // Add the tag to the tag search dictionary.
                if (ts.TryGetValue(tag.Name, out List<Tag> tl))
                {
                    tl.Add(tag);
                }
                else
                {
                    tl = new List<Tag>();
                    tl.Add(tag);
                    ts.Add(tag.Name, tl);
                }
            }
            levelMap.TagGrid = tags;
            levelMap.TagSearch = ts;
            foreach (Region region in Regions)
            {
                region.Tag.Text = region.Text;
                region.Tag.Update();
            }
            levelMap.Regions = Regions;
        }
    }

    [JsonObject]
    public class Tag
    {
        [JsonProperty]
        public string Text { get; set; }
        [JsonProperty]
        public int I { get; set; }
        [JsonProperty]
        public int J { get; set; }

        [JsonIgnore]
        public string Name { get; set; }
        [JsonIgnore]
        public bool IsRegion { get; set; }
        [JsonIgnore]
        public bool Not { get; set; }
        [JsonIgnore]
        public List<TagCommand> Commands { get; set; }
        [JsonIgnore]
        public int TileID { get; set; }
        [JsonIgnore]
        public int EntityID { get; set; }
        [JsonIgnore]
        public int Bookmark { get; set; }
        [JsonIgnore]
        public bool ConditionCheck { get; set; }

        public Tag()
        {
            IsRegion = false;
        }

        /*
        public Tag(string text, int i, int j)
        {
            Text = text;
            I = i;
            J = j;
        }
        */

        public Tag(string text, bool isRegion = false)
        {
            Text = text;
            IsRegion = isRegion;
            Update();
        }

        public void Reset()
        {
            foreach (TagCommand cmd in Commands)
            {
                if (cmd.Repeat == 0) cmd.Repeat = 1;
            }
        }

        public void Update(string text)
        {
            Text = text;
            Update();
        }

        public void Update()
        {
            Name = "";
            Commands = new List<TagCommand>();
            string[] parts = Text.Split(' ');
            int pi = 0;
            if (parts.Length > 0 && parts[0].Length > 0)
            {
                if (parts[0][0] != '?' && !parts[0].Contains('>'))
                {
                    if (parts[0][0] == '!')
                    {
                        Name = parts[0].Remove(0, 1);
                        Not = true;
                    }
                    else
                    {
                        Name = parts[0];
                        Not = false;
                    }
                    pi = 1;
                }
            }
            for (; pi < parts.Length; pi++)
            {
                /*
                TagCommand tc = new TagCommand();
                string[] cmdParts = parts[pi].Split('>');
                tc.Name = cmdParts[0];
                tc.Parameters = new List<string>();
                tc.ParameterInts = new List<int>();
                if (cmdParts.Length > 1)
                {
                    string[] paraParts = cmdParts[1].Split(',');
                    foreach (string param in paraParts)
                    {
                        tc.Parameters.Add(param);
                        tc.ParameterInts.Add(int.TryParse(param, out int val) ? val : 0);
                    }
                }
                Commands.Add(tc);
                */
                string cmdText = parts[pi];
                if (cmdText.Length == 0) continue;
                Commands.Add(new TagCommand(cmdText));
            }
        }
    }

    public class TagCommand
    {
        public string Name { get; set; }
        public int Repeat { get; set; }
        //public List<TagParameter> Parameters { get; set; }
        // TODO: Change these to arrays.
        public List<string> Parameters { get; set; }
        public List<int> ParameterInts { get; set; }
        // TODO: Make this a bit array (bitmask)?
        public List<bool> Not { get; set; }

        public TagCommand(string text)
        {
            string[] pList;
            if (text[0] == '?')
            {
                string[] cmdParts = text.Split('?');
                Name = "?";
                pList = cmdParts[1].Split(',');
            }
            else if (text.Contains('>'))
            {
                string[] cmdParts = text.Split('>');
                Name = cmdParts[0];
                pList = null;
                if (cmdParts.Length == 2)
                {
                    pList = cmdParts[1].Split(',');
                    Repeat = 1;
                }
                else if (cmdParts.Length == 3 && cmdParts[1] == "")
                {
                    pList = cmdParts[2].Split(',');
                    Repeat = -1;
                }
            }
            else
            {
                // not a valid command
                // TODO: throw an error?
                pList = new string[0];
                //Error.LogError("Invalid tag command: " + text); // TODO: Find a way to allow error reporting here. (Maybe I should have left it static.)
            }
            Parameters = new List<string>();
            ParameterInts = new List<int>();
            Not = new List<bool>();
            foreach (string param in pList)
            {
                string paramName;
                if (param.Length > 0 && param[0] == '!')
                {
                    Not.Add(true);
                    paramName = param.Remove(0, 1);
                }
                else
                {
                    Not.Add(false);
                    paramName = param;
                }
                Parameters.Add(paramName);
                ParameterInts.Add(int.TryParse(paramName, out int val) ? val : 0);
            }
        }
    }

    /*
    public class TagParameter
    {
        public string 
    }
    */

    [JsonObject]
    public class Region
    {
        [JsonProperty]
        public string Text { get; private set; }
        [JsonProperty]
        public int X { get; set; }
        [JsonProperty]
        public int Y { get; set; }
        [JsonProperty]
        public int Width { get; set; }
        [JsonProperty]
        public int Height { get; set; }

        [JsonIgnore]
        public Tag Tag { get; set; }

        public Region()
        {
            Tag = new Tag();
        }

        public Region(string text, int x, int y, int width, int height)
        {
            Text = text;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Tag = new Tag(text, true);
        }

        public void Update(string text)
        {
            Text = text;
            Tag.Update(text);
        }
    }
}
