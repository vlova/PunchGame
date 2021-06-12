﻿using System;

namespace PunchGame.Server.Room.Core.Configs
{
    public class PunchConfig
    {
        public int Damage { get; set; }

        public int CriticalDamage { get; set; }

        public decimal CriticalChance { get; set; }

        public TimeSpan MinimalTimeDiff { get; set; }
    }
}
