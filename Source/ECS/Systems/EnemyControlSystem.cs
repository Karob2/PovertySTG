﻿using Engine;
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
            sys.LevelScriptComponents.TryGetFirstEnabled(out LevelScriptComponent level);

            sys.PlayerComponents.TryGetFirstEnabled(out PlayerComponent player);
            BodyComponent playerBody = sys.BodyComponents.GetByOwner(player.Owner);
            EnemyData ed = new EnemyData() { playerBody = playerBody, seconds = (float)gameTime.ElapsedGameTime.TotalSeconds };

            foreach (EnemyComponent component in sys.EnemyComponents.EnabledList)
            {
                BodyComponent body = sys.BodyComponents.GetByOwner(component.Owner);
                if (level.Story != null && component.Phase > 0)
                {
                    body.DX = 0;
                    body.DY = 0;
                    continue;
                }
                ed.component = component;
                ed.body = body;
                if (component.Type <= EnemyType.BraveFairy)
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
                else if (component.Type == EnemyType.MoneyBag)
                {
                    if (component.Phase == 0)
                    {
                        if (body.X < 0 && body.DX < 0) body.DX = -body.DX;
                        if (body.X > Config.LevelWidth && body.DX > 0) body.DX = -body.DX;
                        component.Timer += ed.seconds;
                        body.DY = (float)Math.Sin(component.Timer) / 2;
                        if (Math.Abs(body.X - component.TargetX) < 10)
                        {
                            component.Phase = 1;
                            component.Timer = 0;
                        }
                    }
                    if (component.Phase == 1)
                    {
                        component.Timer += ed.seconds;
                        body.DY = (float)Math.Sin(component.Timer) / 2;
                        body.DX = (float)Math.Cos(component.Timer) / 4;
                        if (component.Timer > 12f)
                        {
                            component.Phase = 2;
                            Random r = new Random();
                            body.DX = 0.4f + (float)r.NextDouble() * 0.05f;
                            if (body.X < Config.LevelWidth / 2) body.DX = -body.DX;
                            //if (r.NextDouble() < 0.5) body.DX = -body.DX;
                        }
                    }
                    if (component.Phase == 2)
                    {
                        component.Timer += ed.seconds;
                        body.DY = (float)Math.Sin(component.Timer) / 2;
                    }
                }
                else if (component.Type >= EnemyType.Boss)
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
                DanmakuFactory.MakeBullet(scene, BulletType.EnemyShot, ed.body.X, ed.body.Y, ed.playerBody.X, ed.playerBody.Y, 3f);
                ed.component.Phase++;
            }
        }

        void UpdateSprite(EnemyData ed)
        {
            EnemyType type = ed.component.Type;
            float dx = ed.body.DX;
            float dy = ed.body.DY;
            string name;
            if (type == EnemyType.Yoshika) name = "yoshika";
            else if (type == EnemyType.Fuyu) name = "fuyu";
            else if (type == EnemyType.Joon) name = "joon";
            else if (type == EnemyType.Shion) name = "shion";
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
