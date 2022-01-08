using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelight.Interfaces;
using RLNET;
using RogueSharp;

namespace Roguelight.Core
{
    public class Actor : IActor, IDrawable
    {
        // IActor
        private int _meleeAttackPower;
        private int _meleeAttackChance;
        private int _awareness;
        private int _defense;
        private int _defenseChance;
        private int _experience;
        private int _health;
        private int _maxHealth;
        private string _name;
        private int _speed;
        private int _stamina;
        private int _maxStamina;
        private int _mana;
        private int _maxMana;
        private int _inventoryID;
        private int _encumberance;
        private int _maxEncumberance;
        private int _experienceToLevel;
        private int _level;
        private int _xLevel;
        private int _yLevel;
        private int _zLevel;
        private string _soundAlerted;
        private ServerTimer Timer = new ServerTimer();

        public int MeleeAttackPower
        {
            get
            {
                return _meleeAttackPower;
            }
            set
            {
                _meleeAttackPower = value;
            }
        }

        public int MeleeAttackChance
        {
            get
            {
                return _meleeAttackChance;
            }
            set
            {
                _meleeAttackChance = value;
            }
        }

        public int Awareness
        {
            get
            {
                return _awareness;
            }
            set
            {
                _awareness = value;
            }
        }

        public int Defense
        {
            get
            {
                return _defense;
            }
            set
            {
                _defense = value;
            }
        }

        public int DefenseChance
        {
            get
            {
                return _defenseChance;
            }
            set
            {
                _defenseChance = value;
            }
        }

        public int Experience
        {
            get
            {
                return _experience;
            }
            set
            {
                _experience = value;
            }
        }

        public int Health
        {
            get
            {
                return _health;
            }
            set
            {
                _health = value;
            }
        }

        public int MaxHealth
        {
            get
            {
                return _maxHealth;
            }
            set
            {
                _maxHealth = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public int Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                _speed = value;
            }
        }

        public int Stamina
        {
            get
            {
                return _stamina;
            }
            set
            {
                _stamina = value;
            }
        }

        public int MaxStamina
        {
            get
            {
                return _maxStamina;
            }
            set
            {
                _maxStamina = value;
            }
        }
        public int XLevel
        {
            get
            {
                return _xLevel;
            }
            set
            {
                _xLevel = value;
            }
        }
        public int YLevel
        {
            get
            {
                return _yLevel;
            }
            set
            {
                _yLevel = value;
            }
        }

        public int ZLevel
        {
            get
            {
                return _zLevel;
            }
            set
            {
                _zLevel = value;
            }
        }
        public int InventoryId { get; set; }
        public int ActorID { get; set; }
        public int mapId { get; set; }

        // IDrawable
        public RLColor Color { get; set; }
        public int Symbol { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        //Targets list is {actor ID, threat level}
        public List<int[]> TargetsList = new List<int[]>();
        public void Draw(RLConsole console, IMap map)
        {
            // Don't draw actors in cells that haven't been explored
            if (!map.GetCell(X, Y).IsExplored)
            {
                return;
            }

            // Only draw the actor with the color and symbol when they are in field-of-view
            if (map.IsInFov(X, Y))
            {
                console.Set(X, Y, Color, Colors.FloorBackgroundFov, Symbol);
            }
            else
            {
                // When not in field-of-view just draw a normal floor
                console.Set(X, Y, Colors.Floor, Colors.FloorBackground, '.');
            }
        }

        public bool ActorMovementTimer()
        {
            bool canMove = false;
            long changeInTicks;
            this.Timer.now = DateTime.Now;
            changeInTicks = this.Timer.now.Ticks - this.Timer.lastMovement.Ticks;
            if(changeInTicks > 10000000 / this.Speed)
            {
                canMove = true;
                this.Timer.lastMovement = DateTime.Now;
            }
            return canMove;
        }
        public bool ActorSecondsTimer()
        {
            bool hasOneSecondPassed = false;
            long changeInTicks;
            this.Timer.now = DateTime.Now;
            changeInTicks = this.Timer.now.Ticks - this.Timer.previousTime.Ticks;
            if (changeInTicks > 10000000)
            {
                hasOneSecondPassed = true;
                this.Timer.previousTime = DateTime.Now;
            }
            return hasOneSecondPassed;
        }
        public int? GetNewTarget()
        {
            int? newTargetID = null;
            if (TargetsList.Count > 0)
            {
                int highestThreat = 0;
                foreach (int[] target in TargetsList)
                {
                    if (highestThreat < target[1])
                    {
                        highestThreat = target[1];
                        newTargetID = target[0];
                    }
                }
            }
            return newTargetID;
        }
        public static void DrawStats(RLConsole statConsole, int position, int health, int maxHealth, string name, string symbol)
        {
            // Start at Y=13 which is below the player stats.
            // Multiply the position by 2 to leave a space between each stat
            int yPosition = 7 + (position * 2);

            // Begin the line by printing the symbol of the monster in the appropriate color
            statConsole.Print(1, yPosition, symbol, Colors.Player);

            // Figure out the width of the health bar by dividing current health by max health
            int width = Convert.ToInt32(((double)health / (double)maxHealth) * 16.0);
            int remainingWidth = 16 - width;

            // Set the background colors of the health bar to show how damaged the monster is
            statConsole.SetBackColor(3, yPosition, width, 1, Swatch.Primary);
            statConsole.SetBackColor(3 + width, yPosition, remainingWidth, 1, Swatch.PrimaryDarkest);

            // Print the monsters name over top of the health bar
            statConsole.Print(2, yPosition, $": {name}", Swatch.DbLight);
        }
    }
}
