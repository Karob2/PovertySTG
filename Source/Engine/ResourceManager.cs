using Engine.Resource;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// TODO: Consider moving this functionality into GameServices.

namespace Engine
{
    public class ResourceManager
    {
        public Game Game { get; set; }
        GameServices gs;

        // Game properties.
        public string GameName { get; set; }
        public string CompanyName { get; set; }
        public string ContentDirectory { get; set; }
#if DEBUG
        // In debug builds, direct-loaded content hasn't been copied to the shared folder
        // and needs to be loaded separately.
        public string ContentDLDirectory { get; set; }
#endif
        public string SaveDirectory { get; private set; }
        public string TempDirectory { get; private set; }

        //public List<LibraryType> LibraryTypes { get; set; }
        public FontLibrary Fonts { get; private set; }
        public LevelMapLibrary LevelMaps { get; private set; }
        public SongLibrary Songs { get; private set; }
        public SoundEffectLibrary SoundEffects { get; private set; }
        public TextureLibrary Textures { get; private set; }
        public SpriteLibrary Sprites { get; private set; }
        public StoryLibrary Stories { get; private set; }

        // For loading content in separate steps, to allow an updating loading screen.
        static int contentLoadLevel;

        public ResourceManager(GameServices gs, string gameName, string companyName)
        {
            this.gs = gs;
            Game = gs.Game;
            GameName = gameName;
            CompanyName = companyName;

            // Determine folder that holds resource files.
            ContentDirectory = "Content";
#if DEBUG
            // In debug builds, direct-loaded content hasn't been copied to the shared folder
            // and needs to be loaded separately.
            ContentDLDirectory = Path.Combine("..", "..", "..", "ContentDL");
#endif

            // Determine folder that holds save and configuration files.
            if (companyName == null || companyName == "")
            {
                SaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), gameName);
                TempDirectory = Path.Combine(Path.GetTempPath(), gameName);
            }
            else
            {
                SaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), companyName, gameName);
                TempDirectory = Path.Combine(Path.GetTempPath(), companyName, gameName);
            }
            Directory.CreateDirectory(SaveDirectory);
            Directory.CreateDirectory(TempDirectory);

            //LibraryTypes.Add(new LibraryType(typeof(TextureLibrary), "textures"));
            //LibraryTypes.Add(new LibraryType(typeof(SpriteLibrary), "sprites"));
        }

        public void Initialize()
        {
            Fonts = new FontLibrary(gs);
            LevelMaps = new LevelMapLibrary(gs);
            Songs = new SongLibrary(gs);
            SoundEffects = new SoundEffectLibrary(gs);
            Textures = new TextureLibrary(gs);
            Sprites = new SpriteLibrary(gs);
            Stories = new StoryLibrary(gs);
        }

        /// <summary>
        /// Load all resources at once.
        /// </summary>
        public void LoadContent()
        {
            LoadResources("fonts", Fonts);
            LoadResources("levels", LevelMaps);
            LoadResources("bgm", Songs);
            LoadResources("sfx", SoundEffects);
            LoadResources("stories", Stories);
            LoadResources("sprites", Sprites.TextureLibrary);
            LoadResources("sprites", Sprites);
            LoadResources("textures", Textures);

            // Automatically generate sprites for all the textures.
            foreach (string key in Sprites.TextureLibrary.List.Keys)
            {
                //Sprites.List.Add("texture_" + key, new Sprite(gs, key, Textures.List[key]));
                if (!Sprites.List.ContainsKey(key))
                {
                    Sprites.List.Add(key, new Sprite(gs, key, Sprites.TextureLibrary.List[key]));
                }
            }
        }

        /// <summary>
        /// Load resources one step at a time, and return a message about the next resource to load. Returns true when all content loading steps are complete.
        /// </summary>
        public bool LoadNextContent(out string message, out float progress)
        {
            contentLoadLevel++;
            progress = contentLoadLevel / 8f;
            switch (contentLoadLevel)
            {
                case 1:
                    message = "fonts";
                    return false;
                case 2:
                    LoadResources("fonts", Fonts);
                    message = "textures";
                    return false;
                case 3:
                    LoadResources("textures", Textures);
                    LoadResources("sprites", Sprites.TextureLibrary);
                    message = "sprites";
                    return false;
                case 4:
                    LoadResources("sprites", Sprites);
                    // Automatically generate sprites for all the textures.
                    foreach (string key in Sprites.TextureLibrary.List.Keys)
                    {
                        //Sprites.List.Add("texture_" + key, new Sprite(gs, key, Textures.List[key]));
                        if (!Sprites.List.ContainsKey(key))
                        {
                            Sprites.List.Add(key, new Sprite(gs, key, Sprites.TextureLibrary.List[key]));
                        }
                    }
                    message = "levels";
                    return false;
                case 5:
                    LoadResources("levels", LevelMaps);
                    message = "music";
                    return false;
                case 6:
                    LoadResources("bgm", Songs);
                    message = "sound effects";
                    return false;
                case 7:
                    LoadResources("sfx", SoundEffects);
                    message = "stories";
                    return false;
                case 8:
                    LoadResources("stories", Stories);
                    message = "complete";
                    return true;
                default:
                    message = "failed";
                    return true;
            }
        }

        /// <summary>
        /// Looks for a resource in various locations and returns true if found.
        /// </summary>
        /// <param name="path">The folder the resource is expected to be in.</param>
        /// <param name="file">The filename of the resource.</param>
        /// <param name="foundPath">The full path to the found file.</param>
        public bool LocateResource(string path, string file, out string foundPath)
        {
            //string collection = Path.GetFileName(Path.GetDirectoryName(path));
            string collection = Path.GetFileName(path);
            //string file = Path.GetFileName(path);
            string ext = Path.GetExtension(file);
            if (ext == null)
            {
                ext = ".xnb";
                file += ext;
            }

            foundPath = Path.Combine(ContentDirectory, collection, file);
            if (File.Exists(foundPath)) return true;
#if DEBUG
            foundPath = Path.Combine(ContentDLDirectory, collection, file);
            if (File.Exists(foundPath)) return true;
#endif
            return false;
        }

        /// <summary>
        /// Load all resources in a content subfolder.
        /// </summary>
        /// <param name="collection">The string name for the type of resource, also used as the subfolder name.</param>
        /// <param name="library">The ResourceLibrary to store the loaded resource in.</param>
        /// <param name="contentDirectory">Override default content directory location.</param>
        public void LoadResources<T>(string collection, T library, string contentDirectory = null) where T : ResourceLibrary
        {
            string path;
            if (contentDirectory == null) path = Path.Combine(ContentDirectory, collection);
            else path = Path.Combine(contentDirectory, collection);
            gs.Error.Log(Path.GetFullPath(path));
            if (Directory.Exists(path))
            {
                foreach (string file in Directory.GetFiles(path))
                {
                    string ext = Path.GetExtension(file);

                    bool valid = false;
                    foreach (string validExt in library.Extensions)
                    {
                        if (ext.Equals(validExt)) valid = true;
                    }
                    if (!valid) continue;

                    gs.Error.Log("Loading " + file);
                    library.Load(file);
                }
            }
#if DEBUG
            if (contentDirectory == null) LoadResources<T>(collection, library, ContentDLDirectory);
#endif
        }
        
        /// <summary>
        /// Returns the full path to a file within the save file / config directory.
        /// </summary>
        public string GetSaveDirectory(string filename)
        {
            return Path.Combine(SaveDirectory, filename);
        }

        /// <summary>
        /// Returns the full path to a file within the save file / config directory.
        /// </summary>
        public string GetTempDirectory(string filename)
        {
            //return Path.Combine(SaveDirectory, ".temp", filename);
            return Path.Combine(TempDirectory, filename);
        }

        /// <summary>
        /// Delete all temporary files from the temp directory.
        /// </summary>
        public void ClearTempDirectory()
        {
            //Songs.Unload();
            if (Directory.Exists(TempDirectory))
            {
                foreach (string file in Directory.GetFiles(TempDirectory))
                {
                    if (Path.GetExtension(file) == ".tmp")
                    {
                        File.Delete(file);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new content manager based off of the default global one.
        /// </summary>
        public ContentManager NewContentManager()
        {
            return new ContentManager(Game.Content.ServiceProvider, Game.Content.RootDirectory);
        }
    }

    /*
    public class LibraryType
    {
        //public ResourceLibrary Library { get; set; }
        public Type Type { get; set; }
        public string Subdirectory { get; set; }

        public LibraryType(Type type, string subdirectory)
        {
            Type = type;
            Subdirectory = subdirectory;
        }
    }
    */
}
