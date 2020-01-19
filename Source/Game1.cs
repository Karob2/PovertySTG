using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Engine;
using Engine.Resource;
using PovertySTG.ECS.Systems;
using PovertySTG.ECS.Components;
using PovertySTG.Factories;
using Engine.Input;

namespace PovertySTG
{
    public class Game1 : Game
    {
        // Save, config, and log files will go in %localappdata%/game/ or %localappdata%/company/game/
        const string gameName = "Dance of the Poverty Gods";
        const string companyName = "Lantern House";
        // Game resolution.
        //const int gameWidth = 1440;
        //const int gameHeight = 1080;
        //int screenMode = 0; //0 = windowed, 1 = fullscreen
        bool screenModeJustChanged;
        //int screenZoomMode = 1; //0 = 1080p (1x), 1 = 720p (0.67x), 2 = 600p (0.56x)
        bool screenZoomModeJustChanged;

        GameServices gs;
        bool contentLoaded;
        float contentProgress;
        Sprite pixel;

        //GraphicsDeviceManager graphics;
        //SpriteBatch spriteBatch;

        public Game1()
        {
            //graphics = new GraphicsDeviceManager(this);
            //Content.RootDirectory = "Content";
            //IsMouseVisible = true;

            Window.Title = gameName;
            gs = new GameServices(this, gameName, companyName);
        }

        protected override void Initialize()
        {
            Config.Default();
            //gs.Initialize(gameWidth, gameHeight, screenZoomMode, screenMode == 1);
            gs.Initialize(Config.defaultWidth, Config.defaultHeight, Config.GameScales[Config.Zoom], Config.Fullscreen);
            InputManager.Initialize(gs, gs.ResourceManager.GetSaveDirectory("inputconfig.json"));
            //TextHandler.Start(gs);
            SoundManager.Initialize(gs);
            //SoundManager.MusicVolume = 0.75f;
            //SoundManager.SoundVolume = 0.75f;
            Config.Initialize(gs, gs.ResourceManager.GetSaveDirectory("config.json"));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            //gs.ResourceManager.LoadContent();
            pixel = PreloadTexture("pixel");
        }

        void LoadContent2()
        {
            contentLoaded = gs.ResourceManager.LoadNextContent(out string message, out contentProgress);
            pixel.RenderStretched(0, 0, contentProgress * gs.DisplayManager.GameWidth, 32, Color.White);
            if (contentLoaded) LoadContent3();
        }

        void LoadContent3()
        {
            gs.RootScene.AddUpdateSystem(new GameStateSystem());
            gs.RootScene.NewEntity().AddComponent(new GameStateComponent()).Enable();
            gs.RootScene.AddScene(SceneFactory.NewMenuScene(gs, "menu")).Enable();
            SoundManager.PlayMusic("Dance_Plague_of_2020");
            //SoundManager.PlayMusic("");
            InputManager.Reset();
        }

        protected override void UnloadContent()
        {
            Config.SaveConfig();
            gs.Terminate();
        }

        protected override void Update(GameTime gameTime)
        {
            if (!contentLoaded) return;

            // Update keyboard, gamepad, and mouse inputs.
            if (IsActive) InputManager.Update(gameTime); // TODO: Do I want the mouse pointer to still move when the game is inactive?
            if (IsActive) CheckGlobalInput();
            gs.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(new Color(5, 5, 20));
            gs.DisplayManager.StartDrawing(new Color(5, 5, 20));

            /*
            Sprite sprite;
            sprite = gs.ResourceManager.Sprites.Get("cirno");
            sprite.Render(0, 0);
            */
            if (!contentLoaded) LoadContent2();
            else gs.Render(gameTime);

            gs.DisplayManager.StopDrawing();
            base.Draw(gameTime);
        }

        void CheckGlobalInput()
        {
            bool shift = Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift);
            bool alt = Keyboard.GetState().IsKeyDown(Keys.LeftAlt) || Keyboard.GetState().IsKeyDown(Keys.RightAlt);

            // F11 or Alt+Enter will switch between windowed and fullscreen.
            if (Keyboard.GetState().IsKeyDown(Keys.F11) || alt && Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                if (screenModeJustChanged) return;
                InputManager.StickMouse();
                Config.Fullscreen = !Config.Fullscreen;
                gs.DisplayManager.SetFullscreen(Config.Fullscreen);
                screenModeJustChanged = true; // Wait until key release before screen mode can be changed again.
                Config.SaveConfig();
            }
            else
            {
                screenModeJustChanged = false;
            }

            // F10 will change window size/zoom.
            if (Keyboard.GetState().IsKeyDown(Keys.F10))
            {
                if (screenZoomModeJustChanged) return;
                InputManager.StickMouse();
                if (Config.Zoom < Config.GameScales.Count - 1) Config.Zoom++;
                else Config.Zoom = 0;
                gs.DisplayManager.SetZoom(Config.GameScales[Config.Zoom]);
                screenZoomModeJustChanged = true; // Wait until key release before screen mode can be changed again.
                Config.SaveConfig();
            }
            else
            {
                screenZoomModeJustChanged = false;
            }
        }

        Sprite PreloadTexture(string key)
        {
            string foundPath;
            Texture2D texture;
            if (gs.ResourceManager.LocateResource("sprites", key + ".png", out foundPath))
            {
                gs.ResourceManager.Sprites.TextureLibrary.Load(foundPath, out texture);
                Sprite sprite = new Sprite(gs, key, gs.ResourceManager.Sprites.TextureLibrary.List[key]);
                gs.ResourceManager.Sprites.List.Add(key, sprite);
                return sprite;
            }
            return null;
        }
    }
}
