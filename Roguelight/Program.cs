using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelight.Core;

namespace Roguelight
{
    class Program
    {
        static void Main(string[] args)
        {
            string input;

            Console.Write("Would you like to host a server? (Y/N): ");
            input = Console.ReadLine().ToUpper();
            if(input == "Y")
            {
                //Start the server timer and create a randomized object for the server.
                Server.Timer = new ServerTimer();
                Console.Write("Would you like to load a previous save? (Y/N): ");
                input = Console.ReadLine().ToUpper();
                if(input == "Y")
                {
                    MapGenerator.IsServer = true;
                    Server.LoadGame();
                    foreach(Player player in Server.PlayerList)
                    {
                        player.isOnline = -2;
                    }
                    SocketListener.StartServer();
                }
                else
                {
                    //The map generator needs to know if we are running a server so that it will create monsters and treasure.
                    MapGenerator.IsServer = true;
                    MapGenerator mapGenerator = new MapGenerator(Client._mapWidth, Client._mapHeight, 400, 13, 7);
                    Server.random = new Random();
                    Server.randomSeed = Server.random.Next(-999999999, 999999999);
                    Server.random = new Random(Server.randomSeed);
                    Server.MapSeed = Server.GenerateSeed();
                    List<DungeonMap> mapList = mapGenerator.GenerateMapSection(Server.MapSeed, true, 0, 0, 0, 1);
                    DungeonMap dummyMap = new DungeonMap();
                    Server.MapList.Add(dummyMap);
                    foreach(DungeonMap map in mapList)
                    {
                        Server.MapList.Add(map);
                    }
                    SocketListener.StartServer();
                }
            }
            else
            {
                //The map generator will use the server map seed to generate levels which match the server levels
                MapGenerator.IsServer = false;
                MapGenerator mapGenerator = new MapGenerator(Client._mapWidth, Client._mapHeight, 400, 13, 7);

                Client.seed = SocketClient.RegisterPlayer();
                string[] chunks = Client.seed.Split('.');
                Client.playerSeed = chunks[0];
                Client.mapSeed = chunks[1];
                Client.inventoryId = chunks[2];
                Int32.TryParse(Client.mapSeed, out int seed);
                List<DungeonMap> mapList = mapGenerator.GenerateMapSection(seed, false, 0, 0, 0, 1);
                DungeonMap newMap = new DungeonMap();
                Engine.DungeonMaps.Add(newMap);
                foreach (DungeonMap map in mapList)
                {
                    Engine.DungeonMaps.Add(map);
                }
                Engine.DungeonMap = Engine.DungeonMaps[1];
                Console.Write("What is your name?: ");
                input = Console.ReadLine();
                SocketClient.SendData(input);
                Client.Run();
            }
        }
    }
}
