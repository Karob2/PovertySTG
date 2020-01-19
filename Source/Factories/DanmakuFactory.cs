using Engine;
using Engine.ECS;
using Engine.Resource;
using Microsoft.Xna.Framework;
using PovertySTG.ECS.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace PovertySTG.Factories
{
    public static class DanmakuFactory
    {
        public static void MakePlayer(Scene scene, float x, float y)
        {
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(20, 0, true));
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
            entity.AddComponent(new RenderComponent(2, 0, true));
            entity.AddComponent(new SpriteComponent(scene.GS, "s_hitbox"));
            entity.Enable();
            entity.AddToGroup("hitbox");
        }

        public static void MakeEnemy(Scene scene, int type, float x, float y, float targetX, float targetY)
        {
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(x, y, 30, 0, true));
            if (type < 100) entity.AddComponent(new SpriteComponent(scene.GS, "s_fair"));
            if (type >= 100)
            {
                string animationName = null;
                if (type == 100) animationName = "yoshika_left";
                if (type == 101) animationName = "fuyu_left";
                if (type == 102) animationName = "joon_left";
                if (type == 103) animationName = "shion_left";
                entity.AddComponent(new SpriteComponent(scene.GS, "bosses", animationName));
            }
            entity.AddComponent(new EnemyComponent(type, targetX, targetY));
            BodyComponent body = new BodyComponent(x, y);
            if (type < 100) body.DeathMargin = 100f;
            entity.AddComponent(body);
            entity.Enable();
        }

        public static void MakeBullet(Scene scene, int type, float x, float y, float targetX, float targetY, float speed)
        {
            Vector2 d = new Vector2(targetX - x, targetY - y);
            if (d.X != 0 || d.Y != 0)
            {
                d.Normalize();
                d *= speed;
            }
            MakeBullet(scene, type, x, y, d.X, d.Y);
        }

        public static void MakeBullet(Scene scene, int type, float x, float y, float dx, float dy)
        {
            int layer = 21;
            string sprite = "s_shot";
            string animation = null;
            if (type < 0) layer = 100;
            if (type < 0) sprite = "pixel";
            if (type == 0) sprite = "s_pshot";
            if (type > 0) layer = 19;

            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(x, y, layer, 0, true));
            entity.AddComponent(new SpriteComponent(scene.GS, sprite, animation));
            entity.AddComponent(new BulletComponent(type));
            BodyComponent body = new BodyComponent(x, y, dx, dy);
            body.DeathMargin = 100f;
            entity.AddComponent(body);
            if (type == 0) entity.AddComponent(new VfxComponent() { RotateSpeed = 4f });
            entity.Enable();
        }

        public static void MakeSlash(Scene scene, int type, float x, float y)
        {
            float alpha = 0.5f;
            float scale = 0.5f;
            if (type == 1) scale = 1f;
            float lifespan = 0.4f;
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(x, y, 5, 0f, true));
            SpriteComponent sprite = new SpriteComponent(scene.GS, "s_slash");
            sprite.Rotation = (float)(new Random().NextDouble() * Math.PI);
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
