using Engine;
using Engine.ECS;
using Engine.Resource;
using PovertySTG.ECS.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Systems
{
    public class SystemReferences
    {
        GameServices gs;
        Scene scene;

        public ComponentGroup<CameraComponent> CameraComponents { get; private set; }
        public ComponentGroup<PointerComponent> PointerComponents { get; private set; }
        public ComponentGroup<RenderComponent> RenderComponents { get; private set; }
        public ComponentGroup<SpriteComponent> SpriteComponents { get; private set; }
        public ComponentGroup<TextComponent> TextComponents { get; private set; }
        public ComponentGroup<ButtonComponent> ButtonComponents { get; private set; }
        public ComponentGroup<PlayerComponent> PlayerComponents { get; private set; }
        public ComponentGroup<EnemyComponent> EnemyComponents { get; private set; }
        public ComponentGroup<BulletComponent> BulletComponents { get; private set; }

        public Font GlobalFont { get; private set; }

        public SystemReferences(GameServices gs, Scene scene)
        {
            this.gs = gs;
            this.scene = scene;
            CameraComponents = scene.GetComponentGroup<CameraComponent>();
            PointerComponents = scene.GetComponentGroup<PointerComponent>();
            RenderComponents = scene.GetComponentGroup<RenderComponent>();
            SpriteComponents = scene.GetComponentGroup<SpriteComponent>();
            TextComponents = scene.GetComponentGroup<TextComponent>();
            ButtonComponents = scene.GetComponentGroup<ButtonComponent>();
            PlayerComponents = scene.GetComponentGroup<PlayerComponent>();
            EnemyComponents = scene.GetComponentGroup<EnemyComponent>();
            BulletComponents = scene.GetComponentGroup<BulletComponent>();

            GlobalFont = gs.ResourceManager.Fonts.Get("pcsenior");
        }

        public GameStateComponent GetGameState()
        {
            if (!gs.RootScene.GetComponentGroup<GameStateComponent>().TryGetFirstEnabled(out GameStateComponent gsc)) return null;
            return gsc;
        }
    }
}
