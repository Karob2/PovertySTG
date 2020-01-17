using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Engine.Resource;
using Engine.Util;
using Engine.ECS;

namespace Engine
{
    // TODO: Review my accessibility level choices. (Or just continue going public crazy.)
    public class GameServices
    {
        public Game Game { get; private set; }
        public Error Error { get; private set; }
        //Scene root;
        public DisplayManager DisplayManager { get; private set; }
        public ResourceManager ResourceManager { get; private set; }
        public Scene RootScene { get; private set; }

        public GameServices(Game game, string gameName, string companyName)
        {
            Game = game;
            Error = new Error(this);
            DisplayManager = new DisplayManager(this);
            ResourceManager = new ResourceManager(this, gameName, companyName);
            RootScene = new Scene(this);
            RootScene.Enable();
        }

        public void Initialize(int gameWidth, int gameHeight, int zoomMode, bool fullscreen)
        {
            ResourceManager.Initialize();
            Error.StartLog();
            /*
            Config.Initialize(ResourceManager.GetSaveDirectory("config.json"));
            DisplayManager.Initialize(gameWidth, gameHeight, Config.Fullscreen);
            DisplayManager.SetZoom(Config.Zoom);
            InputManager.Initialize(ResourceManager.GetSaveDirectory("inputconfig.json"));
            TextHandler.Start();
            SoundManager.Initialize();
            */
            DisplayManager.Initialize(gameWidth, gameHeight, fullscreen);
            DisplayManager.SetZoom(zoomMode);
        }

        public void Terminate()
        {
            Error.EndLog();
            ResourceManager.ClearTempDirectory();
        }

        public void Update(GameTime gameTime)
        {
            RootScene.Update(gameTime);
        }

        public void Render(GameTime gameTime)
        {
            RootScene.Render(gameTime);
        }
    }
}
