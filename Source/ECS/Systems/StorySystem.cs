﻿using Engine.ECS;
using Engine.Input;
using Engine.Resource;
using Microsoft.Xna.Framework;
using PovertySTG.ECS.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Systems
{
    public class StorySystem : CSystem
    {
        SystemReferences sys;
        public StorySystem(SystemReferences sys) { this.sys = sys; }

        Entity talkBox, talkBox2, talkBoxC;
        SpriteComponent talkPortrait, talkPortrait2;
        TextComponent talkText, talkText2, talkTextC;

        public override void Update(GameTime gameTime)
        {
            sys.LevelScriptComponents.TryGetFirstEnabled(out LevelScriptComponent level);

            if (talkBox == null) Bind();
            if (level.Story == null) return;

            level.WaitTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (level.StoryProgress < 0)
            {
                //foreach (Entity entity in scene.GetGroup("talk")) entity.Enable();
                level.StoryProgress = 0;
                level.WaitTimer = 0;
                StoryLine line = level.Story.Lines[0];
                ShowNarrator(line.Message);
                ShowTalker1(line.Left, line.LeftMessage);
                ShowTalker2(line.Right, line.RightMessage);
                foreach (BulletComponent bullet in sys.BulletComponents.EnabledList)
                {
                    if (bullet.Type > BulletType.Items) bullet.Owner.Delete();
                }
            }
            else if (InputManager.JustPressed(GameCommand.Action1) && level.WaitTimer >= 0.5f)
            {
                level.StoryProgress++;
                if (level.StoryProgress >= level.Story.Lines.Count)
                {
                    level.Story = null;
                    level.WaitMode = LevelWaitMode.Start;
                    ShowNarrator(null);
                    ShowTalker1(null, null);
                    ShowTalker2(null, null);
                    InputManager.Reset();
                }
                else
                {
                    StoryLine line = level.Story.Lines[level.StoryProgress];
                    ShowNarrator(line.Message);
                    ShowTalker1(line.Left, line.LeftMessage);
                    ShowTalker2(line.Right, line.RightMessage);
                }
            }
        }

        void Bind()
        {
            talkBox = scene.GetEntity("talk_box");
            talkBox2 = scene.GetEntity("talk_box2");
            talkBoxC = scene.GetEntity("talk_box-c");
            talkPortrait = sys.SpriteComponents.GetByOwner("talk_portrait");
            talkPortrait2 = sys.SpriteComponents.GetByOwner("talk_portrait2");
            talkText = sys.TextComponents.GetByOwner("talk_text");
            talkText2 = sys.TextComponents.GetByOwner("talk_text2");
            talkTextC = sys.TextComponents.GetByOwner("talk_text-c");
        }

        void ShowNarrator(string text = null)
        {
            if (text == null || text == "")
            {
                talkBoxC.Disable();
                talkTextC.Owner.Disable();
            }
            else
            {
                talkTextC.Text = text;
                talkBoxC.Enable();
                talkTextC.Owner.Enable();
            }
        }

        void ShowTalker1(string expression = null, string text = null)
        {
            if (text == null || text == "")
            {
                talkBox.Disable();
                talkText.Owner.Disable();
            }
            else
            {
                talkText.Text = text;
                talkBox.Enable();
                talkText.Owner.Enable();
            }
            if (expression == null || expression == "")
            {
                talkPortrait.Owner.Disable();
            }
            else
            {
                string speaker = expression.Split('_')[0];
                if (gs.ResourceManager.Sprites.Get("talk_" + speaker, out Sprite sprite))
                    talkPortrait.Sprite = sprite;
                talkPortrait.CurrentAnimation = expression;
                talkPortrait.Owner.Enable();
            }
        }

        void ShowTalker2(string expression = null, string text = null)
        {
            if (text == null || text == "")
            {
                talkBox2.Disable();
                talkText2.Owner.Disable();
            }
            else
            {
                talkText2.Text = text;
                talkBox2.Enable();
                talkText2.Owner.Enable();
            }
            if (expression == null || expression == "")
            {
                talkPortrait2.Owner.Disable();
            }
            else
            {
                string speaker = expression.Split('_')[0];
                if (gs.ResourceManager.Sprites.Get("talk_" + speaker, out Sprite sprite))
                    talkPortrait2.Sprite = sprite;
                talkPortrait2.CurrentAnimation = expression + "_flip";
                talkPortrait2.Owner.Enable();
            }
        }
    }
}
