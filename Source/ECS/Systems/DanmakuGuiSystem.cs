using Engine;
using Engine.ECS;
using Engine.Input;
using PovertySTG.ECS.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Systems
{
    public class DanmakuGuiSystem : CSystem
    {
        SystemReferences sys;
        public DanmakuGuiSystem(SystemReferences sys) { this.sys = sys; }

        public override void Update(GameTime gameTime)
        {
            sys.PlayerComponents.TryGetFirstEnabled(out PlayerComponent player);
            TextComponent score = sys.TextComponents.GetByOwner("score");
            //TextComponent lives = sys.TextComponents.GetByOwner("lives");
            //SpriteComponent lives = sys.SpriteComponents.GetByOwner("lives");
            List<Entity> lives = scene.GetGroup("lives");
            //TextComponent bombs = sys.TextComponents.GetByOwner("bombs");
            List<Entity> bombs = scene.GetGroup("bombs");

            score.Text = player.Score.ToString();
            //lives.Text = "Lives: " + player.Lives;
            //int lifeCount = Math.Max(Math.Min(player.Lives, 10), 0);
            //lives.Width = 32 * lifeCount;
            int n = 0;
            foreach (Entity life in lives)
            {
                n++;
                if (player.Lives >= n)
                    sys.RenderComponents.GetByOwner(life).Enabled = true;
                else
                    sys.RenderComponents.GetByOwner(life).Enabled = false;
            }
            //bombs.Text = "Bombs: " + player.Bombs;
            n = 0;
            foreach (Entity bomb in bombs)
            {
                n++;
                if (player.Bombs >= n)
                    sys.RenderComponents.GetByOwner(bomb).Enabled = true;
                else
                    sys.RenderComponents.GetByOwner(bomb).Enabled = false;
            }
        }
    }
}
