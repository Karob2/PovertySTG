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

            while (level.Progress < lines.Count)
            {
                //level.Owner.Scene.GS.Error.DebugPrint(lines[level.Progress]);
                string[] line = lines[level.Progress].Split(' ');
                if (line.Length < 1)
                {
                    level.Progress++;
                    continue;
                }
                switch (line[0])
                {
                    case "loop":
                        level.LoopPoint = level.Progress + 1;
                        break;
                    case "repeat":
                        level.Progress = level.LoopPoint; // TODO: This can cause an infinite loop.
                        level.Progress++;
                        continue;
                    case "leave":
                        foreach (EnemyComponent enemy in sys.EnemyComponents.EnabledList)
                        {
                            enemy.Phase = -1;
                        }
                        break;
                    case "clear":
                        foreach (BulletComponent bullet in sys.BulletComponents.EnabledList)
                        {
                            if (bullet.Type > BulletType.Items) bullet.Owner.Delete();
                        }
                        foreach (EnemyComponent enemy in sys.EnemyComponents.EnabledList)
                        {
                            enemy.Owner.Delete();
                        }
                        break;
                }
                if (line.Length < 2)
                {
                    level.Progress++;
                    continue;
                }
                switch (line[0])
                {
                    case "summon":
                        Random r = new Random();
                        float x = (float)r.NextDouble() * Config.LevelWidth;
                        float y = (float)r.NextDouble() * Config.LevelHeight / 2 + 20;
                        if (line[1] == "fairy")
                            DanmakuFactory.MakeEnemy(scene, EnemyType.Fairy, x, 0, x, y);
                        else if (line[1] == "brave_fairy")
                            DanmakuFactory.MakeEnemy(scene, EnemyType.BraveFairy, x, 0, x, y);
                        else if (line[1] == "moneybag")
                        {
                            float x2;
                            if (x > Config.LevelWidth / 2) x2 = 0 - 50;
                            else x2 = Config.LevelWidth + 50;
                            DanmakuFactory.MakeEnemy(scene, EnemyType.MoneyBag, x2, y, x, y);
                        }
                        else if (line[1] == "yoshika")
                            DanmakuFactory.MakeEnemy(scene, EnemyType.Yoshika, x, 0, x, y);
                        else if (line[1] == "fuyu")
                            DanmakuFactory.MakeEnemy(scene, EnemyType.Fuyu, x, 0, x, y);
                        else if (line[1] == "joon")
                            DanmakuFactory.MakeEnemy(scene, EnemyType.Joon, x, 0, x, y);
                        else if (line[1] == "shion")
                            DanmakuFactory.MakeEnemy(scene, EnemyType.Shion, x, 0, x, y);
                        break;
                    case "wait":
                        if (line[1] == "clear")
                        {
                            level.WaitMode = LevelWaitMode.Clear;
                            level.Progress++;
                            return;
                        }
                        else if (line[1] == "leave")
                        {
                            level.WaitMode = LevelWaitMode.Leave;
                            level.Progress++;
                            return;
                        }
                        else if (float.TryParse(line[1], out float time))
                        {
                            level.WaitMode = LevelWaitMode.Timer;
                            level.WaitTimer = time;
                            level.Progress++;
                            return;
                        }
                        break;
                    case "talk":
                        //sys.GetGameState().Request(lines[level.Progress]);
                        //scene.AddScene(SceneFactory.NewTalkScene(gs, "talk", line[1])).Enable();
                        level.Story = gs.ResourceManager.Stories.Get(line[1]);
                        level.StoryProgress = -1;
                        level.WaitMode = LevelWaitMode.Forever;
                        level.Progress++;
                        return;
                    case "level":
                        level.Level = gs.ResourceManager.LevelScripts.Get(line[1]);
                        level.Progress = 0;
                        level.LoopPoint = 0;
                        level.WaitMode = LevelWaitMode.Start;
                        return;
                }
                level.Progress++;
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
