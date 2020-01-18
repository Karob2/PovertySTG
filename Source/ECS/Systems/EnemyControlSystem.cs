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
    public class EnemyControlSystem : CSystem
    {
        SystemReferences sys;
        public EnemyControlSystem(SystemReferences sys) { this.sys = sys; }

        public override void Update(GameTime gameTime)
        {
            sys.PlayerComponents.TryGetFirstEnabled(out PlayerComponent player);
            BodyComponent playerBody = sys.BodyComponents.GetByOwner(player.Owner);
            EnemyData ed = new EnemyData() { playerBody = playerBody, seconds = (float)gameTime.ElapsedGameTime.TotalSeconds };

            foreach (EnemyComponent component in sys.EnemyComponents.EnabledList)
            {
                BodyComponent body = sys.BodyComponents.GetByOwner(component.Owner);
                ed.component = component;
                ed.body = body;
                if (component.Type < 100)
                {
                    if (component.Phase == 0)
                    {
                        MovePhase(ed, 3.8f);
                    }
                    else if (component.Phase > 0 && component.Phase < 6)
                    {
                        ShootPhase(ed, 1f);
                    }
                    else if (component.Phase >= 5)
                    {
                        if (component.Type == 0) component.Phase = -1;
                        else component.Phase = 1;
                    }
                    else
                    {
                        body.DY = -2.2f;
                    }
                }
                else
                {
                    if (component.Phase == 0)
                    {
                        MovePhase(ed, 3.8f);
                    }
                    else if (component.Phase == 1)
                    {
                        WaitPhase(ed, 1f);
                    }
                    else
                    {
                        Random r = new Random();
                        component.TargetX = (float)r.NextDouble() * Config.LevelWidth;
                        component.TargetY = (float)r.NextDouble() * Config.LevelHeight;
                        component.Phase = 0;
                    }
                    UpdateSprite(ed);
                }
            }
        }

        void WaitPhase(EnemyData ed, float waitTime)
        {
            ed.body.DX = 0;
            ed.body.DY = 0;
            ed.component.Timer += ed.seconds;
            if (ed.component.Timer > waitTime)
            {
                ed.component.Timer -= waitTime;
                ed.component.Phase++;
            }
        }

        void MovePhase(EnemyData ed, float speed)
        {
            float dx = ed.component.TargetX - ed.body.X;
            float dy = ed.component.TargetY - ed.body.Y;
            Vector2 d = new Vector2(dx, dy);
            if (d.LengthSquared() > speed * speed)
            {
                d.Normalize();
                d *= speed;
            }
            else
            {
                ed.component.Phase = 1;
            }
            ed.body.DX = d.X;
            ed.body.DY = d.Y;
        }

        void ShootPhase(EnemyData ed, float cooldown)
        {
            ed.body.DX = 0;
            ed.body.DY = 0;
            ed.component.Timer += ed.seconds;
            if (ed.component.Timer > cooldown)
            {
                ed.component.Timer -= cooldown;
                DanmakuFactory.MakeBullet(scene, 1, ed.body.X, ed.body.Y, ed.playerBody.X, ed.playerBody.Y, 3f);
                ed.component.Phase++;
            }
        }

        void UpdateSprite(EnemyData ed)
        {
            int type = ed.component.Type;
            float dx = ed.body.DX;
            float dy = ed.body.DY;
            string name;
            if (type == 100) name = "yoshika";
            else if (type == 101) name = "fuyu";
            else if (type == 102) name = "joon";
            else name = "";
            if (dx < 0) ed.component.Direction = Direction.Left;
            if (dx > 0) ed.component.Direction = Direction.Right;
            string direction;
            if (ed.component.Direction == Direction.Left) direction = "_left";
            else direction = "_right";
            string motion = "";
            if (dx != 0 || dy != 0) motion = "_move";

            sys.SpriteComponents.GetByOwner(ed.component.Owner).CurrentAnimation = name + motion + direction;
        }
    }

    public class EnemyData
    {
        public EnemyComponent component;
        public BodyComponent body;
        public BodyComponent playerBody;
        public float seconds;
    }
}
