using Engine.Util;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.IO;

namespace Engine.Resource
{
    /// <summary>
    /// Collection of loaded songs identified by string names.
    /// </summary>
    public class SongLibrary : ResourceLibrary<Song>
    {
        readonly string[] extensions = new string[] { ".xnb", ".ogg", ".ogz", ".ogg-z", ".ogz-g" };
        public override string[] Extensions => extensions;
        ContentManager contentManager;

        public SongLibrary(GameServices gs) : base(gs)
        {
            // Content manager instance for storing songs.
            contentManager = gs.ResourceManager.NewContentManager();
        }

        protected override Song InternalLoad(string path)
        {
            Song song;

            string ext = Path.GetExtension(path);
            if (ext == null)
            {
                ext = ".xnb";
                path += ext;
            }

            if (ext.Equals(".xnb"))
            {
                contentManager.RootDirectory = Path.GetDirectoryName(Path.GetFullPath(path));
                song = contentManager.Load<Song>(Path.GetFileNameWithoutExtension(path));
            }
            else if (ext.Equals(".ogg"))
            {
                song = Song.FromUri(path, new Uri(path, UriKind.Relative));
            }
            else if (ext.Equals(".ogg-z"))
            {
                song = Song.FromUri(path, new Uri(path, UriKind.Relative));
                string path2 = path.Replace(".ogg-z", ".ogz");
                Muddle.NewKeys();
                Muddle.Encrypt(path, path2);
            }
            else if (ext.Equals(".ogz-g"))
            {
                string path2 = path.Replace(".ogz-g", ".ogg");
                Muddle.Decrypt(path, path2);
                song = Song.FromUri(path2, new Uri(path2, UriKind.Relative));
            }
            else if (ext.Equals(".ogz"))
            {
                string path2 = gs.ResourceManager.GetTempDirectory(Path.GetFileNameWithoutExtension(path) + ".tmp");
                Muddle.Decrypt(path, path2);
                song = Song.FromUri(path2, new Uri(path2, UriKind.Relative));
            }
            else
            {
                song = null;
                gs.Error.LogErrorAndShutdown("Song file type not supported yet.");
            }
            return song;
        }

        protected override void InternalUnload(Song song)
        {
            song.Dispose();
        }

        public override void Unload()
        {
            contentManager.Unload();
            base.Unload();
        }
    }
}
