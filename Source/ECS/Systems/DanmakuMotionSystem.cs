using Engine;
using Engine.ECS;
using Engine.Input;
using Engine.Resource;
using PovertySTG.ECS.Components;
using PovertySTG.Factories;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Text;
using System;

namespace PovertySTG.ECS.Systems
{
    public class MotionSystem : CSystem
    {
        SystemReferences sys;
        public MotionSystem(SystemReferences sys) { this.sys = sys; }

        double timer = 0;
        double timer2 = 0;
        Random r = new Random();

        public override void Update(GameTime gameTime)
        {
            sys.PlayerComponents.TryGetFirstEnabled(out PlayerComponent player);
            //BodyComponent playerBody = sys.BodyComponents.GetByOwner(player.Owner);

            timer += gameTime.ElapsedGameTime.TotalSeconds;
            if (timer > 0.2)
            {
                timer -= 0.2;
                float x = (float)r.NextDouble() * Config.LevelWidth;
                float dy = (float)r.NextDouble() * 3 + 1;
                //DanmakuFactory.MakeBullet(scene, BulletType.BG, x, 0, 0, dy);
            }

            /*
            timer2 += gameTime.ElapsedGameTime.TotalSeconds;
            if (timer2 > 2)
            {
                timer2 -= 2;
                float x = (float)r.NextDouble() * Config.LevelWidth;
                float y = (float)r.NextDouble() * Config.LevelHeight / 2 + 20;
                DanmakuFactory.MakeEnemy(scene, 0, x * 1.5f, -32, x * 0.8f, y);
            }
            */

            foreach (BodyComponent component in sys.BodyComponents.EnabledList)
            {
                component.X += component.DX;
                component.Y += component.DY;
                component.DX += component.DDX;
                component.DY += component.DDY;

                if (component.Pen != Vector4.Zero)
                {
                    float x = component.Pen.X * Config.LevelWidth;
                    float y = component.Pen.Y * Config.LevelHeight;
                    float x2 = component.Pen.Z * Config.LevelWidth;
                    float y2 = component.Pen.W * Config.LevelHeight;
                    if (component.Owner.HasTag("player"))
                    {
                        if (component.X < x || component.X > x2)
                        {
                            sys.SpriteComponents.TryGetByOwner(component.Owner, out SpriteComponent sprite);
                            sprite.CurrentAnimation = "still";
                        }
                    }
                    if (component.X < x) component.X = x;
                    if (component.X > x2) component.X = x2;
                    if (component.Y < y) component.Y = y;
                    if (component.Y > y2) component.Y = y2;
                }

                float margin = component.DeathMargin;
                if (margin > 0)
                {
                    if (component.X < -margin || component.X > Config.LevelWidth + margin
                        || component.Y < -margin || component.Y > Config.LevelHeight + margin)
                    {
                        component.Owner.Delete();
                        continue;
                    }
                }

                if (component.Lifespan > 0)
                {
                    component.Age += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (component.Age >= component.Lifespan)
                    {
                        component.Owner.Delete();
                        continue;
                    }
                }

                if (sys.RenderComponents.TryGetEnabled(component.Owner, out RenderComponent renderComponent))
                {
                    renderComponent.X = component.X;
                    renderComponent.Y = component.Y;
                }
            }

            if (player.SpecialState >= 4 && player.SpecialTimer == 0f && player.SpecialTimer2 == 0f)
            {
                foreach (EnemyComponent enemy in sys.EnemyComponents.EnabledList)
                {
                    BodyComponent body = sys.BodyComponents.GetByOwner(enemy.Owner);
                    DamageEnemy(player, enemy, body, null, 1);
                }
            }

            foreach (BulletComponent component in sys.BulletComponents.EnabledList)
            {
                BodyComponent body = sys.BodyComponents.GetByOwner(component.Owner);
                // Check for collisions.
                if (component.Type >= BulletType.EnemyShot && player.SpecialState >= 4)
                {
                    //DanmakuFactory.MakeSlash(scene, 1, body.X, body.Y);
                    component.Owner.Delete();
                    continue;
                }
                float radius = sys.SpriteComponents.GetByOwner(component.Owner).Sprite.CollisionBox.Radius;
                if (component.Type == BulletType.Coin && body.Y > Config.LevelHeight && body.DY > 0)
                {
                    body.DY = -body.DY * 0.5f;
                    component.Type = BulletType.Coin2;
                }
                if (component.Type == BulletType.Coin || component.Type == BulletType.Coin2)
                {
                    if (body.X < 0 && body.DX < 0) body.DX = -body.DX;
                    if (body.X > Config.LevelWidth && body.DX > 0) body.DX = -body.DX;
                }
                if (body.X > -radius && body.X < Config.LevelWidth + radius
                    && body.Y > -radius && body.Y < Config.LevelHeight + radius)
                {
                    if (component.Type <= BulletType.Items && component.Type > BulletType.BG)
                    {
                        BodyComponent targetBody = sys.BodyComponents.GetByOwner(player.Owner);
                        float targetRadius = 100;
                        float targetRadius2 = sys.SpriteComponents.GetByOwner(player.Owner).Sprite.CollisionBox.Radius;
                        float dx = targetBody.X - body.X;
                        float dy = targetBody.Y - body.Y;
                        float dd = radius + targetRadius;
                        float dd2 = radius + targetRadius2;
                        float distanceSquared = dx * dx + dy * dy;
                        if (distanceSquared < dd2 * dd2)
                        {
                            if (component.Type == BulletType.PointItem) player.Point += 1;
                            else if (component.Type == BulletType.PowerItem) player.Power += 0.05f;
                            else player.Wealth += 0.05f;
                            component.Owner.Delete();
                            continue;
                        }
                        if (distanceSquared < dd * dd)
                        {
                            //body.DX = body.DX * 0.7f;
                            //body.DY = body.DY * 0.7f;
                            float speed = 8f;
                            float distance = (float)Math.Sqrt(distanceSquared) / speed;
                            body.DX = body.DX + (dx / distance - body.DX) * 0.3f;
                            body.DY = body.DY + (dy / distance - body.DY) * 0.3f;
                            continue;
                        }
                        else if (component.Type == BulletType.PointItem || component.Type == BulletType.PowerItem)
                        {
                            body.DX = body.DX * (1 - 0.08f);
                            body.DY = body.DY + (4f - body.DY) * 0.08f;
                        }
                    }
                    if (component.Type == BulletType.PlayerShot || component.Type == BulletType.PowerShot || component.Type == BulletType.HomingShot)
                    {
                        foreach (EnemyComponent enemy in sys.EnemyComponents.EnabledList)
                        {
                            BodyComponent targetBody = sys.BodyComponents.GetByOwner(enemy.Owner);
                            float targetRadius = sys.SpriteComponents.GetByOwner(enemy.Owner).Sprite.CollisionBox.Radius;
                            if (enemy.Shield != null) targetRadius = 100;
                            float dx = targetBody.X - body.X;
                            float dy = targetBody.Y - body.Y;
                            float dd = radius + targetRadius;
                            float distanceSquared = dx * dx + dy * dy;
                            if (distanceSquared < dd * dd)
                            {
                                DamageEnemy(player, enemy, body, component);
                                continue;
                            }
                        }
                    }
                    if (component.Type >= BulletType.EnemyShot)
                    {
                        BodyComponent targetBody = sys.BodyComponents.GetByOwner(player.Owner);
                        float targetRadius = sys.SpriteComponents.GetByOwner(player.Owner).Sprite.CollisionBox.Radius;
                        float dx = targetBody.X - body.X;
                        float dy = targetBody.Y - body.Y;
                        float dd = radius + targetRadius;
                        float distanceSquared = dx * dx + dy * dy;
                        if (distanceSquared < dd * dd)
                        {
                            DanmakuFactory.MakeSlash(scene, 1, body.X, body.Y);
                            component.Owner.Delete();
                            player.Lives--;
                            player.Power -= 0.1f;
                            if (player.Lives < 0) player.Lives = 8;
                            continue;
                        }
                    }
                }
            }

            player.Power = Math.Min(Math.Max(player.Power, 0), 1);
            player.Wealth = Math.Min(Math.Max(player.Wealth, 0), 1);
        }

        void DamageEnemy(PlayerComponent player, EnemyComponent enemy, BodyComponent body, BulletComponent component, int mode = 0)
        {
            if (enemy.Shield != null)
            {
                if (component != null && component.Type != BulletType.PowerShot)
                {
                    //component.Owner.Delete();
                    if (body.DY < 0) body.DY = -body.DY / 2;
                    Random r = new Random();
                    body.DX = (float)(r.NextDouble() - 0.5) * 5f;
                    component.Type = BulletType.BG;
                    return;
                }
            }
            if (mode == 0) DanmakuFactory.MakeSlash(scene, 0, body.X, body.Y);
            if (enemy.Type == EnemyType.MoneyBag) DanmakuFactory.MakeCoin(scene, body.X, body.Y);
            if (component != null)
            {
                component.Owner.Delete();
                if (enemy.Shield != null)
                {
                    enemy.ShieldHealth -= component.Power;
                    sys.SpriteComponents.GetByOwner(enemy.Shield).Scale = (enemy.ShieldHealth + 200) / 300;
                    if (enemy.ShieldHealth <= 0)
                    {
                        enemy.Shield.Delete();
                        enemy.Shield = null;
                    }
                }
                else
                {
                    enemy.Health -= component.Power;
                }
            }
            if (mode == 1 && enemy.Shield == null) enemy.Health -= 30f;
            if (enemy.Health <= 0)
            {
                enemy.Lives--;
                if (enemy.Lives > 0)
                {
                    AddShield(enemy);
                    enemy.Health = enemy.MaxHealth;
                }
                else
                {
                    if (enemy.Type == EnemyType.MoneyBag)
                    {
                        DanmakuFactory.MakeCoin(scene, body.X, body.Y);
                        DanmakuFactory.MakeCoin(scene, body.X, body.Y);
                        DanmakuFactory.MakeCoin(scene, body.X, body.Y);
                        DanmakuFactory.MakeCoin(scene, body.X, body.Y);
                        DanmakuFactory.MakeCoin(scene, body.X, body.Y);
                        DanmakuFactory.MakeCoin(scene, body.X, body.Y);
                    }
                    else
                    {
                        DanmakuFactory.MakePointItem(scene, body.X, body.Y);
                        DanmakuFactory.MakePointItem(scene, body.X, body.Y);
                        DanmakuFactory.MakePointItem(scene, body.X, body.Y);
                        DanmakuFactory.MakePowerItem(scene, body.X, body.Y);
                        DanmakuFactory.MakePowerItem(scene, body.X, body.Y);
                        DanmakuFactory.MakePowerItem(scene, body.X, body.Y);
                    }
                    enemy.Owner.Delete();
                    player.Score += 50;
                }
            }
            else
            {
                player.Score += 1;
                //player.Power += 0.1f;
            }
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
}
