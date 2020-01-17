using Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    /// <summary>
    /// Class for handling screen resolution, positioning, scaling, and drawing.
    /// </summary>
    public class DisplayManager
    {
        GameServices gs;
        public Game Game { get; set; }
        public GraphicsDeviceManager GraphicsManager { get; set; }
        public SpriteBatch SpriteBatch { get; set; }
        private RenderTarget2D renderTarget;

        // Native screen resolution, used for fullscreen size.
        int nativeWidth, nativeHeight;
        // The size of the game window when not fullscreen.
        int windowedWidth, windowedHeight;
        // The current window or fullscreen size.
        int currentWidth, currentHeight;
        // The resolution of the game area (internal game resolution).
        int baseWidth, baseHeight;
        public int GameWidth { get { return baseWidth; } }
        public int GameHeight { get { return baseHeight; } }
        // The center of the game area.
        public int CenterX { get; private set; }
        public int CenterY { get; private set; }

        public float GameScale { get; set; } = 1f; // How much to scale up the game in windowed mode.
        int viewWidth, viewHeight, viewX, viewY; // Viewport.
        float scaleFactor;
        public Matrix ScaleMatrix { get; set; }

        // Mouse movement can be affected by the display positioning and scaling.
        public int PointerX { get; set; }
        public int PointerY { get; set; }
        public int PointerZ { get; set; }
        int pointerRestX, pointerRestY;
        int pointerOldX, pointerOldY, pointerOldZ;
        bool pointerMoved;
        int pointerBoundLeft, pointerBoundRight, pointerBoundTop, pointerBoundBottom;
        bool lmbPressed, lmbToggled, rmbPressed, rmbToggled;

        bool fullscreen;
        public bool Fullscreen => fullscreen;

        public DisplayManager(GameServices gs)
        {
            this.gs = gs;
            Game = gs.Game;
            GraphicsManager = new GraphicsDeviceManager(gs.Game);
        }

        /// <summary>
        /// Required during game initialization.
        /// </summary>
        public void CreateGraphicsDevice(Game game)
        {
            Game = game;
            GraphicsManager = new GraphicsDeviceManager(game);
        }

        /// <summary>
        /// Initialize the display manager with the game resolution equal to the window resolution.
        /// </summary>
        public void Initialize(int baseWidth, int baseHeight, bool fullscreen = false)
        {
            Initialize(baseWidth, baseHeight, (int)(baseWidth * GameScale), (int)(baseHeight * GameScale), fullscreen);
        }

        /// <summary>
        /// Initialize the display manager with specified game and window resolutions.
        /// </summary>
        public void Initialize(int baseWidth, int baseHeight, int windowedWidth, int windowedHeight, bool fullscreen = false)
        {
            // Not actually native, but best guess without doing some nasty shenanigans.
            nativeWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            nativeHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            /*
            DisplayManager.baseWidth = baseWidth;
            DisplayManager.baseHeight = baseHeight;
            CenterX = baseWidth / 2;
            CenterY = baseHeight / 2;
            */
            this.windowedWidth = windowedWidth;
            this.windowedHeight = windowedHeight;
            this.fullscreen = fullscreen;

            /*
            if (fullscreen) SetFullscreen();
            else SetWindowed();
            */

            SetBase(baseWidth, baseHeight);

            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        public void SetBase(int baseWidth, int baseHeight)
        {
            this.baseWidth = baseWidth;
            this.baseHeight = baseHeight;
            CenterX = baseWidth / 2;
            CenterY = baseHeight / 2;
            windowedWidth = (int)(baseWidth * GameScale);
            windowedHeight = (int)(baseHeight * GameScale);
            if (fullscreen) SetFullscreen();
            else SetWindowed();

            renderTarget = new RenderTarget2D(Game.GraphicsDevice, baseWidth, baseHeight);
        }

        /// <summary>
        /// Change display mode to windowed.
        /// </summary>
        void SetWindowed()
        {
            GraphicsManager.IsFullScreen = false;
            GraphicsManager.PreferredBackBufferWidth = windowedWidth;
            GraphicsManager.PreferredBackBufferHeight = windowedHeight;
            GraphicsManager.ApplyChanges();
            currentWidth = Game.Window.ClientBounds.Width;
            currentHeight = Game.Window.ClientBounds.Height;

            ScaleView();
        }

        /// <summary>
        /// Change display mode to borderless fullscreen at native resolution.
        /// </summary>
        void SetFullscreen()
        {
            GraphicsManager.IsFullScreen = true;
            GraphicsManager.HardwareModeSwitch = false; //borderless window
            GraphicsManager.PreferredBackBufferWidth = nativeWidth;
            GraphicsManager.PreferredBackBufferHeight = nativeHeight;
            GraphicsManager.ApplyChanges();
            currentWidth = Game.Window.ClientBounds.Width;
            currentHeight = Game.Window.ClientBounds.Height;

            ScaleView();
        }

        public void SetFullscreen(bool fullscreen)
        {
            this.fullscreen = fullscreen;
            if (fullscreen) SetFullscreen();
            else SetWindowed();
        }

        public void SetZoom(float zoom)
        {
            GameScale = zoom;
            windowedWidth = (int)(baseWidth * zoom);
            windowedHeight = (int)(baseHeight * zoom);
            if (fullscreen) SetFullscreen();
            else SetWindowed();
        }

        /// <summary>
        /// Calculate the viewport area and scaling factors to make the game fit properly inside the window.
        /// </summary>
        private void ScaleView()
        {
            viewWidth = baseWidth * currentHeight / baseHeight;
            if (viewWidth <= currentWidth)
            {
                viewHeight = currentHeight;
                scaleFactor = (float)currentHeight / baseHeight;
            }
            else
            {
                viewWidth = currentWidth;
                viewHeight = baseHeight * currentWidth / baseWidth;
                scaleFactor = (float)currentWidth / baseWidth;
            }
            ScaleMatrix = Matrix.CreateScale(scaleFactor, scaleFactor, 1f);
            viewX = (currentWidth - viewWidth) / 2;
            viewY = (currentHeight - viewHeight) / 2;
            Game.GraphicsDevice.Viewport = new Viewport(viewX, viewY, viewWidth, viewHeight);
            //ResetPointerLocation();
            InputManager.Reset();
        }

        #region drawing methods
        // TODO: Consider moving some or all drawing methods to their own classes.

        /// <summary>
        /// Begin the global SpriteBatch.
        /// </summary>
        public void StartDrawing(Color color)//GameTime gameTime)
        {
            Game.GraphicsDevice.SetRenderTarget(renderTarget);
            //DeltaDrawSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Game.GraphicsDevice.Clear(color);
            //SpriteBatch.Begin(blendState: BlendState.NonPremultiplied);
            //SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, DisplayManager.ScaleMatrix);
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
        }

        /*
        public void StartDrawingWrapped()
        {
            Game.GraphicsDevice.Clear(Color.Black);
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap, null, null, null, DisplayManager.ScaleMatrix);
        }
        */

        /// <summary>
        /// End the global SpriteBatch.
        /// </summary>
        public void StopDrawing()
        {
            SpriteBatch.End();

            Game.GraphicsDevice.SetRenderTarget(null);
            Game.GraphicsDevice.Viewport = new Viewport(viewX, viewY, viewWidth, viewHeight);
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearClamp, null, null, null, this.ScaleMatrix);
            SpriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            SpriteBatch.End();
        }

        // Of the following texture methods, only DrawWrapped is used so far.

        /// <summary>
        /// Draw texture to game area.
        /// </summary>
        public void DrawTexture(Texture2D texture, float x, float y)
        {
            SpriteBatch.Draw(texture, ToPixel(x, y), Color.White);
        }

        /// <summary>
        /// Draw centered texture to game area.
        /// </summary>
        public void DrawCentered(Texture2D texture, float x, float y)
        {
            float centeredX = x - texture.Width / 2;
            float centeredY = y - texture.Height / 2;
            SpriteBatch.Draw(texture, ToPixel(centeredX, centeredY), Color.White);
        }

        /// <summary>
        /// Draw wrapped (infinite) texture to game area.
        /// </summary>
        public void DrawWrapped(Texture2D texture, float x, float y)
        {
            float startX = (x % texture.Width + texture.Width) % texture.Width - texture.Width;
            float startY = (y % texture.Height + texture.Height) % texture.Height - texture.Height;
            for (float i = startX; i < baseWidth; i += texture.Width)
            {
                for (float j = startY; j < baseHeight; j += texture.Height)
                {
                    SpriteBatch.Draw(texture, ToPixel(i, j), Color.White);
                }
            }
        }

        #endregion

        // Methods to round render coordinates to nearest pixel.
        // TODO: Might be a pointless waste of resources.
        public Vector2 ToPixel(float x, float y)
        {
            //return new Vector2(x, y);
            int i, j;
            i = (int)x;
            if (x < 0) i = (int)(x - i + 1) + i - 1;
            j = (int)y;
            if (y < 0) j = (int)(y - j + 1) + j - 1;
            return new Vector2(i, j);
        }
        public Rectangle ToPixel(float x, float y, int width, int height)
        {
            int i, j;
            i = (int)x;
            if (x < 0) i = (int)(x - i + 1) + i - 1;
            j = (int)y;
            if (y < 0) j = (int)(y - j + 1) + j - 1;
            return new Rectangle(i, j, width, height);
        }
        public Rectangle ToPixel(float x, float y, float width, float height)
        {
            int i, j;
            i = (int)x;
            if (x < 0) i = (int)(x - i + 1) + i - 1;
            j = (int)y;
            if (y < 0) j = (int)(y - j + 1) + j - 1;
            return new Rectangle(i, j, (int)width, (int)height);
        }

        public Point ScaleCoordinates(Point input)
        {
            int scaledX = (int)((input.X - viewX) / scaleFactor);
            int scaledY = (int)((input.Y - viewY) / scaleFactor);
            return new Point(scaledX, scaledY);
        }

        public Point ReverseScaleCoordinates(Point input)
        {
            int scaledX = (int)(input.X * scaleFactor) + viewX;
            int scaledY = (int)(input.Y * scaleFactor) + viewY;
            return new Point(scaledX, scaledY);
        }
    }
}
