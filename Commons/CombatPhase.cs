using System;
using System.Collections.Generic;
using System.Text;

namespace Yuzuri.Commons
{
    public enum CombatPhase
    {
        None = 0,
        PreOffTurn = 1,
        OffTurn = 2,
        AttackTurn = 3,
        OffAfterTurn = 4,
        AfterTurn = 5,
        PostTurn = 6,
        End = -1
    }
}
