using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelight.Core;
using RogueSharp.DiceNotation;

namespace Roguelight.Monsters
{
    public class SkeletonMan
        : Monster
    {
        public static SkeletonMan Create(int level)
        {
            int health = Dice.Roll("2D5");
            Random random = new Random();
            return new SkeletonMan
            {
                MeleeAttackPower = Dice.Roll("1D3") + level / 3,
                MeleeAttackChance = Dice.Roll("25D3"),
                Awareness = 15,
                Color = Colors.KoboldColor,
                Defense = Dice.Roll("1D3") + level / 3,
                DefenseChance = Dice.Roll("10D4"),
                Experience = Dice.Roll("5D5"),
                Health = health,
                MaxHealth = health,
                Name = "Skeleton",
                Speed = 3,
                Symbol = 13
            };
        }
    }
}
