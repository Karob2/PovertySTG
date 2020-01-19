using System;
using System.Collections.Generic;
using System.Text;

namespace PovertySTG
{
    public enum LevelWaitMode
    {
        Start,
        None,
        Timer,
        Clear,
        Leave,
        Loop,
        Forever
    }

    public enum Alignment
    {
        Left,
        Center,
        Right
    }

    public enum Direction
    {
        Left,
        Right
    }

    public enum EnemyType
    {
        Fairy = 0,
        BraveFairy = 1,
        MoneyBag = 2,
        Boss = 100,
        Yoshika = 100,
        Fuyu = 101,
        Joon = 102,
        Shion = 103
    }

    public enum BulletType
    {
        BG = -1000,
        Items = -100,
        Coin = -100,
        Coin2 = -101,
        PointItem = -102,
        PowerItem = -103,
        PlayerShot = 0,
        EnemyShot = 1
    }
}
