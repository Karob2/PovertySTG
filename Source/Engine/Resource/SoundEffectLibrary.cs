using Engine.Util;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace Engine.Resource
{
    /// <summary>
    /// Collection of loaded sound effects identified by string names.
    /// </summary>
    public class SoundEffectLibrary : ResourceLibrary<SoundEffect>
    {
        readonly string[] extensions = new string[] { ".xnb", ".ogg", ".ogz", ".wav" };
        public override string[] Extensions => extensions;
        ContentManager contentManager;

        public SoundEffectLibrary(GameServices gs) : base(gs)
        {
            // Content manager instance for storing sound effects.
            contentManager = gs.ResourceManager.NewContentManager();
        }

        protected override SoundEffect InternalLoad(string path)
        {
            SoundEffect fx;

            string ext = Path.GetExtension(path);
            if (ext == null)
            {
                ext = ".xnb";
                path += ext;
            }

            if (ext.Equals(".xnb"))
            {
                contentManager.RootDirectory = Path.GetDirectoryName(Path.GetFullPath(path));
                fx = contentManager.Load<SoundEffect>(Path.GetFileNameWithoutExtension(path));
            }
            else if (ext.Equals(".ogg"))
            {
                fx = AudioHelper.SoundEffectFromOgg(path);
            }
            else if (ext.Equals(".ogz"))
            {
                fx = AudioHelper.SoundEffectFromOgz(path);
            }
            else if (ext.Equals(".wav"))
            {
                fx = AudioHelper.SoundEffectFromWav(path);
            }
            else
            {
                fx = null;
                gs.Error.LogErrorAndShutdown("Sound effect file type not supported yet.");
            }
            return fx;
        }

        protected override void InternalUnload(SoundEffect fx)
        {
            fx.Dispose();
        }

        public override void Unload()
        {
            contentManager.Unload();
            base.Unload();
        }
    }
}
