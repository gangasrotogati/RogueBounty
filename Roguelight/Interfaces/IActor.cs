using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelight.Interfaces
{
    public interface IActor
    {
        int MeleeAttackPower { get; set; }
        int MeleeAttackChance { get; set; }
        int Awareness { get; set; }
        int Defense { get; set; }
        int DefenseChance { get; set; }
        int Experience { get; set; }
        int Health { get; set; }
        int MaxHealth { get; set; }
        string Name { get; set; }
        int Speed { get; set; }
        int ZLevel { get; set; }
    }
}
