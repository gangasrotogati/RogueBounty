using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelight.Core;
using RLNET;
using Roguelight.Systems;

namespace Roguelight.Core
{
    public class Client
    {
        public static RLRootConsole rootConsole;
        public static Engine engine;
        public const int screenWidth = 96;//100
        public const int screenHeight = 36;//75
        public static string seed;
        public static string playerSeed;
        public static string mapSeed;
        public static string inventoryId;

        public static readonly int _mapWidth = 76;//80
        public static readonly int _mapHeight = 20;//48
        public static RLConsole _mapConsole;
        public static readonly int _messageWidth = 70;//80
        public static readonly int _messageHeight = 8;//11
        public static RLConsole _messageConsole;
        public static readonly int _statWidth = 20;//20
        public static readonly int _statHeight = 36;//70
        public static RLConsole _statConsole;
        public static readonly int _inventoryWidth = 70;//80
        public static readonly int _inventoryHeight = 8;//11
        public static RLConsole _inventoryConsole;
        public static readonly int _characterSheetWidth = 70;//80
        public static readonly int _characterSheetHeight = 8;//11
        public static RLConsole _characterSheetConsole;
        public static readonly int _equipmentWidth = 70;//80
        public static readonly int _equipmentHeight = 8;//11
        public static RLConsole _equipmentConsole;
        //loot and abilities matches the dimensions of the inventory console so no need to declare dimensions for these right now.
        public static RLConsole _lootConsole;
        public static RLConsole _abilitiesConsole;
        public static CommandSystem CommandSystem { get; set; }

        public static List<Player> PlayerList = new List<Player>();
        public static List<Monster> MonsterList = new List<Monster>();
        public static List<Item> MapItemList = new List<Item>();

        public static void Run()
        {
            //rootConsole = new RLRootConsole("ascii_8x8.png", screenWidth, screenHeight, 8, 8);
            //rootConsole = new RLRootConsole("ascii_10x10.png", screenWidth, screenHeight, 10, 10);
            //rootConsole = new RLRootConsole("ascii_20x20.png", screenWidth, screenHeight, 20, 20);
            //rootConsole = new RLRootConsole("ascii_16x16.png", screenWidth, screenHeight, 16, 16);
            rootConsole = new RLRootConsole("ascii_16x24.png", screenWidth, screenHeight, 16, 24);
            _inventoryConsole = new RLConsole(_inventoryWidth, _inventoryHeight);
            _equipmentConsole = new RLConsole(_equipmentWidth, _equipmentHeight);
            _characterSheetConsole = new RLConsole(_characterSheetWidth, _characterSheetHeight);
            _lootConsole = new RLConsole(_inventoryWidth, _inventoryHeight);
            engine = new Engine(rootConsole);
            _abilitiesConsole = new RLConsole(_inventoryWidth, _inventoryHeight);
            _mapConsole = new RLConsole(_mapWidth, _mapHeight);
            _messageConsole = new RLConsole(_messageWidth, _messageHeight);
            _statConsole = new RLConsole(_statWidth, _statHeight);
            CommandSystem = new CommandSystem();

            rootConsole.Run();
        }
    }
}
