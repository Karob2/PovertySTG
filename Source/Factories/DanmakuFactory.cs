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
            SpriteComponent sc = new SpriteComponent(scene.GS, "sanny");
            /*
            Frame frame = sc.Sprite.DefaultAnimationObject.Frames[0];
            sc.Width = frame.Width / 2;
            sc.Height = frame.Height / 2;
            sc.Stretched = true;
            */
            entity.AddComponent(sc);
            entity.AddComponent(new PlayerComponent(x, y));
            entity.Enable();
        }

        public static void MakeBullet(Scene scene, int type, float x, float y, float dx, float dy)
        {
            int layer = 1;
            string sprite = "shot";
            string animation = null;
            if (type < 0) layer = 100;
            if (type < 0) sprite = "pixel";
            if (type == 0) animation = "shard1";
            if (type == 1) animation = "diamond1";

            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(layer, 0));
            entity.AddComponent(new SpriteComponent(scene.GS, sprite, animation));
            entity.AddComponent(new BulletComponent(type, x, y, dx, dy));
            entity.Enable();
        }

        public static void MakeEnemy(Scene scene, int type, float x, float y, float targetX, float targetY)
        {
            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(1, 0));
            entity.AddComponent(new SpriteComponent(scene.GS, "fair"));
            entity.AddComponent(new EnemyComponent(type, x, y, targetX, targetY));
            entity.Enable();
        }
    }
}
