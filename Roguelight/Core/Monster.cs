using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelight.Behaviors;
using RogueSharp;

namespace Roguelight.Core
{
    public class Monster: Actor
    {
        public int? SecondsAlerted { get; set; }
        public int? CurrentTarget { get; set; }
        public bool bIsAtRest = true;

        public virtual void PerformAction(Player player, int nextStepX, int nextStepY, bool bIsInFov)
        {
            var behavior = new StandardMoveAndAttack();
            behavior.Act(this, player, nextStepX, nextStepY, bIsInFov);
        }
    }
}
