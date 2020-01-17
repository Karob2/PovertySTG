using Engine;
using Engine.ECS;
using PovertySTG.ECS.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.Factories
{
    public class DanmakuFactory
    {
        public static void MakeBullet(GameServices gs, Scene scene, int type, float x, float y, float dx, float dy)
        {
            int layer = 1;
            string sprite = "projectiles";
            string animation = null;
            if (type < 0) layer = 100;
            if (type < 0) sprite = "pixel";
            if (type == 0) animation = "shard1";
            if (type == 1) animation = "diamond1";

            Entity entity = scene.NewEntity();
            entity.AddComponent(new RenderComponent(layer, 0));
            entity.AddComponent(new SpriteComponent(gs, sprite, animation));
            entity.AddComponent(new BulletComponent(type, x, y, dx, dy));
            entity.Enable();
        }
    }
}
