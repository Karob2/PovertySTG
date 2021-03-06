﻿using Engine.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG.ECS.Components
{
    public class EnemyComponent : Component
    {
        public EnemyType Type { get; set; }
        public float TargetX { get; set; }
        public float TargetY { get; set; }
        public double Timer { get; set; }
        public double Timer2 { get; set; }
        public double MoneyTimer { get; set; }
        public int Phase { get; set; }
        public Direction Direction { get; set; }
        /*
        public float X { get; set; }
        public float Y { get; set; }
        public float DX { get; set; }
        public float DY { get; set; }
        */
        public float Health { get; set; } = 100f;
        public float MaxHealth { get; set; } = 100f;
        public int Lives { get; set; } = 1;

        public Entity Shield { get; set; }
        public float ShieldHealth { get; set; }

        public EnemyComponent() { }

        public EnemyComponent(EnemyType type, float targetX, float targetY)
        {
            Type = type;
            TargetX = targetX;
            TargetY = targetY;
        }

        /*
        public EnemyComponent(int type, float x, float y, float targetX, float targetY)
        {
            Type = type;
            X = x;
            Y = y;
            TargetX = targetX;
            TargetY = targetY;
        }
        */

        public override void AttachTo(Entity entity) { entity.AddComponent(this); }
        public override void Remove() { Owner.RemoveComponent(this); }
    }
}
