using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;

namespace Roguelight.Core
{
    public class Player : Actor
    {
        public int isOnline { get; set; }
        public int? SecondsAlerted { get; set; }
        public int? CurrentTarget { get; set; }
        public List<int> equipmentList { get; set; }
        public Player()
        {
            MeleeAttackPower = 2;
            MeleeAttackChance = 50;
            Awareness = 130;
            Color = Colors.Player;
            Defense = 2;
            DefenseChance = 40;
            Experience = 0;
            Health = 100;
            MaxHealth = 100;
            Speed = 10;
            Name = "Rogue";
            Color = Colors.Player;
            Symbol = 5;
            X = 10;
            Y = 10;
            Stamina = 100;
            MaxStamina = 100;
            isOnline = 1;
            XLevel = 0;
            YLevel = 0;
            ZLevel = 0;
            mapId = 1;
            equipmentList = new List<int>();
        }
        public static void DrawStats(RLConsole statConsole, Player player)
        {
            statConsole.Print(1, 1, $"Name:    {player.Name}", Colors.Text);
            statConsole.Print(1, 3, $"Health:  {player.Health}/{player.MaxHealth}", Colors.Text);
            statConsole.Print(1, 5, $"Stamina: {player.Stamina}/{player.MaxStamina}", Colors.Text);
            statConsole.Print(1, 7, $"X:{player.XLevel} Y:{player.YLevel} Z:{player.ZLevel}", Colors.Text);
        }
    }
}
