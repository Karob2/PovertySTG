﻿using Engine;
using Engine.ECS;
using Engine.Resource;
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
            entity.AddComponent(new RenderComponent(1, 0));
            SpriteComponent sc = new SpriteComponent(scene.GS, "s_sanny");
            /*
            Frame frame = sc.Sprite.DefaultAnimationObject.Frames[0];
            sc.Width = frame.Width / 2;
            sc.Height = frame.Height / 2;
            sc.Stretched = true;
            */
            entity.AddComponent(sc);
            entity.AddComponent(new PlayerComponent());
            entity.AddComponent(new BodyComponent(x, y));
            entity.Enable();
        }

        public static void MakeBullet(Scene scene, int type, float x, float y, float dx, float dy)
        {
            int layer = 1;
            string sprite = "s_shot";
            string animation = null;
            if (type < 0) layer = 100;
            if (type < 0) sprite = "pixel";
            if (type == 0) sprite = "s_pshot";

            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(x, y, layer, 0));
            entity.AddComponent(new SpriteComponent(scene.GS, sprite, animation));
            entity.AddComponent(new BulletComponent(type));
            entity.AddComponent(new BodyComponent(x, y, dx, dy));
            if (type == 0) entity.AddComponent(new VfxComponent() { RotateSpeed = 4f });
            entity.Enable();
        }

        public static void MakeEnemy(Scene scene, int type, float x, float y, float targetX, float targetY)
        {
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(x, y, 1, 0));
            if (type == 0) entity.AddComponent(new SpriteComponent(scene.GS, "s_fair"));
            if (type == 100) entity.AddComponent(new SpriteComponent(scene.GS, "s_glowy"));
            entity.AddComponent(new EnemyComponent(type, targetX, targetY));
            entity.AddComponent(new BodyComponent(x, y));
            entity.Enable();
        }

        public static void MakeSlash(Scene scene, int type, float x, float y)
        {
            float alpha = 0.5f;
            float scale = 0.5f;
            float lifespan = 0.4f;
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(x, y, 1, 0));
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
