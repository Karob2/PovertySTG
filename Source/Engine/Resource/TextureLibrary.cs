using Engine.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Engine.Resource
{
    /// <summary>
    /// Collection of loaded sprites identified by string names.
    /// </summary>
    public class TextureLibrary : ResourceLibrary<Texture2D>
    {
        readonly string[] extensions = new string[] { ".xnb", ".png", ".jpg" };
        public override string[] Extensions => extensions;
        ContentManager contentManager;

        public TextureLibrary(GameServices gs) : base(gs)
        {
            // Content manager instance for storing textures.
            contentManager = gs.ResourceManager.NewContentManager();
        }

        protected override Texture2D InternalLoad(string path)
        {
            Texture2D texture = null;
            //string path2 = Path.Combine(ResourceManager.ContentDirectory, "textures", path)

            string ext = Path.GetExtension(path);
            if (ext == null)
            {
                ext = ".xnb";
                path += ext;
            }

            if (!File.Exists(path))
            {
                gs.Error.LogErrorAndShutdown("Failed to find texture <" + path + ">.");
            }

            if (ext.Equals(".xnb"))
            {
                contentManager.RootDirectory = Path.GetDirectoryName(Path.GetFullPath(path));
                texture = contentManager.Load<Texture2D>(Path.GetFileNameWithoutExtension(path));
            }
            else if (ext.Equals(".png") || ext.Equals(".jpg"))
            {
                FileStream fileStream = new FileStream(path, FileMode.Open);
                texture = Texture2D.FromStream(gs.Game.GraphicsDevice, fileStream);

                // Premultiply the alpha.
                Color[] buffer = new Color[texture.Width * texture.Height];
                texture.GetData(buffer);
                for (int i = 0; i < buffer.Length; i++)
                    buffer[i] = Color.FromNonPremultiplied(buffer[i].R, buffer[i].G, buffer[i].B, buffer[i].A);
                texture.SetData(buffer);

                fileStream.Dispose();
            }
            else
            {
                gs.Error.LogErrorAndShutdown("Invalid or misssing texture extension <" + path + ">.");
            }

            return texture;
        }

        protected override void InternalUnload(Texture2D texture)
        {
            texture.Dispose();
        }

        public override void Unload()
        {
            contentManager.Unload();
            base.Unload();
        }
    }
}
