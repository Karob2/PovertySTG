using Engine;
using Engine.ECS;
using Engine.Input;
using PovertySTG.ECS.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Engine.Resource;
using PovertySTG.Factories;

namespace PovertySTG.ECS.Systems
{
    public class LevelScriptSystem : CSystem
    {
        SystemReferences sys;
        public LevelScriptSystem(SystemReferences sys) { this.sys = sys; }

        public override void Update(GameTime gameTime)
        {
            sys.LevelScriptComponents.TryGetFirstEnabled(out LevelScriptComponent level);
            float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            LevelScript levelScript = level.Level;
            if (levelScript == null) return;
            switch (level.WaitMode)
            {
                case LevelWaitMode.Start:
                    ProgressLevel(level, levelScript);
                    break;
                case LevelWaitMode.Timer:
                    if (level.WaitTimer > 0) level.WaitTimer -= seconds;
                    if (level.WaitTimer <= 0)
                    {
                        ProgressLevel(level, levelScript);
                    }
                    break;
                case LevelWaitMode.Leave:
                    if (CheckLeave()) ProgressLevel(level, levelScript);
                    break;
                case LevelWaitMode.Clear:
                    if (CheckClear()) ProgressLevel(level, levelScript);
                    break;
            }
        }

        bool CheckClear()
        {
            foreach (EnemyComponent enemy in sys.EnemyComponents.EnabledList)
            {
                return false;
            }
            return true;
        }

        bool CheckLeave()
        {
            foreach (EnemyComponent enemy in sys.EnemyComponents.EnabledList)
            {
                if (enemy.Phase >= 0) return false;
            }
            return true;
        }

        void ProgressLevel(LevelScriptComponent level, LevelScript levelScript)
        {
            List<string> lines = levelScript.Lines;

            while (level.Progress < lines.Count - 1)
            {
                level.Progress++;
                string[] line = lines[level.Progress].Split(' ');
                if (line.Length < 1)
                {
                    continue;
                }
                switch (line[0])
                {
                    case "loop":
                        level.LoopPoint = level.Progress + 1;
                        break;
                    case "repeat":
                        level.Progress = level.LoopPoint - 1;
                        continue;
                }
                if (line.Length < 2) continue;
                switch (line[0])
                {
                    case "summon":
                        Random r = new Random();
                        float x = (float)r.NextDouble() * Config.LevelWidth;
                        float y = (float)r.NextDouble() * Config.LevelHeight / 2 + 20;
                        if (line[1] == "cute")
                            DanmakuFactory.MakeEnemy(scene, 0, x, 0, x, y);
                        else if (line[1] == "dork")
                            DanmakuFactory.MakeEnemy(scene, 1, x, 0, x, y);
                        else if (line[1] == "yoshika")
                            DanmakuFactory.MakeEnemy(scene, 100, x, 0, x, y);
                        else if (line[1] == "fuyu")
                            DanmakuFactory.MakeEnemy(scene, 101, x, 0, x, y);
                        else if (line[1] == "joon")
                            DanmakuFactory.MakeEnemy(scene, 102, x, 0, x, y);
                        break;
                    case "wait":
                        if (line[1] == "clear")
                        {
                            level.WaitMode = LevelWaitMode.Clear;
                            return;
                        }
                        else if (line[1] == "leave")
                        {
                            level.WaitMode = LevelWaitMode.Leave;
                            return;
                        }
                        else if (float.TryParse(line[1], out float time))
                        {
                            level.WaitMode = LevelWaitMode.Timer;
                            level.WaitTimer = time;
                            return;
                        }
                        break;
                    case "talk":
                        //sys.GetGameState().Request(lines[level.Progress]);
                        //scene.AddScene(SceneFactory.NewTalkScene(gs, "talk", line[1])).Enable();
                        level.WaitMode = LevelWaitMode.Forever;
                        return;
                }
            }

            // Reached end of script with no remaining action. Go dormant.
            level.Level = null;
            level.Progress = 0;
            level.WaitMode = LevelWaitMode.Start;
            level.LoopPoint = 0;
            return;
        }
    }
}
