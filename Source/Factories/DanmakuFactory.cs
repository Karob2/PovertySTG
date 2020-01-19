using Engine;
using Engine.ECS;
using Engine.Resource;
using Microsoft.Xna.Framework;
using PovertySTG.ECS.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.Factories
{
    public static class DanmakuFactory
    {
        public static void MakePlayer(Scene scene, float x, float y)
        {
            Entity entity = scene.NewEntity();
            RenderComponent renderComponent = new RenderComponent(20, 0, true);
            entity.AddComponent(renderComponent);
            SpriteComponent sc = new SpriteComponent(scene.GS, "player");
            /*
            Frame frame = sc.Sprite.DefaultAnimationObject.Frames[0];
            sc.Width = frame.Width / 2;
            sc.Height = frame.Height / 2;
            sc.Stretched = true;
            */
            entity.AddComponent(sc);
            entity.AddComponent(new PlayerComponent() { Lives = 5, Bombs = 2});
            BodyComponent body = new BodyComponent(x, y);
            body.Pen = new Vector4(0, 0, 1, 1);
            entity.AddComponent(body);
            entity.Enable();
            entity.AddToGroup("player");

            entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(2, 0, true) { Leader = renderComponent });
            entity.AddComponent(new SpriteComponent(scene.GS, "s_hitbox"));
            entity.Enable();
            entity.AddToGroup("hitbox");
        }

        public static void MakeEnemy(Scene scene, EnemyType type, float x, float y, float targetX, float targetY)
        {
            Random r = new Random();
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(x, y, 30, 0, true));
            if (type >= EnemyType.Boss)
            {
                string animationName = null;
                if (type == EnemyType.Yoshika) animationName = "yoshika_left";
                if (type == EnemyType.Fuyu) animationName = "fuyu_left";
                if (type == EnemyType.Joon) animationName = "joon_left";
                if (type == EnemyType.Shion) animationName = "shion_left";
                entity.AddComponent(new SpriteComponent(scene.GS, "bosses", animationName));
            }
            else if (type == EnemyType.MoneyBag)
                entity.AddComponent(new SpriteComponent(scene.GS, "s_moneybag"));
            else if (type == EnemyType.Fairy)
                entity.AddComponent(new SpriteComponent(scene.GS, "fairy"));
            else
                entity.AddComponent(new SpriteComponent(scene.GS, "fairy", "fairy2"));
            EnemyComponent enemyComponent = new EnemyComponent(type, targetX, targetY);
            if (type == EnemyType.MoneyBag) enemyComponent.Timer = r.NextDouble() * 100f;
            if (type >= EnemyType.Boss) enemyComponent.MaxHealth = 200;
            enemyComponent.Health = enemyComponent.MaxHealth;
            if (type == EnemyType.Yoshika) enemyComponent.Lives = 2;
            if (type == EnemyType.Fuyu) enemyComponent.Lives = 2;
            if (type == EnemyType.Joon) enemyComponent.Lives = 2;
            if (type == EnemyType.Shion) enemyComponent.Lives = 2;
            entity.AddComponent(enemyComponent);
            BodyComponent body = new BodyComponent(x, y);
            if (type == EnemyType.MoneyBag) body.DX = 0.4f + (float)r.NextDouble() * 0.05f;
            if (type < EnemyType.Boss) body.DeathMargin = 100f;
            entity.AddComponent(body);
            entity.Enable();
        }

        public static void MakeDirBullet(Scene scene, BulletType type, float x, float y, float targetX, float targetY, float speed, float accel = 0f)
        {
            Vector2 d = new Vector2(targetX - x, targetY - y);
            Vector2 a = new Vector2(0, 0);
            if (d.X != 0 || d.Y != 0)
            {
                d.Normalize();
                a = d;
                d *= speed;
            }
            a *= accel;
            MakeBullet(scene, type, x, y, d.X, d.Y, a.X, a.Y);
        }

        //public static void MakeBullet(Scene scene, BulletType type, float x, float y, float dx, float dy)
        public static void MakeBullet(Scene scene, BulletType type, float x, float y, float dx, float dy, float ax = 0f, float ay = 0f)
        {
            int layer = 21;
            string sprite = "s_bullet_generic";
            string animation = null;
            float power = 10f;
            if (type == BulletType.BG) layer = 100;
            if (type == BulletType.BG) sprite = "pixel";
            if (type == BulletType.Coin) sprite = "s_coin";
            if (type == BulletType.PointItem) sprite = "s_pointitem";
            if (type == BulletType.PowerItem) sprite = "s_poweritem";
            if (type == BulletType.PlayerShot) sprite = "snakes";
            if (type == BulletType.PowerShot) sprite = "s_coin";
            if (type == BulletType.HomingShot) sprite = "s_star";
            if (type >= BulletType.EnemyShot) layer = 19;
            if (type == BulletType.FRed) sprite = "s_fred";
            if (type == BulletType.FBlue) sprite = "s_fblue";

            if (type == BulletType.PowerShot) power = 4f;

            Entity entity = scene.NewEntity();
            RenderComponent rc = new RenderComponent(x, y, layer, 0, true);
            entity.AddComponent(rc);
            SpriteComponent sc = new SpriteComponent(scene.GS, sprite, animation);
            if (type == BulletType.PowerShot)
            {
                //sc.Rotation = (float)Math.PI / 2;
                //sc.Rotation = (float)(new Random().NextDouble() * Math.PI);
                //sc.Scale = 0.5f;
                //sc.Color = Color.Yellow;
                sc.Alpha = 0.9f;
            }
            if (type == BulletType.PlayerShot)
            {
                sc.Scale = 0.5f;
                sc.Color = new Color(84, 200, 84);
                sc.Alpha = 0.7f;
            }
            if (type == BulletType.HomingShot)
            {
                //sc.Scale = 0.5f;
                sc.Color = new Color(200, 200, 84);
                sc.Alpha = 0.7f;
            }
            if (type == BulletType.EnemyShot)
            {
                //sc.Scale = 0.75f;
            }
            entity.AddComponent(sc);
            entity.AddComponent(new BulletComponent(type, power));
            BodyComponent body = new BodyComponent(x, y, dx, dy);
            body.DeathMargin = 100f;
            body.DDX = ax;
            body.DDY = ay;
            if (type == BulletType.Coin) body.DDY = 0.3f;
            entity.AddComponent(body);
            if (sprite == "s_star") entity.AddComponent(new VfxComponent() { RotateSpeed = 4f });
            entity.Enable();
        }

        public static void MakeCoin(Scene scene, float x, float y)
        {
            Random r = new Random();
            MakeBullet(scene, BulletType.Coin, x, y, ((float)r.NextDouble() - 0.5f) * 4f, ((float)r.NextDouble() - 0.5f) * 2f - 8f);
        }
        public static void MakePointItem(Scene scene, float x, float y)
        {
            Random r = new Random();
            MakeBullet(scene, BulletType.PointItem, x, y, ((float)r.NextDouble() - 0.5f) * 8f, ((float)r.NextDouble() - 0.5f) * 8f);
        }
        public static void MakePowerItem(Scene scene, float x, float y)
        {
            Random r = new Random();
            MakeBullet(scene, BulletType.PowerItem, x, y, ((float)r.NextDouble() - 0.5f) * 8f, ((float)r.NextDouble() - 0.5f) * 8f);
        }

        public static void MakeStar(Scene scene, float x, float y)
        {
            MakeSlash(scene, 10, x, y);
        }

        public static void MakeSlash(Scene scene, int type, float x, float y)
        {
            float alpha = 0.5f;
            float scale = 1f;
            if (type == 0) scale = 0.5f;
            if (type == 2) scale = 0.25f;
            if (type == 10) alpha = 1f;
            float lifespan = 2f;
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(x, y, 5, 0f, true));
            SpriteComponent sprite;
            if (type == 10) sprite = new SpriteComponent(scene.GS, "s_star");
            else
            {
                sprite = new SpriteComponent(scene.GS, "s_slash");
                sprite.Rotation = (float)(new Random().NextDouble() * Math.PI);
            }
            sprite.Alpha = alpha;
            sprite.Scale = scale;
            entity.AddComponent(sprite);
            entity.AddComponent(new VfxComponent() { AlphaSpeed = -alpha / lifespan, ScaleSpeed = -scale / lifespan });
            BodyComponent body = new BodyComponent(x, y);
            body.Lifespan = lifespan;
            entity.AddComponent(body);
            entity.Enable();
        }
    }
}
