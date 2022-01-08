using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelight.Core;
using RogueSharp.DiceNotation;

namespace Roguelight.Monsters
{
    public class Skeleton : Monster
    {
        public static Skeleton Create(int level)
        {
            int health = Dice.Roll("2D5");
            return new Skeleton
            {
                Attack = Dice.Roll("1D3") + level / 3,
                AttackChance = Dice.Roll("25D3"),
                Awareness = 10,
                Color = Colors.KoboldColor,
                Defense = Dice.Roll("1D3") + level / 3,
                DefenseChance = Dice.Roll("10D4"),
                Gold = Dice.Roll("5D5"),
                Health = health,
                MaxHealth = health,
                Stamina = 100,
                MaxStamina = 100,
                Name = "Skeleton",
                Speed = 14,
                Symbol = 'S'
            };
        }
    }
}
