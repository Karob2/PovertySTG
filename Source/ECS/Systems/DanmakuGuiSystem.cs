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
            TextComponent lives = sys.TextComponents.GetByOwner("lives");
            TextComponent bombs = sys.TextComponents.GetByOwner("bombs");

            score.Text = player.Score.ToString();
            lives.Text = "Lives: " + player.Lives;
            bombs.Text = "Bombs: " + player.Bombs;
        }
    }
}
