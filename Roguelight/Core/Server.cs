using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using RogueSharp;
using RogueSharp.DiceNotation;
using Roguelight.Items;
using System.Text.Json;
using System.IO;

namespace Roguelight.Core
{
    public class Server
    {
        public static List<Player> PlayerList = new List<Player>();
        public static List<DungeonMap> MapList = new List<DungeonMap>();
        public static List<DungeonMap> MapGenerationData = new List<DungeonMap>();
        public static List<Inventory> InventoryList = new List<Inventory>();
        public static List<Item> ItemList = new List<Item>();
        public static ServerTimer Timer;
        public static Player thisPlayer;
        public static Random random;
        public static int randomSeed;
        public static string serverMessage;
        public static int MapSeed { get; set; }

        public static int GenerateSeed()
        {
            int seed = (int)DateTime.UtcNow.Ticks;
            return seed;
        }

        public static int GenerateRandomID()
        {
            int id = random.Next(0, 999999999);
            return id;
        }
        public static string RegisterPlayer()
        {
            //generate a new inventory object for this player
            Inventory inventory = new Inventory();
            int id;
            int seed;
            Random randomizeItem = new Random();
            InventoryList.Add(inventory);
            for(int i = 0; i < 14; i++)
            {
                id = randomizeItem.Next(-999999999, 999999999);
                seed = randomizeItem.Next(-999999999, 999999999);
                Item item1 = ItemGenerator.GenerateItem(id, seed);
                item1.XLevel = -1;
                item1.YLevel = -1;
                item1.ZLevel = -1;
                Server.ItemList.Add(item1);
                InventoryList[InventoryList.Count - 1].itemsList.Add(item1.itemId);
            }

            Player PlayerData = new Player(); //playerID, x, y, isOnline
            PlayerData.ActorID = PlayerList.Count;
            PlayerData.X = MapList[1].SpawnX;
            PlayerData.Y = MapList[1].SpawnY;
            PlayerData.InventoryId = inventory.inventoryID;
            PlayerList.Add(PlayerData);
            return PlayerData.ActorID.ToString() + "." + MapSeed.ToString() + "." + PlayerData.InventoryId.ToString(); //Tell the client its player ID so that it can use it to register commands. Also tell it the map seed so that it can generate dungeon maps. And its inventory ID to access its inventory.
        }
        public static void RegisterAction(string data)
        {
            string[] chunks = data.Split('.');
            Int32.TryParse(chunks[0], out int playerID); // Look at the Player ID
            foreach (Player playerData in Server.PlayerList)
            {
                if (playerData.ActorID == playerID) //If the PlayerID matches a player in the list, then edit their position according to the key input.
                {
                    switch (chunks[1]){
                        case "UP": { MovePlayer(playerData, 0, -1); break; }
                        case "LEFT": { MovePlayer(playerData, -1, 0); break; }
                        case "RIGHT": { MovePlayer(playerData, 1, 0); break; }
                        case "DOWN": { MovePlayer(playerData, 0, 1); break; }
                        case "UPLEFT": { MovePlayer(playerData, -1, -1); break; }
                        case "UPRIGHT": { MovePlayer(playerData, 1, -1); break; }
                        case "DOWNLEFT": { MovePlayer(playerData, -1, 1); break; }
                        case "DOWNRIGHT": { MovePlayer(playerData, 1, 1); break; }
                        case "STAIRSDOWN": { MoveDownStairs(playerData);  break; }
                        case "STAIRSUP": { MoveUpStairs(playerData); break; }
                    }
                }
            }
        }

        public static string UpdateClient(string data)
        {
            string response = "";
            string[] chunks = data.Split(',');
            int actorId = Int32.Parse(chunks[0]);
            int mapId = PlayerList[actorId].mapId;

            PlayerList[actorId].isOnline = 1;
            thisPlayer = PlayerList[actorId];

            foreach (Player player in PlayerList)
            {
                if(player.mapId == mapId)
                {
                    response += $"{player.ActorID}.{player.X}.{player.Y}.{player.isOnline}.{player.Health}.{player.MaxHealth}.{player.Symbol}.{player.Name}.player.{player.XLevel}.{player.YLevel}.{player.ZLevel}.{player.mapId},";
                    //0.ID 1.X 2.Y 3.isOnline 4.Health 5.MaxHealth 6.symbol 7.name 8.player 9.XLevel 10.YLevel 11.ZLevel 12.MapId
                }
            }
            foreach (Monster monster in MapList[mapId]._monsters)
            {
                monster.mapId = mapId;
                response += $"{monster.ActorID}.{monster.X}.{monster.Y}.{monster.Health}.{monster.MaxHealth}.{monster.Symbol}.{monster.Name}.{monster.Awareness}.monster.{monster.XLevel}.{monster.YLevel}.{monster.ZLevel}.{monster.mapId}.{monster.Speed}.{monster.bIsAtRest},";
                //0.ID 1.X 2.Y 3.Health 4.MaxHealth 5.symbol 6.name 7.awareness 8.monster 9.XLevel 10.YLevel 11.ZLevel 12.MapId 13.Speed 14.bIsAtRest
            }
            foreach (Item item in ItemList)
            {
                if(item.mapId == mapId)
                {
                    response += $"{item.itemId}.{item.itemSeed}.{item.X}.{item.Y}.{item.mapId}.item,";
                }
            }
            //chunks[0] is the player ID chunks[1] is monsterIDxXpathxYpathxFov.
            UpdateMonstersFov(chunks[0], chunks[1]);
            response += serverMessage;
            response += ",<EOF>";
            serverMessage = null;
            return response;
        }

        public static string UpdateDungeonMap(string data)
        {
            string[] chunks = data.Split(',');
            Int32.TryParse(chunks[0], out int mapId);
            return MapList[mapId].Seed.ToString() + "." + $"{MapList[mapId].ZOrigin}.{MapList[mapId].XOrigin}.{MapList[mapId].YOrigin}.{MapList[mapId].SectionId}";
        }

        private static void CrossOverMapEdge( Player PlayerData, int dx, int dy)
        {
            bool bIsMapFound = true;
            bool bIsMapGenDataFound = true;
            int mapId = -999;
            MapList[PlayerData.mapId].SetCellProperties(PlayerData.X, PlayerData.Y, false, true); // Set the previous position to walkable
            int i = 0;
            if (PlayerData.X + dx == -1 && PlayerData.Y + dy == -1)
            {
                PlayerData.YLevel -= 1;
                PlayerData.XLevel -= 1;
            }
            else if (PlayerData.X + dx == MapList[PlayerData.mapId].Width && PlayerData.Y + dy == -1)
            {
                PlayerData.YLevel -= 1;
                PlayerData.XLevel += 1;
            }
            else if(PlayerData.X + dx == MapList[PlayerData.mapId].Width && PlayerData.Y + dy == MapList[PlayerData.mapId].Height)
            {
                PlayerData.YLevel += 1;
                PlayerData.XLevel += 1;
            }
            else if(PlayerData.X + dx == -1 && PlayerData.Y + dy == MapList[PlayerData.mapId].Height)
            {
                PlayerData.YLevel += 1;
                PlayerData.XLevel -= 1;
            }
            else if(PlayerData.X + dx == -1)
            {
                PlayerData.XLevel -= 1;
            }
            else if(PlayerData.X + dx == MapList[PlayerData.mapId].Width)
            {
                PlayerData.XLevel += 1;
            }
            else if(PlayerData.Y + dy == -1)
            {
                PlayerData.YLevel -= 1;
            }
            else if(PlayerData.Y + dy == MapList[PlayerData.mapId].Height)
            {
                PlayerData.YLevel += 1;
            }

            while (MapList[i].ZLevel != PlayerData.ZLevel || MapList[i].XLevel != PlayerData.XLevel || MapList[i].YLevel != PlayerData.YLevel)
            {
                if (i == MapList.Count - 1 && (MapList[i].ZLevel != PlayerData.ZLevel || MapList[i].XLevel != PlayerData.XLevel || MapList[i].YLevel != PlayerData.YLevel))
                {
                    bIsMapFound = false;
                    break;
                }
                i++;
            }
            if (bIsMapFound == false)
            {
                if(MapGenerationData.Count > 0)
                {
                    int j = 0;
                    while (MapGenerationData[j].ZLevel != PlayerData.ZLevel || MapGenerationData[j].XLevel != PlayerData.XLevel || MapGenerationData[j].YLevel != PlayerData.YLevel)
                    {
                        if (j == MapGenerationData.Count - 1 && (MapGenerationData[j].ZLevel != PlayerData.ZLevel || MapGenerationData[j].XLevel != PlayerData.XLevel || MapGenerationData[j].YLevel != PlayerData.YLevel))
                        {
                            bIsMapGenDataFound = false;
                            break;
                        }
                        j++;
                    }
                    if (bIsMapGenDataFound)
                    {
                        mapId = MapGenerationData[j].MapId;
                        LoadMapData(MapGenerationData[j]);
                    }
                    else
                    {
                        mapId = CreateMapSection(PlayerData, PlayerData.ZLevel);
                    }
                }
                else
                {
                    mapId = CreateMapSection(PlayerData, PlayerData.ZLevel);
                }
            }
            else
            {
                mapId = i;
            }

            if(PlayerData.X + dx == -1 && PlayerData.Y + dy == -1)
            {
                if (MapList[mapId].IsWalkable(MapList[mapId].Width - 1, MapList[mapId].Height - 1))
                {
                    PlayerData.X = MapList[PlayerData.mapId].Width - 1;
                    PlayerData.Y = MapList[PlayerData.mapId].Height - 1;
                    PlayerData.mapId = mapId;
                }
                else
                {
                    PlayerData.YLevel += 1;
                    PlayerData.XLevel += 1;
                }
            }
            else if (PlayerData.X + dx == MapList[PlayerData.mapId].Width && PlayerData.Y + dy == -1)
            {
                if (MapList[mapId].IsWalkable(0, MapList[mapId].Height - 1))
                {
                    PlayerData.X = 0;
                    PlayerData.Y = MapList[PlayerData.mapId].Height - 1;
                    PlayerData.mapId = mapId;
                }
                else
                {
                    PlayerData.YLevel += 1;
                    PlayerData.XLevel -= 1;
                }
            }
            else if (PlayerData.X + dx == MapList[PlayerData.mapId].Width && PlayerData.Y + dy == MapList[PlayerData.mapId].Height)
            {
                if (MapList[mapId].IsWalkable(0, 0))
                {
                    PlayerData.X = 0;
                    PlayerData.Y = 0;
                    PlayerData.mapId = mapId;
                }
                else
                {
                    PlayerData.YLevel -= 1;
                    PlayerData.XLevel -= 1;
                }
            }
            else if (PlayerData.X + dx == -1 && PlayerData.Y + dy == MapList[PlayerData.mapId].Height)
            {
                if (MapList[mapId].IsWalkable(MapList[mapId].Width - 1, 0))
                {
                    PlayerData.X = MapList[PlayerData.mapId].Width - 1;
                    PlayerData.Y = 0;
                    PlayerData.mapId = mapId;
                }
                else
                {
                    PlayerData.YLevel -= 1;
                    PlayerData.XLevel += 1;
                }
            }
            else if (PlayerData.X + dx == -1)
            {
                if (MapList[mapId].IsWalkable(MapList[mapId].Width - 1, PlayerData.Y + dy))
                {
                    PlayerData.X = MapList[mapId].Width - 1;
                    PlayerData.Y += dy;
                    PlayerData.mapId = mapId;
                }
                else
                {
                    PlayerData.XLevel += 1;
                }
            }
            else if (PlayerData.X + dx == MapList[mapId].Width)
            {
                if (MapList[mapId].IsWalkable(0, PlayerData.Y + dy))
                {
                    PlayerData.X = 0;
                    PlayerData.Y += dy;
                    PlayerData.mapId = mapId;
                }
                else
                {
                    PlayerData.XLevel -= 1;
                }
            }
            else if (PlayerData.Y + dy == -1)
            {
                if (MapList[mapId].IsWalkable(PlayerData.X + dx, MapList[mapId].Height - 1))
                {
                    PlayerData.Y = MapList[mapId].Height - 1;
                    PlayerData.X += dx;
                    PlayerData.mapId = mapId;
                }
                else
                {
                    PlayerData.YLevel += 1;
                }
            }
            else if (PlayerData.Y + dy == MapList[mapId].Height)
            {
                if (MapList[mapId].IsWalkable(PlayerData.X + dx, 0))
                {
                    PlayerData.Y = 0;
                    PlayerData.X += dx;
                    PlayerData.mapId = mapId;
                }
                else
                {
                    PlayerData.YLevel -= 1;
                }
            }
            MapList[PlayerData.mapId].SetCellProperties(PlayerData.X, PlayerData.Y, false, false); //Set the new position to unwalkable
        }

        public static void MovePlayer(Player PlayerData, int dx, int dy)
        {
            Monster monster = MapList[PlayerData.mapId].GetMonsterAt(PlayerData.X + dx, PlayerData.Y + dy);

            if (PlayerData.ActorMovementTimer())
            {
                if (monster != null)
                {
                    Attack(PlayerData, monster);
                }
                else if (PlayerData.Y + dy == -1 || PlayerData.Y + dy == MapList[PlayerData.mapId].Height || PlayerData.X + dx == -1 || PlayerData.X + dx == MapList[PlayerData.mapId].Width)
                {
                    CrossOverMapEdge(PlayerData, dx, dy);
                }
                else if (MapList[PlayerData.mapId].IsWalkable(PlayerData.X + dx, PlayerData.Y + dy) && PlayerData.Stamina > 0)
                {
                    MapList[PlayerData.mapId].SetCellProperties(PlayerData.X, PlayerData.Y, false, true); // Set the previous position to walkable
                    PlayerData.X += dx;
                    PlayerData.Y += dy;
                    MapList[PlayerData.mapId].SetCellProperties(PlayerData.X, PlayerData.Y, false, false); //Set the new position to unwalkable
                }
            }
        }

        public static void MoveMonster(Monster monster, int nextStepX, int nextStepY, bool bIsInFov)
        {
            int dx = nextStepX - monster.X;
            int dy = nextStepY - monster.Y;
            bool isPlayerDetected = false;
            bool isMonsterDetected = false;

            //Check if a second has gone by, and if so, increase the time the monster has been alerted by one second.
            if (monster.ActorSecondsTimer())
            {
                monster.SecondsAlerted++;
            }
            //Check if a player is in the way and if so, attack it.
            if (monster.ActorMovementTimer())
            {
                foreach (Player playerData in PlayerList)
                {
                    if (playerData.X == nextStepX && playerData.Y == nextStepY)
                    {
                        Attack(monster, playerData);
                        isPlayerDetected = true;
                    }
                }
                if(isPlayerDetected == false)
                {
                    foreach (Monster otherMonster in MapList[monster.mapId]._monsters)
                    {
                        if (otherMonster.X == nextStepX && otherMonster.Y == nextStepY)
                        {
                            isMonsterDetected = true;
                        }
                    }
                    if (isPlayerDetected == false && isMonsterDetected == false)
                    {
                        MapList[monster.mapId].SetCellProperties(monster.X, monster.Y, false, true); // Set the previous position to walkable
                        monster.X += dx;
                        monster.Y += dy;
                        MapList[monster.mapId].SetCellProperties(monster.X, monster.Y, false, false); //Set the new position to unwalkable
                    }
                }
            }
        }

        public static void MoveDownStairs(Player PlayerData)
        {
            bool bIsMapFound = true;
            if (MapList[PlayerData.mapId].CanMoveDownToNextLevel(PlayerData))
            {
                MapList[PlayerData.mapId].SetCellProperties(PlayerData.X, PlayerData.Y, false, true);
                int z = PlayerData.ZLevel + 1;
                int i = 0;
                while(MapList[i].ZLevel != z || MapList[i].XLevel != PlayerData.XLevel || MapList[i].YLevel != PlayerData.YLevel)
                {
                    if(i == MapList.Count - 1 && (MapList[i].ZLevel != z || MapList[i].XLevel != PlayerData.XLevel || MapList[i].YLevel != PlayerData.YLevel))
                    {
                        bIsMapFound = false;
                        break;
                    }
                    i++;
                }
                if (bIsMapFound == false)
                {
                    PlayerData.mapId = CreateMapSection(PlayerData, z);
                }
                else
                {
                    PlayerData.mapId = i;
                }
                PlayerData.X = MapList[PlayerData.mapId].StairsUp.X;
                PlayerData.Y = MapList[PlayerData.mapId].StairsUp.Y;
                PlayerData.ZLevel = PlayerData.ZLevel + 1;
                MapList[PlayerData.mapId].SetCellProperties(PlayerData.X, PlayerData.Y, false, false);
            }
        }
        public static void MoveUpStairs(Player PlayerData)
        {
            bool bIsMapFound = true;
            if (MapList[PlayerData.mapId].CanMoveUpToNextLevel(PlayerData))
            {
                MapList[PlayerData.mapId].SetCellProperties(PlayerData.X, PlayerData.Y, false, true);
                int z = PlayerData.ZLevel - 1;
                int i = 0;
                while (MapList[i].ZLevel != z || MapList[i].XLevel != PlayerData.XLevel || MapList[i].YLevel != PlayerData.YLevel)
                {
                    if (i == MapList.Count - 1 && (MapList[i].ZLevel != z || MapList[i].XLevel != PlayerData.XLevel || MapList[i].YLevel != PlayerData.YLevel))
                    {
                        bIsMapFound = false;
                        break;
                    }
                    i++;
                }
                if (bIsMapFound == false)
                {
                    PlayerData.mapId = CreateMapSection(PlayerData, z);
                }
                else
                {
                    PlayerData.mapId = i;
                }
                PlayerData.mapId = i;
                PlayerData.X = MapList[i].StairsDown.X;
                PlayerData.Y = MapList[i].StairsDown.Y;
                PlayerData.ZLevel = PlayerData.ZLevel - 1;
                MapList[i].SetCellProperties(PlayerData.X, PlayerData.Y, false, false);
            }
        }
        public static void Attack(Actor attacker, Actor defender)
        {
            StringBuilder attackMessage = new StringBuilder();
            StringBuilder defenseMessage = new StringBuilder();

            int hits = ResolveAttack(attacker, defender, attackMessage);

            int blocks = ResolveDefense(defender, hits, attackMessage, defenseMessage);

            if(thisPlayer.ActorID == attacker.ActorID)
            {
                serverMessage += $"The {attacker.Name} hits {defender.Name} for {hits} damage!_1.";
            }
            else
            {
                serverMessage += $"The {attacker.Name} hits {defender.Name} for {hits} damage!_2.";
            }
            
            if (!string.IsNullOrWhiteSpace(defenseMessage.ToString()))
            {
                if (thisPlayer.ActorID == defender.ActorID)
                {
                    serverMessage += $"{defender.Name} blocks {blocks} damage!_3.";
                }
                else
                {
                    serverMessage += $"{defender.Name} blocks {blocks} damage!_4.";
                }
            }

            int damage = hits - blocks;

            ResolveDamage(defender, damage);
            //Add threat
            if (defender.TargetsList.Count > 0)
            {
                foreach (int[] target in defender.TargetsList)
                {
                    if (target[0] == attacker.ActorID)
                    {
                        target[1] += damage;
                    }
                }
            }
        }

        // The attacker rolls based on his stats to see if he gets any hits
        private static int ResolveAttack(Actor attacker, Actor defender, StringBuilder attackMessage)
        {
            int hits = 0;

            attackMessage.AppendFormat("{0} attacks {1} and rolls: ", attacker.Name, defender.Name);

            // Roll a number of 100-sided dice equal to the Attack value of the attacking actor
            DiceExpression attackDice = new DiceExpression().Dice(attacker.MeleeAttackPower, 100);
            DiceResult attackResult = attackDice.Roll();

            // Look at the face value of each single die that was rolled
            foreach (TermResult termResult in attackResult.Results)
            {
                attackMessage.Append(termResult.Value + ", ");
                // Compare the value to 100 minus the attack chance and add a hit if it's greater
                if (termResult.Value >= 100 - attacker.MeleeAttackChance)
                {
                    hits++;
                }
            }

            return hits;
        }

        // The defender rolls based on his stats to see if he blocks any of the hits from the attacker
        private static int ResolveDefense(Actor defender, int hits, StringBuilder attackMessage, StringBuilder defenseMessage)
        {
            int blocks = 0;

            if (hits > 0)
            {
                attackMessage.AppendFormat("scoring {0} hits.", hits);
                defenseMessage.AppendFormat("  {0} defends and rolls: ", defender.Name);

                // Roll a number of 100-sided dice equal to the Defense value of the defendering actor
                DiceExpression defenseDice = new DiceExpression().Dice(defender.Defense, 100);
                DiceResult defenseRoll = defenseDice.Roll();

                // Look at the face value of each single die that was rolled
                foreach (TermResult termResult in defenseRoll.Results)
                {
                    defenseMessage.Append(termResult.Value + ", ");
                    // Compare the value to 100 minus the defense chance and add a block if it's greater
                    if (termResult.Value >= 100 - defender.DefenseChance)
                    {
                        blocks++;
                    }
                }
                defenseMessage.AppendFormat("resulting in {0} blocks.", blocks);
            }
            else
            {
                attackMessage.Append("and misses completely.");
            }

            return blocks;
        }

        // Apply any damage that wasn't blocked to the defender
        private static void ResolveDamage(Actor defender, int damage)
        {
            if (damage > 0)
            {
                defender.Health = defender.Health - damage;

                if (defender.Health <= 0)
                {
                    ResolveDeath(defender);
                }
            }
            else
            {
            }
        }

        // Remove the defender from the map and add some messages upon death.
        private static void ResolveDeath(Actor defender)
        {
            if (defender is Player)
            {
            }
            else if (defender is Monster)
            {
                MapList[defender.mapId].RemoveMonster((Monster)defender);
            }
        }

        public static void UpdateMonstersFov(string id, string monsterdata)
        {
            string[] chunks = monsterdata.Split('.');
            int monsterId = 0;
            int nextStepX = 0;
            int nextStepY = 0;
            bool bIsInFov;
            
            for(int i = 0; i < chunks.Length - 1; i++)
            {
                //reset the field of view
                bIsInFov = false;
                //make sure there is actually a fov update
                if (chunks.Length > 1)
                {
                    string[] subChunks = chunks[i].Split('x');
                    Int32.TryParse(subChunks[0], out monsterId);
                    if (id == monsterdata)
                    {
                        return;
                    }
                    Int32.TryParse(subChunks[1], out nextStepX);
                    Int32.TryParse(subChunks[2], out nextStepY);
                    if(subChunks[3] == "1")
                    {
                        bIsInFov = true;
                    }
                }
                Int32.TryParse(id, out int playerID);
                foreach (Player player in PlayerList)
                {
                    //Check for monsters that are targeting disconnected players
                    if(player.isOnline == -1)
                    {
                        foreach(Monster monster in MapList[player.mapId]._monsters)
                        {
                            if(monster.CurrentTarget == player.ActorID)
                            {
                                monster.CurrentTarget = null;
                            }
                        }
                    }
                    if (player.ActorID == playerID)
                    {
                        foreach (Monster monster in MapList[player.mapId]._monsters)
                        {
                            if (monster.ActorID == monsterId)
                            {
                                monster.PerformAction(player, nextStepX, nextStepY, bIsInFov);
                            }
                        }
                    }
                }
            }
        }
        public static string GetInventoryItems(string data)
        {
            string[] chunks = data.Split(',');
            int.TryParse(chunks[0], out int playerInventoryId);
            string response = "";
            Inventory playerInventory = new Inventory();
            foreach(Inventory inventory in InventoryList)
            {
                if(inventory.inventoryID == playerInventoryId)
                {
                    playerInventory = inventory;
                }
            }
            foreach(int itemId in playerInventory.itemsList)
            {
                foreach(Item item in ItemList)
                {
                    if(item.itemId == itemId)
                    {
                        response += $"{item.itemId}.{item.itemSeed},";
                    }
                    // 0.id 1.name 2.category 3.material 4.quality 5.weight 6.value 7.symbol
                }
            }
            return response;
        }
        public static void DropItem(string data)
        {
            string[] chunks = data.Split(',');
            string[] subchunks = chunks[0].Split('.');
            int.TryParse(subchunks[0], out int playerId);
            int playerX = 0;
            int playerY = 0;
            int playerZLevel = 0;
            int playerYLevel = 0;
            int playerXLevel = 0;
            int playerMapId = 0;
            int playerInventory = 0;
            int playerItemId = int.Parse(subchunks[1]);
            Item dummyItem = new Item();
            //search for the player
            foreach(Player player in PlayerList)
            {
                if(player.ActorID == playerId)
                {
                    //when the player is found copy their data that we need to place the item
                    playerX = player.X;
                    playerY = player.Y;
                    playerZLevel = player.ZLevel;
                    playerXLevel = player.XLevel;
                    playerYLevel = player.YLevel;
                    playerMapId = player.mapId;
                    playerInventory = player.InventoryId;
                }
            }
            //Find and remove the designated item from the player's inventory
            List<int> itemList = new List<int>();
            foreach (Inventory inventory in InventoryList)
            {
                if (inventory.inventoryID == playerInventory)
                {
                    foreach (int itemId in inventory.itemsList)
                    {
                        if (itemId != playerItemId)
                        {
                            itemList.Add(itemId);
                        }
                    }
                    inventory.itemsList = itemList;
                }
            }

            foreach(Item item in ItemList)
            {
                if(item.itemId == playerItemId)
                {
                    item.X = playerX;
                    item.Y = playerY;
                    item.ZLevel = playerZLevel;
                    item.XLevel = playerXLevel;
                    item.YLevel = playerYLevel;
                    item.mapId = playerMapId;
                }
            }
        }

        public static void GiveItem(string data)
        {
            string[] chunks = data.Split(',');
            string[] subchunks = chunks[0].Split('.');
            int.TryParse(subchunks[0], out int playerId);
            int playerInventory = 0;
            int playerItemId = int.Parse(subchunks[1]);
            Item dummyItem = new Item();
            //search for the player
            foreach (Player player in PlayerList)
            {
                if (player.ActorID == playerId)
                {
                    //when the player is found copy their data that we need to place the item
                    playerInventory = player.InventoryId;
                }
            }
            List<int> itemList = new List<int>();
            foreach (Inventory inventory in InventoryList)
            {
                if (inventory.inventoryID == playerInventory)
                {
                    foreach (int itemId in inventory.itemsList)
                    {
                        itemList.Add(itemId);
                    }
                    itemList.Add(playerItemId);
                    inventory.itemsList = itemList;
                }
            }
            foreach (Item item in ItemList)
            {
                if (item.itemId == playerItemId)
                {
                    item.mapId = null;
                }
            }
        }

        public static void EquiptItem(string data)
        {
            string[] chunks = data.Split(',');
            string[] subchunks = chunks[0].Split('.');
            int.TryParse(subchunks[0], out int playerId);
            int.TryParse(subchunks[1], out int ItemId);
            int playerInventory = 0;
            List<int> itemList = new List<int>();
            Item newItem = new Item();

            int i = 0;
            while(ItemList[i].itemId != ItemId)
            {
                i++;
            }
            //store the properties of the item to be equipted into newItem to compare with other equipment in the next block of code.
            newItem = ItemList[i];
            //Make sure this isn't a consumable item.
            if(newItem.itemCategory == "consumable")
            {
                return;
            }

            foreach (Player player in PlayerList)
            {
                if(player.ActorID == playerId)
                {
                    //Check if the player already has equipment in this slot
                    //If this is a weapon, we need to compare the weapon categories, if it is armor, then the subcategories, if ammo we check if the player has the correct ranged weapon equipted.
                    if(newItem.itemCategory == "melee" || newItem.itemCategory == "ranged" || newItem.itemCategory == "ammo")
                    {
                        foreach (int item in player.equipmentList)
                        {
                            int n = 0;
                            while (ItemList[n].itemId != item)
                            {
                                n++;
                            }
                            if (ItemList[n].itemCategory == newItem.itemCategory)
                            {
                                serverMessage = $"You must remove your {ItemList[n].itemName} to equipt that_0";
                                return;
                            }
                        }
                    }
                    if (newItem.itemCategory == "armor")
                    {
                        foreach (int item in player.equipmentList)
                        {
                            int n = 0;
                            while (ItemList[n].itemId != item)
                            {
                                n++;
                            }
                            if (ItemList[n].itemSubcategory == newItem.itemSubcategory)
                            {
                                serverMessage = $"You must remove your {ItemList[n].itemName} to equipt that_0";
                                return;
                            }
                        }
                    }
                    //Remove the item from the player's inventory
                    playerInventory = player.InventoryId;
                    foreach(Inventory inventory in InventoryList)
                    {
                        if(inventory.inventoryID == playerInventory)
                        {
                            foreach(int item in inventory.itemsList)
                            {
                                if(item != ItemId)
                                {
                                    itemList.Add(item);
                                }
                            }
                            inventory.itemsList = itemList;
                        }
                    }
                    //Add the item to the player's equipment list
                    player.equipmentList.Add(ItemId);
                }
            }
        }

        public static void DequiptItem(string data)
        {
            string[] chunks = data.Split(',');
            string[] subchunks = chunks[0].Split('.');
            int.TryParse(subchunks[0], out int playerId);
            int.TryParse(subchunks[1], out int itemId);
            List<int> equipmentList = new List<int>();
            Item newItem = new Item();
            foreach(Player player in PlayerList)
            {
                if(player.ActorID == playerId)
                {
                    foreach(int item in player.equipmentList)
                    {
                        if(item != itemId)
                        {
                            equipmentList.Add(item);
                        }
                    }
                    player.equipmentList = equipmentList;
                    //Add the item to the player's inventory
                    foreach(Inventory inventory in InventoryList)
                    {
                        if(inventory.inventoryID == player.InventoryId)
                        {
                            inventory.itemsList.Add(itemId);
                        }
                    }
                }

            }
        }
        public static string GetEquipmentItems(string data)
        {
            string[] chunks = data.Split(',');
            int.TryParse(chunks[0], out int playerId);
            string response = "";
            List<int> equipmentList = new List<int>();
            foreach (Player player in PlayerList)
            {
                if (player.ActorID == playerId)
                {
                    foreach(int item in player.equipmentList)
                    {
                        equipmentList.Add(item);
                    }
                }
            }
            foreach (int itemId in equipmentList)
            {
                foreach (Item item in ItemList)
                {
                    if (item.itemId == itemId)
                    {
                        response += $"{item.itemId}.{item.itemSeed},";
                    }
                    // 0.id 1.seed
                }
            }
            return response;
        }

        public static void SaveGame()
        {
            string fileName = "PlayerList.json";
            string jsonString = JsonSerializer.Serialize(PlayerList);
            File.WriteAllText(fileName, jsonString);

            fileName = "ServerRandom.json";
            jsonString = JsonSerializer.Serialize(randomSeed);
            File.WriteAllText(fileName, jsonString);

            fileName = "MapRandom.json";
            jsonString = JsonSerializer.Serialize(MapSeed);
            File.WriteAllText(fileName, jsonString);

            fileName = "MapList.json";
            jsonString = JsonSerializer.Serialize(MapList);
            File.WriteAllText(fileName, jsonString);

            fileName = "InventoryList.json";
            jsonString = JsonSerializer.Serialize(InventoryList);
            File.WriteAllText(fileName, jsonString);

            fileName = "ItemList.json";
            jsonString = JsonSerializer.Serialize(ItemList);
            File.WriteAllText(fileName, jsonString);
        }

        public static void LoadGame()
        {
            string fileName = "PlayerList.json";
            string jsonString = File.ReadAllText(fileName);
            PlayerList = JsonSerializer.Deserialize<List<Player>>(jsonString);

            fileName = "ServerRandom.json";
            jsonString = File.ReadAllText(fileName);
            randomSeed = JsonSerializer.Deserialize<int>(jsonString);
            random = new Random(randomSeed);

            fileName = "MapRandom.json";
            jsonString = File.ReadAllText(fileName);
            MapSeed = JsonSerializer.Deserialize<int>(jsonString);

            fileName = "MapList.json";
            jsonString = File.ReadAllText(fileName);
            MapGenerationData = JsonSerializer.Deserialize<List<DungeonMap>>(jsonString);
            MapList.Add(new DungeonMap());
            LoadMapData(MapGenerationData[1]);

            fileName = "InventoryList.json";
            jsonString = File.ReadAllText(fileName);
            InventoryList = JsonSerializer.Deserialize<List<Inventory>>(jsonString);

            fileName = "ItemList.json";
            jsonString = File.ReadAllText(fileName);
            ItemList = JsonSerializer.Deserialize<List<Item>>(jsonString);
        }

        public static void LoadMapData(DungeonMap mapSection)
        {
            MapGenerator mapGenerator = new MapGenerator(Client._mapWidth, Client._mapHeight, 400, 13, 7);
            List<DungeonMap> mapList = mapGenerator.GenerateMapSection(mapSection.Seed, true, mapSection.XOrigin, mapSection.YOrigin, mapSection.ZOrigin, mapSection.SectionId);
            foreach (DungeonMap map in mapList)
            {
                MapList.Add(map);
            }

        }

        public static void WakeMonster(string data)
        {
            string[] chunks = data.Split('.');
            Int32.TryParse(chunks[0], out int ActorId);
            Int32.TryParse(chunks[1], out int mapId);
            foreach (Monster monster in MapList[mapId]._monsters)
            {
                if (monster.bIsAtRest && monster.ActorID == ActorId)
                {
                    monster.bIsAtRest = false;
                }
            }
        }

        public static int CreateMapSection(Player PlayerData, int ZOrigin)
        {
            int seed = Server.GenerateSeed();
            int mapId = -999;
            MapGenerator mapGenerator = new MapGenerator(Client._mapWidth, Client._mapHeight, 400, 13, 7);
            List<DungeonMap> newMaps = new List<DungeonMap>();

            if (PlayerData.YLevel % 3 == 0 || PlayerData.YLevel % 3 == 0)
            {
                if(PlayerData.XLevel % 3 == 0 || PlayerData.XLevel % 3 == 0)
                {
                    newMaps = mapGenerator.GenerateMapSection(seed, true, PlayerData.XLevel, PlayerData.YLevel, ZOrigin, MapList.Count);
                    mapId = MapList.Count;
                }
                if (PlayerData.XLevel % 3 == 1 || PlayerData.XLevel % 3 == -2)
                {
                    newMaps = mapGenerator.GenerateMapSection(seed, true, PlayerData.XLevel, PlayerData.YLevel, ZOrigin, MapList.Count);
                    mapId = MapList.Count + 1;
                }
                if (PlayerData.XLevel % 3 == 2 || PlayerData.XLevel % 3 == -1)
                {
                    newMaps = mapGenerator.GenerateMapSection(seed, true, PlayerData.XLevel, PlayerData.YLevel, ZOrigin, MapList.Count);
                    mapId = MapList.Count + 2;
                }
            }
            if (PlayerData.YLevel % 3 == 1 || PlayerData.YLevel % 3 == -2)
            {
                if (PlayerData.XLevel % 3 == 0 || PlayerData.XLevel % 3 == 0)
                {
                    newMaps = mapGenerator.GenerateMapSection(seed, true, PlayerData.XLevel, PlayerData.YLevel, ZOrigin, MapList.Count);
                    mapId = MapList.Count + 3;
                }
                if (PlayerData.XLevel % 3 == 1 || PlayerData.XLevel % 3 == -2)
                {
                    newMaps = mapGenerator.GenerateMapSection(seed, true, PlayerData.XLevel, PlayerData.YLevel, ZOrigin, MapList.Count);
                    mapId = MapList.Count + 4;
                }
                if (PlayerData.XLevel % 3 == 2 || PlayerData.XLevel % 3 == -1)
                {
                    newMaps = mapGenerator.GenerateMapSection(seed, true, PlayerData.XLevel, PlayerData.YLevel, ZOrigin, MapList.Count);
                    mapId = MapList.Count + 5;
                }
            }
            if (PlayerData.YLevel % 3 == 2 || PlayerData.YLevel % 3 == -1)
            {
                if (PlayerData.XLevel % 3 == 0 || PlayerData.XLevel % 3 == 0)
                {
                    newMaps = mapGenerator.GenerateMapSection(seed, true, PlayerData.XLevel, PlayerData.YLevel, ZOrigin, MapList.Count);
                    mapId = MapList.Count + 6;
                }
                if (PlayerData.XLevel % 3 == 1 || PlayerData.XLevel % 3 == -2)
                {
                    newMaps = mapGenerator.GenerateMapSection(seed, true, PlayerData.XLevel, PlayerData.YLevel, ZOrigin, MapList.Count);
                    mapId = MapList.Count + 7;
                }
                if (PlayerData.XLevel % 3 == 2 || PlayerData.XLevel % 3 == -1)
                {
                    newMaps = mapGenerator.GenerateMapSection(seed, true, PlayerData.XLevel, PlayerData.YLevel, ZOrigin, MapList.Count);
                    mapId = MapList.Count + 8;
                }
            }

            int n = MapList.Count;
            foreach (DungeonMap map in newMaps)
            {
                MapList.Add(map);
                MapList[n].MapId = n;
                n++;
            }
            return mapId;
        }
    }
}