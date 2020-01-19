using Engine;
using Engine.ECS;
using PovertySTG.ECS.Components;
using PovertySTG.ECS.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using PovertySTG.Engine.Util;

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
            MakeButton(64, 264, "Reset Controls", "controls-r");
            MakeButton(74, 304, "Save and Return", "return");

            return scene;
        }

        public static Scene NewControlsScene(GameServices gs, string name)
        {
            NewScene(gs, name, out Scene scene, out SystemReferences sys);

            scene.AddUpdateSystem(new GuiSystem(sys));
            scene.AddUpdateSystem(new KeyConfigSystem(sys));
            scene.AddRenderSystem(new RenderSystem(sys));

            //MakeCursor();
            MakeText(20, 20, "Press escape to go back.");
            MakeText(20, gs.DisplayManager.GameHeight / 2, "Press").AddToGroup("press");
            MakeText(20, gs.DisplayManager.GameHeight - 20, "Conflicts with a previously set key.").AddToGroup("error");

            return scene;
        }

        public static Scene NewDanmakuScene(GameServices gs, string name)
        {
            NewScene(gs, name, out Scene scene, out SystemReferences sys);

            Entity entity = scene.NewEntity();
            entity.AddComponent(new LevelScriptComponent(scene, "stage1"));
            entity.AddComponent(new CameraComponent(-Config.LevelX, -Config.LevelY));
            entity.Enable();

            scene.AddUpdateSystem(new LevelScriptSystem(sys));
            scene.AddUpdateSystem(new StorySystem(sys));
            scene.AddUpdateSystem(new PlayerControlSystem(sys));
            scene.AddUpdateSystem(new EnemyControlSystem(sys));
            scene.AddUpdateSystem(new MotionSystem(sys));
            scene.AddUpdateSystem(new VfxSystem(sys));

            scene.AddRenderSystem(new DanmakuGuiSystem(sys));
            scene.AddRenderSystem(new RenderSystem(sys));

            DanmakuFactory.MakePlayer(scene, Config.LevelWidth / 2, Config.LevelHeight * 3 / 4);
            /*
            float spawnX = 100;
            float spawnY = 0;
            float targetY = Config.LevelHeight / 2;
            DanmakuFactory.MakeEnemy(scene, 100, spawnX, spawnY, spawnX, targetY);
            spawnX = Config.LevelWidth / 2;
            DanmakuFactory.MakeEnemy(scene, 101, spawnX, spawnY, spawnX, targetY);
            spawnX = Config.LevelWidth - 100;
            DanmakuFactory.MakeEnemy(scene, 102, spawnX, spawnY, spawnX, targetY);
            */

            //MakeRect(Config.LevelWidth, 0, 1440, 1080, new Color(30, 60, 120));
            MakeLevelGui();

            MakeGraphic("textbox", 90, 144, 0, 5).AddToGroup("talk_box").AddToGroup("talk").Disable();
            MakeGraphic("textbox2", 117, 155, 0, 6).AddToGroup("talk_box2").AddToGroup("talk").Disable();
            MakeGraphic("textbox-c", (90 + 117) / 2 - 55, 160 - 19, 0, 1).AddToGroup("talk_box-c").AddToGroup("talk").Disable();
            MakeGraphic("talk_sanae", 126, 258, 0, 10).AddToGroup("talk_portrait").AddToGroup("talk").Disable();
            MakeGraphic("talk_sanae", 462, 260, 0, 10, true).AddToGroup("talk_portrait2").AddToGroup("talk").Disable();
            Color color1 = ColorHelper.FromHex("#fff3e5");
            MakeTalkText(110, 169, "This is the place where they took your sisters and the others?", color1, 330, 4.5f)
                .AddToGroup("talk_text").AddToGroup("talk").Disable();
            MakeTalkText(110 + 27, 169 + 11, "Yes, I can feel the misfortune.", color1, 330, 5.5f)
                .AddToGroup("talk_text2").AddToGroup("talk").Disable();
            MakeTalkText(110 + 27 / 2, 160 + 16, "Just chillin'", color1, 330, 0.5f)
                .AddToGroup("talk_text-c").AddToGroup("talk").Disable();

            MakeRect(0, 0, 612, 600, new Color(128, 128, 128, 128), 0, 999f).AddToGroup("flash").Disable();

            return scene;
        }

        /*
        public static Scene NewTalkScene(GameServices gs, string name, string storyName)
        {
            NewScene(gs, name, out Scene scene, out SystemReferences sys);

            Entity entity = scene.NewEntity();
            //entity.AddComponent(new StoryComponent(scene, "stage1"));
            entity.Enable();

            //scene.AddUpdateSystem(new TalkSystem(sys));
            scene.AddRenderSystem(new RenderSystem(sys));

            MakeGraphic("textbox", 90, 144, 0, 0).AddToGroup("talk_box");
            MakeGraphic("textbox", 117, 155, 0, 0).AddToGroup("talk_box2");
            MakeGraphic("talk_sanae", 2, 258, 0, 0).AddToGroup("talk");
            MakeGraphic("talk_sanae", 338, 260, 0, 0).AddToGroup("talk2");

            return scene;
        }
        */

        public static void MakeCursor()
        {
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(-99, 0));
            entity.AddComponent(new SpriteComponent(gs, "pointer"));
            entity.AddComponent(new PointerComponent());
            entity.Enable();
        }

        public static Entity MakeRect(float x, float y, float x2, float y2, Color color, int layer = 0, float depth = 0f)
        {
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(x, y, layer, depth));
            SpriteComponent sc = new SpriteComponent(gs, "pixel");
            sc.Color = color;
            sc.Width = x2 - x;
            sc.Height = y2 - y;
            sc.Stretched = true;
            entity.AddComponent(sc);
            entity.Enable();
            return entity;
        }

        public static Entity MakeGraphic(string image, float x, float y, int layer, float depth, bool flip = false)
        {
            return MakeGraphic(image, null, x, y, layer, depth, flip);
        }

        public static Entity MakeGraphic(string image, string animation, float x, float y, int layer, float depth, bool flip = false)
        {
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(x, y, layer, depth));
            entity.AddComponent(new SpriteComponent(gs, image, animation));
            entity.Enable();
            return entity;
        }

        public static void MakeGraphicRow(string image, float count, float spacing, float x, float y, int layer, float depth, string group = null)
        {
            Entity entity;
            for (int i = 0; i < count; i++)
            {
                entity = scene.NewEntity();
                entity.AddComponent(new RenderComponent(x, y, layer, depth));
                entity.AddComponent(new SpriteComponent(gs, image));
                entity.Enable();
                if (group != null) entity.AddToGroup(group);
                x += spacing;
            }
        }

        /*
        public static Entity MakeCounter(string image, float x, float y)
        {
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(x, y, -1, 0));
            entity.AddComponent(new SpriteComponent(gs, image) { Tiled = true, Width = 32, Height = 32 });
            entity.Enable();
            return entity;
        }
        */

        public static Entity MakeText(int x, int y, string text)
        {
            return MakeText(x, y, text, Color.White);
        }

        public static Entity MakeText(int x, int y, string text, Color color, string font = "menufont", Alignment align = Alignment.Left)
        {
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(x, y, -1, 0));
            entity.AddComponent(new TextComponent(gs, font, text, color, align));
            entity.Enable();
            return entity;
        }

        public static Entity MakeTalkText(int x, int y, string text, Color color, int width, float depth)
        {
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(x, y, 0, depth));
            entity.AddComponent(new TextComponent(gs, "talkfont", text, color) { Width = width, Border = 0f });
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

        static void MakeLevelGui()
        {
            MakeGraphic("overlay", 0, 0, 0, 0);
            int sideMargin = 604;
            int sideMargin2 = 792;
            int sideMargin3 = sideMargin + 64;
            Color color1 = ColorHelper.FromHex("#fff3e5");
            Color color2 = ColorHelper.FromHex("#a3e0bd");
            Color color3 = ColorHelper.FromHex("#4bc5d6");
            int topMargin = 54;
            int topSpacing = 34;
            MakeText(sideMargin, topMargin, "HiScore:", color1, "scorefont");
            MakeText(sideMargin2, topMargin, "", color1, "scorefont", Alignment.Right).AddToGroup("hiscore");
            topMargin += topSpacing;
            MakeText(sideMargin, topMargin, "Score:", color1, "scorefont");
            MakeText(sideMargin2, topMargin, "", color1, "scorefont", Alignment.Right).AddToGroup("score");
            topMargin += topSpacing;
            topMargin += topSpacing;
            MakeText(sideMargin, topMargin, "Life:", color2, "scorefont");
            //MakeText(sideMargin2, topMargin, "", color2, "scorefont", Alignment.Right).AddToGroup("lives");
            MakeGraphicRow("life", 4, 28, sideMargin3, topMargin - 4, -1, 0, "lives");
            MakeGraphicRow("life", 4, 28, sideMargin3 + 10, topMargin, -1, 0, "lives");
            topMargin += topSpacing;
            MakeText(sideMargin, topMargin, "Bomb:", color2, "scorefont");
            //MakeText(sideMargin2, topMargin, "", color2, "scorefont", Alignment.Right).AddToGroup("bombs");
            MakeGraphicRow("bomb", 4, 28, sideMargin3, topMargin - 4, -1, 0, "bombs");
            MakeGraphicRow("bomb", 4, 28, sideMargin3 + 10, topMargin, -1, 0, "bombs");
            topMargin += topSpacing;
            topMargin += topSpacing;
            MakeText(sideMargin, topMargin, "Power:", color3, "scorefont");
            //MakeText(sideMargin2, topMargin, "0", color3, "scorefont", Alignment.Right).AddToGroup("power");
            MakeGraphic("power", sideMargin3, topMargin + 4, -1, 0);
            MakeRect(sideMargin3 + 2, topMargin + 4 + 2, sideMargin3 + 122, topMargin + 4 + 20, new Color(0, 0, 0), -2).AddToGroup("power");
            topMargin += topSpacing;
            MakeText(sideMargin, topMargin, "Graze:", color3, "scorefont");
            MakeText(sideMargin2, topMargin, "0", color3, "scorefont", Alignment.Right).AddToGroup("graze");
            topMargin += topSpacing;
            MakeText(sideMargin, topMargin, "Point:", color3, "scorefont");
            MakeText(sideMargin2, topMargin, "0", color3, "scorefont", Alignment.Right).AddToGroup("point");

            sideMargin = 26;
            topMargin = 560;
            //MakeText(sideMargin, topMargin, "Power:", color3, "scorefont");
            MakeGraphic("wealth", sideMargin, topMargin, -1, 0);
            MakeRect(sideMargin + 2, topMargin + 2, sideMargin + 122, topMargin + 15, new Color(0, 0, 0), -2).AddToGroup("wealth");
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
