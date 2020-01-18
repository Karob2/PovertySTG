using Engine.ECS;
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

        Entity talkBox, talkBox2;
        SpriteComponent talkPortrait, talkPortrait2;
        TextComponent talkText, talkText2;

        public override void Update(GameTime gameTime)
        {
            sys.LevelScriptComponents.TryGetFirstEnabled(out LevelScriptComponent level);

            if (talkBox == null) Bind();

            if (level.Story == null) return;
            if (level.StoryProgress < 0)
            {
                //foreach (Entity entity in scene.GetGroup("talk")) entity.Enable();
                level.StoryProgress = 0;
                StoryLine line = level.Story.Lines[0];
                ShowTalker1(line.Left, line.LeftMessage);
            }
        }

        void Bind()
        {
            talkBox = scene.GetEntity("talk_box");
            talkBox2 = scene.GetEntity("talk_box2");
            talkPortrait = sys.SpriteComponents.GetByOwner("talk_portrait");
            talkPortrait2 = sys.SpriteComponents.GetByOwner("talk_portrait2");
            talkText = sys.TextComponents.GetByOwner("talk_text");
            talkText2 = sys.TextComponents.GetByOwner("talk_text2");
        }

        void ShowTalker1(string expression = null, string text = null)
        {
            if (text == null)
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
            if (expression == null)
            {
                talkPortrait.Owner.Disable();
            }
            else
            {
                string speaker = expression.Split('_')[0];
                talkPortrait.Sprite = gs.ResourceManager.Sprites.Get("talk_" + speaker);
                talkPortrait.CurrentAnimation = expression;
                talkPortrait.Owner.Enable();
            }
        }
    }
}
