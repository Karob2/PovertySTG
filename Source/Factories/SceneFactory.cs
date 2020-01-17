using Engine;
using Engine.ECS;
using PovertySTG.ECS.Components;
using PovertySTG.ECS.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.Factories
{
    public static class SceneFactory
    {
        static List<ButtonComponent> buttons;
        static bool buttonWrap;
        static GameServices gs;
        static Scene scene;

        public static Scene NewMenuScene(GameServices gs, string name)
        {
            NewScene(gs, name, out Scene scene, out SystemReferences sys);

            scene.AddUpdateSystem(new GuiSystem(sys));
            scene.AddRenderSystem(new RenderSystem(sys));

            //MakeCursor();

            StartButtonList(true);
            MakeButton(64, 64, "Start", "start");
            MakeButton(64, 104, "Options", "options");
            MakeButton(64, 144, "Quit", "quit");

            return scene;
        }

        public static Scene NewOptionsScene(GameServices gs, string name)
        {
            NewScene(gs, name, out Scene scene, out SystemReferences sys);

            scene.AddUpdateSystem(new GuiSystem(sys));
            scene.AddRenderSystem(new RenderSystem(sys));

            //MakeCursor();

            StartButtonList(true);
            MakeButton(64, 64, "Music Volume:", null, "m-dn", "m-up");
            MakeButton(64, 104, "Sound Volume:", null, "s-dn", "s-up");
            MakeButton(64, 144, "Fullscreen:", "fs", "fs", "fs");
            MakeButton(64, 184, "Zoom:", "zoom-up", "zoom-dn", "zoom-up");
            MakeButton(64, 224, "Configure Controls", "controls");
            MakeButton(74, 264, "Save and Return", "return");

            return scene;
        }

        public static Scene NewControlsScene(GameServices gs, string name)
        {
            NewScene(gs, name, out Scene scene, out SystemReferences sys);

            scene.AddUpdateSystem(new GuiSystem(sys));
            scene.AddUpdateSystem(new KeyConfigSystem(sys));
            scene.AddRenderSystem(new RenderSystem(sys));

            MakeCursor();
            MakeText(20, 20, "Press escape to go back.");
            MakeText(20, gs.DisplayManager.GameHeight / 2, "Press").AddToGroup("press");
            MakeText(20, gs.DisplayManager.GameHeight - 20, "Conflicts with a previously set key.").AddToGroup("error");

            return scene;
        }

        public static Scene NewDanmakuScene(GameServices gs, string name)
        {
            NewScene(gs, name, out Scene scene, out SystemReferences sys);

            scene.AddUpdateSystem(new PlayerControlSystem(sys));
            scene.AddUpdateSystem(new EnemyControlSystem(sys));
            scene.AddUpdateSystem(new MotionSystem(sys));
            scene.AddUpdateSystem(new VfxSystem(sys));

            scene.AddRenderSystem(new DanmakuGuiSystem(sys));
            scene.AddRenderSystem(new RenderSystem(sys));

            DanmakuFactory.MakePlayer(scene, Config.LevelWidth / 2, Config.LevelHeight * 3 / 4);
            MakeRect(Config.LevelWidth, 0, 1440, 1080, new Color(30, 60, 120));
            int sideMargin = (int)Config.LevelWidth + 40;
            int topMargin = 150;
            int topSpacing = 60;
            MakeText(sideMargin, topMargin, "Score").AddToGroup("score");
            MakeText(sideMargin, topMargin + topSpacing, "Lives").AddToGroup("lives");
            MakeText(sideMargin, topMargin + topSpacing * 2, "Bombs").AddToGroup("bombs");

            return scene;
        }

        public static void MakeCursor()
        {
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(-99, 0));
            entity.AddComponent(new SpriteComponent(gs, "pointer"));
            entity.AddComponent(new PointerComponent());
            entity.Enable();
        }

        public static void MakeRect(float x, float y, float x2, float y2, Color color)
        {
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(x, y, 0, 0));
            SpriteComponent sc = new SpriteComponent(gs, "pixel");
            sc.Color = color;
            sc.Width = x2 - x;
            sc.Height = y2 - y;
            sc.Stretched = true;
            entity.AddComponent(sc);
            entity.Enable();
        }

        public static Entity MakeText(int x, int y, string text)
        {
            return MakeText(x, y, text, Color.White);
        }

        public static Entity MakeText(int x, int y, string text, Color color, string font = "menufont")
        {
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(x, y, -1, 0));
            entity.AddComponent(new TextComponent(gs, font, text, color));
            entity.Enable();
            return entity;
        }

        public static void StartButtonList(bool wrap)
        {
            buttonWrap = wrap;
            buttons = new List<ButtonComponent>();
        }

        public static void MakeButton(int x, int y, string text, string command = null, string leftCommand = null, string rightCommand = null)
        {
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(x, y, 0, 0));
            entity.AddComponent(new TextComponent(gs, "menufont", text, Color.White));
            ButtonComponent button = new ButtonComponent(entity, command, leftCommand, rightCommand);
            button.X = x; button.Y = y;
            entity.AddComponent(button);
            entity.Enable();

            if (buttons.Count > 0)
            {
                if (buttonWrap)
                {
                    ButtonComponent firstButton = buttons[0];
                    if (firstButton.Y < button.Y)
                    {
                        button.Down = firstButton;
                        firstButton.Up = button;
                    }
                    else if (firstButton.X < button.X)
                    {
                        button.Right = firstButton;
                        firstButton.Left = button;
                    }
                }

                ButtonComponent previousButton = buttons[buttons.Count - 1];
                if (previousButton.Y < button.Y)
                {
                    button.Up = previousButton;
                    previousButton.Down = button;
                }
                else if (previousButton.X < button.X)
                {
                    button.Left = previousButton;
                    previousButton.Right = button;
                }
            }
            buttons.Add(button);
        }

        public static void NewScene(GameServices gs, string name, out Scene scene, out SystemReferences sys)
        {
            scene = new Scene(gs, name);
            sys = new SystemReferences(gs, scene);

            SceneFactory.gs = gs;
            SceneFactory.scene = scene;
        }
    }
}
