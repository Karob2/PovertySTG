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
                        if (component.Type == EnemyType.Fairy) component.Phase = -1;
                        else if (component.Type == EnemyType.BraveFairy)
                        {
                            //component.Phase = 1;
                            //if (component.Shield == null) AddShield(component);
                            component.Phase++;
                            if (component.Phase >= 9) component.Phase = -1;
                        }
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
                    component.MoneyTimer -= ed.seconds;
                    if (component.MoneyTimer < 0f)
                    {
                        Random r = new Random();
                        float x = (float)r.NextDouble() * Config.LevelWidth;
                        float y = (float)r.NextDouble() * Config.LevelHeight / 2 + 20;
                        float x3;
                        if (x > Config.LevelWidth / 2) x3 = 0 - 50;
                        else x3 = Config.LevelWidth + 50;
                        DanmakuFactory.MakeEnemy(scene, EnemyType.MoneyBag, x3, y, x, y);
                        component.MoneyTimer = 16f;
                    }
                    if (component.Phase == 0)
                    {
                        if (component.Type == EnemyType.Yoshika)
                        {
                            component.TargetX = playerBody.X;
                            component.TargetY = playerBody.Y - 330;
                        }
                        //MovePhaseBoss(ed);
                        MovePhase(ed, 3.8f, 1.7f, 1.7f);
                    }
                    else if (component.Phase == 1)
                    {
                        if (component.Type == EnemyType.Yoshika)
                        {
                            float wait = 0.5f;
                            float speed = 0.1f;
                            float accel = 0.01f;
                            WaitPhase(ed, wait);
                            component.Timer2 += ed.seconds;
                            if (component.Timer2 > 0.06f)
                            {
                                component.Timer2 = 0;
                                float dd = ((float)component.Timer / wait - 0.5f) * 2f;
                                float bx = dd * 200f;
                                float by = dd * 100f;
                                DanmakuFactory.MakeDirBullet(scene, BulletType.EnemyShot, ed.body.X + bx, ed.body.Y + 100 + by, ed.playerBody.X, ed.playerBody.Y, speed, accel);
                                DanmakuFactory.MakeDirBullet(scene, BulletType.EnemyShot, ed.body.X - bx, ed.body.Y + 100 + by, ed.playerBody.X, ed.playerBody.Y, speed, accel);
                            }
                        }
                        else component.Phase = 2;
                        //DanmakuFactory.MakeBullet(scene, BulletType.EnemyShot, ed.body.X, ed.body.Y, ed.playerBody.X, ed.playerBody.Y, 3f);
                    }
                    else if (component.Phase == 2)
                    {
                        component.Phase = 0;
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
                ed.component.Timer = 0;
                ed.component.Phase++;
            }
        }

        void MovePhase(EnemyData ed, float speed, float minWaitTime = 0f, float waitTime = 100f)
        {
            if (ed.component.Type == EnemyType.Yoshika) speed = 2f;
            float dx = ed.component.TargetX - ed.body.X;
            float dy = ed.component.TargetY - ed.body.Y;
            Vector2 d = new Vector2(dx, dy);
            ed.component.Timer += ed.seconds;
            if (d.LengthSquared() > speed * speed)
            {
                d.Normalize();
                d *= speed;
                if (ed.component.Timer > waitTime)
                {
                    ed.component.Timer = 0;
                    ed.component.Phase = 1;
                }
            }
            else if (ed.component.Timer > minWaitTime)
            {
                ed.component.Timer = 0;
                ed.component.Phase = 1;
            }
            ed.body.DX = d.X;
            ed.body.DY = d.Y;
        }

        /*
        void MovePhaseBoss(EnemyData ed)
        {
            float speed = 0f;
            float duration = 5;
            switch (ed.component.Type)
            {
                case EnemyType.Yoshika:
                    speed = 3f;
                    break;
            }
        }
        */

        void ShootPhase(EnemyData ed, float cooldown)
        {
            ed.body.DX = 0;
            ed.body.DY = 0;
            ed.component.Timer += ed.seconds;
            if (ed.component.Timer > cooldown)
            {
                ed.component.Timer -= cooldown;
                if (ed.component.Type == EnemyType.BraveFairy)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        double r = i * Math.PI * 2 / 12;
                        float dx = (float)Math.Sin(r);
                        float dy = (float)Math.Cos(r);
                        DanmakuFactory.MakeBullet(scene, BulletType.EnemyShot, ed.body.X, ed.body.Y, dx * 2f, dy * 2f);
                    }
                }
                else
                {
                    DanmakuFactory.MakeDirBullet(scene, BulletType.EnemyShot, ed.body.X, ed.body.Y, ed.playerBody.X, ed.playerBody.Y, 3f);
                }
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

        void AddShield(EnemyComponent component)
        {
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(0, 0, 1, -100, true) { Leader = sys.RenderComponents.GetByOwner(component.Owner) });
            entity.AddComponent(new SpriteComponent(gs, "s_shield") { Color = Color.Yellow });
            entity.Enable();
            component.Shield = entity;
            component.ShieldHealth = 100f;
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
