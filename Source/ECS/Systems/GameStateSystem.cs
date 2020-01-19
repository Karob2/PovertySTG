using Engine;
using Engine.ECS;
using Engine.Input;
using Engine.Resource;
using PovertySTG.ECS.Components;
using PovertySTG.Factories;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Systems
{
    public class GameStateSystem : CSystem
    {
        //SystemReferences sys;
        //public GameStateSystem(SystemReferences sys) { this.sys = sys; }

        public override void Update(GameTime gameTime)
        {
            //ComponentGroup<GameStateComponent> gsc = scene.GetComponentGroup<GameStateComponent>();
            if (!scene.GetComponentGroup<GameStateComponent>().TryGetFirstEnabled(out GameStateComponent gsc)) return;
            string request = gsc.Command;
            if (request == null) return;
            if (request == "danmaku")
            {
                scene.AddScene(SceneFactory.NewDanmakuScene(gs, "danmaku")).Enable();
                InputManager.Reset();
            }
            if (request == "options")
            {
                scene.AddScene(SceneFactory.NewOptionsScene(gs, "options")).Enable();
                InputManager.Reset();
            }
            if (request == "controls")
            {
                scene.AddScene(SceneFactory.NewControlsScene(gs, "controls")).Enable();
                InputManager.Reset();
            }
            if (request == "return")
            {
                scene.RemoveLastScene();
                InputManager.Reset();
            }
            if (request == "quit")
            {
                gs.Game.Exit();
            }
            /*
            string[] parts = request.Split(' ');
            if (parts.Length >= 2)
            {
                if (parts[0] == "talk")
                {
                    scene.AddScene(SceneFactory.NewTalkScene(gs, "talk", parts[1])).Enable();
                }
            }
            */
            gsc.Command = null;
        }
    }
}
