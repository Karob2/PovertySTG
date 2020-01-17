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

            MakeCursor();

            StartButtonList(true);
            MakeButton(64, 64, "Start", "start");
            MakeButton(64, 84, "Options", "options");
            MakeButton(64, 104, "Quit", "quit");

            return scene;
        }

        public static Scene NewOptionsScene(GameServices gs, string name)
        {
            NewScene(gs, name, out Scene scene, out SystemReferences sys);

            scene.AddUpdateSystem(new GuiSystem(sys));
            scene.AddRenderSystem(new RenderSystem(sys));

            MakeCursor();

            StartButtonList(true);
            MakeButton(64, 44, "Music Volume:", null, "m-dn", "m-up");
            MakeButton(64, 64, "Sound Volume:", null, "s-dn", "s-up");
            MakeButton(64, 84, "Fullscreen:", "fs", "fs", "fs");
            MakeButton(64, 104, "Zoom:", "zoom-up", "zoom-dn", "zoom-up");
            MakeButton(64, 124, "Configure Controls", "controls");
            MakeButton(74, 144, "Save and Return", "return");

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
            scene.AddUpdateSystem(new MotionSystem(sys));
            scene.AddRenderSystem(new RenderSystem(sys));

            MakePlayer(90, 90);

            return scene;
        }

        public static void MakePlayer(float x, float y)
        {
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(1, 0));
            entity.AddComponent(new SpriteComponent(gs, "cirno", "walk_up"));
            entity.AddComponent(new PlayerComponent(x, y));
            entity.Enable();
        }

        public static void MakeCursor()
        {
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(-99, 0));
            entity.AddComponent(new SpriteComponent(gs, "pointer"));
            entity.AddComponent(new PointerComponent());
            entity.Enable();
        }

        public static Entity MakeText(int x, int y, string text)
        {
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(x, y, 0, 0));
            entity.AddComponent(new TextComponent(gs, "pcsenior", text, Color.White));
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
            entity.AddComponent(new TextComponent(gs, "pcsenior", text, Color.White));
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
