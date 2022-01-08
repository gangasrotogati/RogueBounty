using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using RogueSharp;
using Roguelight.Systems;
using Roguelight.Behaviors;
using Roguelight.Tables;

namespace Roguelight.Core
{
    public class Engine
    {
        public RLRootConsole rootConsole;
        public static List<DungeonMap> DungeonMaps = new List<DungeonMap>();
        public static DungeonMap DungeonMap { get; set; }
        public static MessageLog MessageLog { get; set; }
        public static Player ClientPlayer = new Player();
        public static List<Item> PlayerItemList = new List<Item>();
        public static List<Item> LootList = new List<Item>();
        public static List<Item> PlayerEquipmentList = new List<Item>();
        public List<TargetNet> FovList = new List<TargetNet>();
        public static bool isExploring;
        public static bool isGoingDownStairs;
        public static bool isGoingUpStairs;
        public static bool bAutoMeleeAttack;
        public static long referenceTime;
        private string fovData = "";
        private static int inventoryUiSelectionIndex = 0;
        private static int inventoryUiSelectionIndexMaximum = 0;
        private static int inventoryStartOfMenu = 0;
        private static int inventoryEndOfMenu = 6;
        private static int lootUiSelectionIndex = 0;
        private static int lootUiSelectionIndexMaximum = 0;
        private static int lootStartOfMenu = 0;
        private static int lootEndOfMenu = 6;
        private static int equipmentUiSelectionIndex = 0;
        private static int equipmentUiSelectionIndexMaximum = 0;
        private static int equipmentStartOfMenu = 0;
        private static int equipmentEndOfMenu = 6;
        private static bool bIsInventoryOpen = false;
        private static bool bIsCharacterSheetOpen = false;
        private static bool bIsEquipmentOpen = false;
        private static bool bIsLootOpen = false;
        private static bool bIsAbilitiesOpen = false;
        private static ItemCategories _itemCategories = new ItemCategories();
        private static ItemNames _itemNames = new ItemNames();
        private static Materials _materials = new Materials();
        private static Qualities _qualities= new Qualities();
        private static SkillTypes _skillTypes = new SkillTypes();
        private static WieldTypes _wieldYypes = new WieldTypes();
        private static DateTime Timer = DateTime.Now;
        private static DateTime lastMovement = DateTime.Now;

        public Engine(RLRootConsole console)
        {
            SocketClient.GeneratePlayerData(SocketClient.SendData(Client.playerSeed + "," + fovData + ",<UPDATE>"));
            FovList = new List<TargetNet>();
            foreach (Monster monster in Client.MonsterList)
            {
                TargetNet targetNet = new TargetNet();
                FieldOfView monsterFov = new FieldOfView(DungeonMap);
                monsterFov.ComputeFov(monster.X, monster.Y, monster.Awareness, true);
                targetNet.monsterId = monster.ActorID;
                targetNet.Fov = monsterFov;
                FovList.Add(targetNet);
            }
            rootConsole = console;
            rootConsole.Render += Render;
            rootConsole.Update += Update;

            MessageLog = new MessageLog();
            MessageLog.Add("The rogue arrives on level 1_0");
            MessageLog.Add($"Level created with seed '{Client.mapSeed}'_0");
        }

        public void Render(object sender, UpdateEventArgs e)
        {
            //Clean the slate
            rootConsole.Clear();
            Client._mapConsole.Clear();
            Client._statConsole.Clear();
            Client._messageConsole.Clear();
            LootList = new List<Item>();
            int numberOfPlayers = Client.PlayerList.Count;
            DungeonMap.Draw(Client._mapConsole);
            MessageLog.Draw(Client._messageConsole);
            //Update player data from the server
            SocketClient.GeneratePlayerData(SocketClient.SendData(Client.playerSeed + "," + fovData + ",<UPDATE>"));
            fovData = "";

            //Draw the map
            //Draw the players
            int n = 0;
                Int32.TryParse(Client.playerSeed, out int playerSeed);
            foreach (Item item in Client.MapItemList)
            {
                if (item.mapId == DungeonMap.MapId && item.X > 0 && item.Y > 0)
                {
                    if (DungeonMap.IsInFov(item.X, item.Y))
                    {
                        Client._mapConsole.Set(item.X, item.Y, RLColor.LightGray, null, '/');
                        if(item.X == ClientPlayer.X && item.Y == ClientPlayer.Y)
                        {
                            LootList.Add(item);
                        }
                    }
                }
            }
            //Whenever we find items to loot, we have to activate the loot menu. We start by resetting the menu navigation variables. This function will test whether any items exist on the player tile.
            ResetLootMenuNav();
            //Check that the player exists and is connected to the server
            // Draw other players if they are registered, they are on the same ZLevel, they are online and they are in the FOV.
            foreach (Player Player in Client.PlayerList)
            {
                //Check if the specified actor is online and registered and at the same Zlevel as the player.
                if(Player.isOnline >= 0)
                {
                    //fovData += CheckPlayersFov(Player);
                    if (DungeonMap.IsInFov(Player.X, Player.Y))
                    {
                        //Client._mapConsole.Set(Player.X, Player.Y, Player.Color, null, Player.Symbol);
                        //If the player isn't controled by the client, then draw their stats as other's stats
                        if (Player.ActorID != playerSeed)
                        {
                            n++;
                            if(Player.ZLevel == 0)
                            {
                                Client._mapConsole.Set(Player.X, Player.Y, Player.Color, Swatch.BeigeDarker, Player.Symbol);
                            }
                            else
                            {
                                Client._mapConsole.Set(Player.X, Player.Y, Player.Color, Swatch.DbDark, Player.Symbol);
                            }
                            Player.DrawStats(Client._statConsole, n, Player.Health, Player.MaxHealth, Player.Name, $"{Player.Symbol}");
                        }
                    }
                }
                //Get updates about this player
                if (Player.ActorID == playerSeed && Player.isOnline >= 0)
                {
                    ClientPlayer = Player;
                    DungeonMap.UpdatePlayerFieldOfView(Player.X, Player.Y, Player.Awareness);
                    Player.DrawStats(Client._statConsole, ClientPlayer);
                    if(Player.ZLevel != DungeonMap.ZLevel || Player.XLevel != DungeonMap.XLevel || Player.YLevel != DungeonMap.YLevel)
                    {
                        GetNewMap(Player);
                        //Update the player data and create fovs for the new monsters
                        SocketClient.GeneratePlayerData(SocketClient.SendData(Client.playerSeed + "," + fovData + ",<UPDATE>"));
                        FovList = new List<TargetNet>();
                        foreach(Monster monster in Client.MonsterList) 
                        {
                            TargetNet targetNet = new TargetNet();
                            FieldOfView monsterFov = new FieldOfView(DungeonMap);
                            monsterFov.ComputeFov(monster.X, monster.Y, monster.Awareness, true);
                            targetNet.monsterId = monster.ActorID;
                            targetNet.Fov = monsterFov;
                            FovList.Add(targetNet);
                        }
                    }
                }
            }

            //Compute the Fov of monsters and update the server when a fov is active.
            bool bUpdateFov = false;
            long changeInTicks;
            Timer = DateTime.Now;
            changeInTicks = Timer.Ticks - lastMovement.Ticks;
            if (changeInTicks > 10000000 / 6)
            {
                bUpdateFov = true;
                lastMovement = DateTime.Now;
            }
            if (bUpdateFov)
            {
                foreach(Monster Monster in Client.MonsterList)
                {
                    if (!Monster.bIsAtRest)
                    {
                        fovData += CheckActorsFov(Monster);
                    }
                }
            }

            foreach (Monster Monster in Client.MonsterList)
            {
                //Check if the specified actor is online and registered and at the same Zlevel as the player.
                if (Monster.ActorID != 0 && Monster.mapId == DungeonMap.MapId)
                {
                    if (DungeonMap.IsInFov(Monster.X, Monster.Y))
                    {
                        //add the monster to the player targets list, prioritize the first target detected
                        if (ClientPlayer.TargetsList.Count == 0)
                        {
                            ClientPlayer.TargetsList.Add(new int[] { Monster.ActorID, 2 });
                        }
                        else
                        {
                            ClientPlayer.TargetsList.Add(new int[] { Monster.ActorID, 1 });
                        }
                        if(Monster.ZLevel == 0)
                        {
                            Client._mapConsole.Set(Monster.X, Monster.Y, Monster.Color, Swatch.BeigeDarker, Monster.Symbol);
                        }
                        else
                        {
                            Client._mapConsole.Set(Monster.X, Monster.Y, Monster.Color, null, Monster.Symbol);
                        }
                        //If the player isn't controled by the client, then draw their stats as other's stats
                        if (Monster.ActorID != playerSeed)
                        {
                            n++;
                            Monster.DrawStats(Client._statConsole, n, Monster.Health, Monster.MaxHealth, Monster.Name, $"{Monster.Symbol}");
                        }
                    }
                }
            }
            List<TargetNet> dummyFovList = new List<TargetNet>();
            foreach(TargetNet fov in FovList)
            {
                if (fov.Fov.IsInFov(ClientPlayer.X, ClientPlayer.Y))
                {
                    SocketClient.SendData($"{fov.monsterId}.{ClientPlayer.mapId}.<WAKE>");
                }
                else
                {
                    dummyFovList.Add(fov);
                }
            }
            FovList = dummyFovList;

            //Draw the player using client data, so that we never have to wait for a server update to draw the player.
            if (ClientPlayer.ZLevel == 0)
            {
                Client._mapConsole.Set(ClientPlayer.X, ClientPlayer.Y, ClientPlayer.Color, Swatch.BeigeDarker, ClientPlayer.Symbol);
            }
            else
            {
                Client._mapConsole.Set(ClientPlayer.X, ClientPlayer.Y, ClientPlayer.Color, Swatch.DbDark, ClientPlayer.Symbol);
            }

            RLConsole.Blit(Client._mapConsole, 0, 0, Client._mapWidth, Client._mapHeight,
              Client.rootConsole, 0, Client._inventoryHeight);
            Client._mapConsole.SetBackColor(0, 0, Client._mapWidth, Client._mapHeight, Swatch.Beige);
            RLConsole.Blit(Client._statConsole, 0, 0, Client._statWidth, Client._statHeight, Client.rootConsole, Client._mapWidth, 0);
            Client._statConsole.SetBackColor(0, 0, Client._mapWidth, Client._mapHeight, Swatch.Beige);
            RLConsole.Blit(Client._messageConsole, 0, 0, Client._messageWidth, Client._messageHeight, Client.rootConsole, 0, Client.screenHeight - Client._messageHeight);
            Client._messageConsole.SetBackColor(0, 0, Client._mapWidth, Client._mapHeight, Swatch.Beige);
            if (bIsInventoryOpen)
            {
                RLConsole.Blit(Client._inventoryConsole, 0, 0, Client._inventoryWidth, Client._inventoryHeight,
              Client.rootConsole, 0, 0);
                DrawInventory(Client._inventoryConsole);
            }
            else if (bIsCharacterSheetOpen)
            {
                RLConsole.Blit(Client._characterSheetConsole, 0, 0, Client._inventoryWidth, Client._inventoryHeight,
              Client.rootConsole, 0, 0);
            }
            else if (bIsEquipmentOpen)
            {
                RLConsole.Blit(Client._equipmentConsole, 0, 0, Client._inventoryWidth, Client._inventoryHeight,
              Client.rootConsole, 0, 0);
                DrawEquipmentConsole(Client._equipmentConsole);
            }
            else if (bIsAbilitiesOpen)
            {
                RLConsole.Blit(Client._abilitiesConsole, 0, 0, Client._inventoryWidth, Client._inventoryHeight,
              Client.rootConsole, 0, 0);
            }
            else
            {
                RLConsole.Blit(Client._lootConsole, 0, 0, Client._inventoryWidth, Client._inventoryHeight,
              Client.rootConsole, 0, 0);
                DrawLootConsole(Client._lootConsole);
            }

            if (bIsInventoryOpen)
            {
                Client._inventoryConsole.SetBackColor(0, 0, Client._inventoryWidth, Client._inventoryHeight, RLColor.Black);
                Client._inventoryConsole.Print(1, 1, "Character Sheet", RLColor.LightGray);
                Client._inventoryConsole.Print(20, 1, "Equipment", RLColor.LightGray);
                Client._inventoryConsole.Print(33, 1, "Inventory", RLColor.White);
                Client._inventoryConsole.Print(47, 1, "Abilities", RLColor.LightGray);
            }
            else if (bIsCharacterSheetOpen)
            {
                Client._characterSheetConsole.SetBackColor(0, 0, Client._characterSheetWidth, Client._characterSheetHeight, RLColor.Black);
                Client._characterSheetConsole.Print(1, 1, "Character Sheet", RLColor.White);
                Client._characterSheetConsole.Print(20, 1, "Equipment", RLColor.LightGray);
                Client._characterSheetConsole.Print(33, 1, "Inventory", RLColor.LightGray);
                Client._characterSheetConsole.Print(47, 1, "Abilities", RLColor.LightGray);
            }
            else if (bIsEquipmentOpen)
            {
                Client._equipmentConsole.SetBackColor(0, 0, Client._equipmentWidth, Client._equipmentHeight, RLColor.Black);
                Client._equipmentConsole.Print(1, 1, "Character Sheet", RLColor.LightGray);
                Client._equipmentConsole.Print(20, 1, "Equipment", RLColor.White);
                Client._equipmentConsole.Print(33, 1, "Inventory", RLColor.LightGray);
                Client._equipmentConsole.Print(47, 1, "Abilities", RLColor.LightGray);
            }
            else if (bIsAbilitiesOpen)
            {
                Client._abilitiesConsole.SetBackColor(0, 0, Client._equipmentWidth, Client._equipmentHeight, RLColor.Black);
                Client._abilitiesConsole.Print(1, 1, "Character Sheet", RLColor.LightGray);
                Client._abilitiesConsole.Print(20, 1, "Equipment", RLColor.LightGray);
                Client._abilitiesConsole.Print(33, 1, "Inventory", RLColor.LightGray);
                Client._abilitiesConsole.Print(47, 1, "Abilities", RLColor.White);
            }
            else if (bIsLootOpen)
            {
                Client.rootConsole.SetBackColor(0, 0, Client._equipmentWidth, Client._equipmentHeight, RLColor.Black);
                Client.rootConsole.Print(1, 1, "Character Sheet", RLColor.LightGray);
                Client.rootConsole.Print(20, 1, "Equipment", RLColor.LightGray);
                Client.rootConsole.Print(33, 1, "Inventory", RLColor.LightGray);
                Client.rootConsole.Print(47, 1, "Abilities", RLColor.LightGray);
            }
            else
            {
                Client.rootConsole.SetBackColor(0, 0, Client._equipmentWidth, Client._equipmentHeight, RLColor.Black);
                Client.rootConsole.Print(1, 1, "Character Sheet", RLColor.LightGray);
                Client.rootConsole.Print(20, 1, "Equipment", RLColor.LightGray);
                Client.rootConsole.Print(33, 1, "Inventory", RLColor.LightGray);
                Client.rootConsole.Print(47, 1, "Abilities", RLColor.LightGray);
            }
            rootConsole.Draw();
        }

        public void Update(object sender, UpdateEventArgs e)
        {
            RLKeyPress keyPress = rootConsole.Keyboard.GetKeyPress();


            if (keyPress != null)
            {
                isGoingDownStairs = false;
                isExploring = false;
                isGoingUpStairs = false;
                switch (keyPress.Key)
                {
                    case RLKey.Keypad8: Client.CommandSystem.MovePlayer(Direction.Up); break;
                    case RLKey.Keypad2: Client.CommandSystem.MovePlayer(Direction.Down); break;
                    case RLKey.Keypad4: Client.CommandSystem.MovePlayer(Direction.Left); break;
                    case RLKey.Keypad6: Client.CommandSystem.MovePlayer(Direction.Right); break;
                    case RLKey.Keypad7: Client.CommandSystem.MovePlayer(Direction.UpLeft); break;
                    case RLKey.Keypad9: Client.CommandSystem.MovePlayer(Direction.UpRight); break;
                    case RLKey.Keypad1: Client.CommandSystem.MovePlayer(Direction.DownLeft); break;
                    case RLKey.Keypad3: Client.CommandSystem.MovePlayer(Direction.DownRight); break;
                    case RLKey.Period: GoDownStairs(); break;
                    case RLKey.Comma: GoUpStairs(); break;
                    case RLKey.Keypad0: isExploring = true; break;
                    case RLKey.Number1: if(bAutoMeleeAttack == false) bAutoMeleeAttack = true; else if (bAutoMeleeAttack) bAutoMeleeAttack = false; break;
                    case RLKey.I: OpenInventory(); break;
                    case RLKey.C: OpenCharacterSheet(); break;
                    case RLKey.E: OpenEquipment(); break;
                    case RLKey.A: OpenAbilities(); break;
                    case RLKey.KeypadPlus:
                        {
                            if (bIsInventoryOpen)
                            {
                                if (inventoryUiSelectionIndex < inventoryUiSelectionIndexMaximum)
                                {
                                    inventoryUiSelectionIndex++;
                                }
                                break;
                            }
                            else if (bIsEquipmentOpen)
                            {
                                if (equipmentUiSelectionIndex < PlayerEquipmentList.Count - 1)
                                {
                                    equipmentUiSelectionIndex++;
                                }
                                break;
                            }
                            else
                            {
                                if (lootUiSelectionIndex < lootUiSelectionIndexMaximum)
                                {
                                    lootUiSelectionIndex++;
                                }
                                break;
                            }
                        }
                    case RLKey.KeypadMinus:
                        {
                            if (bIsInventoryOpen)
                            {
                                if (inventoryUiSelectionIndex > 0)
                                {
                                    inventoryUiSelectionIndex--;
                                }
                                break;
                            }
                            else if (bIsEquipmentOpen)
                            {
                                if (equipmentUiSelectionIndex > 0)
                                {
                                    equipmentUiSelectionIndex--;
                                }
                                break;
                            }
                            else
                            {
                                if (lootUiSelectionIndex > 0)
                                {
                                    lootUiSelectionIndex--;
                                }
                                break;
                            }
                        }
                    case RLKey.D: DropItem(); break;
                    case RLKey.G: GetItem(); break;
                    case RLKey.Enter:
                        {
                            if(bIsInventoryOpen == true)
                            {
                                EquiptItem();
                            }
                            else if (bIsEquipmentOpen == true)
                            {
                                DequiptItem();
                            }
                            break;
                        }
                }
            }
            else
            {
                if (isExploring)
                {
                    isExploring = Explore();
                }
                if (isGoingDownStairs)
                {
                        isGoingDownStairs = FindStairsDown();
                }
                if (isGoingUpStairs)
                {
                        isGoingUpStairs = FindStairsUp();
                }
                if (bAutoMeleeAttack)
                {
                    AutoMeleeAttack();
                }
            }

            Client._mapConsole.SetBackColor(0, 0, Client._mapWidth, Client._mapHeight, RLColor.Black);
            Client._mapConsole.Print(1, 1, "Map", RLColor.White);

            Client._statConsole.SetBackColor(0, 0, Client._statWidth, Client._statHeight, RLColor.LightGray);
            Client._statConsole.Print(1, 1, "Stats", RLColor.White);

            Client._messageConsole.SetBackColor(0, 0, Client._messageWidth, Client._messageHeight, Swatch.DbDeepWater);
        }

        public void GetNewMap(Player Player)
        {
            bool mapFound = false;
            while(mapFound == false)
            {
                foreach (DungeonMap Map in DungeonMaps)
                {
                    if (Map.MapId == Player.mapId)
                    {
                        DungeonMap = Map;
                        mapFound = true;
                    }
                }
                if (mapFound == false)
                {
                    SocketClient.RequestDungeonMap(Player.mapId);
                }
            }
        }
        private void GoDownStairs()
        {
            if(DungeonMap.StairsDown != null)
            {
                if (ClientPlayer.X == DungeonMap.StairsDown.X && ClientPlayer.Y == DungeonMap.StairsDown.Y)
                {
                    Client.CommandSystem.MovePlayer(Direction.DownStairs);
                    AutoExplore.previousCell = null;
                }
                else
                {
                    isGoingDownStairs = true;
                }
            }
        }

        private void GoUpStairs()
        {
            if(DungeonMap.StairsUp != null)
            {
                if (ClientPlayer.X == DungeonMap.StairsUp.X && ClientPlayer.Y == DungeonMap.StairsUp.Y)
                {
                    Client.CommandSystem.MovePlayer(Direction.UpStairs);
                    AutoExplore.previousCell = null;
                }
                else
                {
                    isGoingUpStairs = true;
                }
            }
        }

        public bool Explore()
        {
            bool didPlayerMove = true;
            var behavior = new AutoExplore();
            didPlayerMove = behavior.Act(ClientPlayer, Client.CommandSystem);
            return didPlayerMove;
        }

        public void AutoMeleeAttack()
        {
            if(ClientPlayer.TargetsList.Count > 0)
            {
                if(isExploring == true)
                {
                    isExploring = false;
                }
                if (isGoingUpStairs == true)
                {
                    isGoingUpStairs = false;
                }
                if (isGoingDownStairs == true)
                {
                    isGoingDownStairs = false;
                }
                ClientPlayer.CurrentTarget = ClientPlayer.GetNewTarget();
                var behavior = new AutoMeleeAttack();
                behavior.Act(ClientPlayer.CurrentTarget, ClientPlayer);
            }
        }

        public bool FindStairsDown()
        {
            bool didPlayerMove = true;
            var behavior = new FindStairsDown();
            didPlayerMove = behavior.Act(ClientPlayer, Client.CommandSystem);
            return didPlayerMove;
        }
        public bool FindStairsUp()
        {
            bool didPlayerMove = true;
            var behavior = new FindStairsUp();
            didPlayerMove = behavior.Act(ClientPlayer, Client.CommandSystem);
            return didPlayerMove;
        }

        public string CheckActorsFov(Actor other)
        {
            DungeonMap dungeonMap = Engine.DungeonMap;
            Player player = ClientPlayer;
            FieldOfView monsterFov = new FieldOfView(dungeonMap);
            ICell stepForward;
            int? stepForwardX;
            int? stepForwardY;
            string data = "";

            // If the monster has not been alerted, compute a field-of-view 
            // Use the monster's Awareness value for the distance in the FoV check
            // If the player is in the monster's FoV then alert it
            // Add a message to the MessageLog regarding this alerted status

            monsterFov.ComputeFov(other.X, other.Y, other.Awareness, true);
            stepForward = GetPath(other, player);
            if(stepForward == null)
            {
                return "";
            }
            stepForwardX = stepForward.X;
            stepForwardY = stepForward.Y;
            data = $"{other.ActorID}x{stepForwardX}x{stepForwardY}x0.";
            if (monsterFov.IsInFov(player.X, player.Y))
            {
                data = $"{other.ActorID}x{stepForwardX}x{stepForwardY}x1.";
            }
            return data;
        }

        public static ICell GetPath(Actor monster, Actor player)
        {
            PathFinder pathFinder = new PathFinder(DungeonMap);
            Path path = null;
            ICell cell = null;
            try
            {
                path = pathFinder.ShortestPath(
                DungeonMap.GetCell(monster.X, monster.Y),
                DungeonMap.GetCell(player.X, player.Y));
                if(path.Length <= 1)
                {
                    return null;
                }
                cell = path.StepForward();
            }
            catch (PathNotFoundException)
            {
            }
            return cell;
        }

        public static void OpenInventory()
        {
            if(bIsInventoryOpen == false)
            {
                bIsInventoryOpen = true;
                bIsEquipmentOpen = false;
                bIsCharacterSheetOpen = false;
                bIsAbilitiesOpen = false;
                PlayerItemList = UpdateInventory(SocketClient.SendData($"{Client.inventoryId},<INVENTORY>"));
                inventoryUiSelectionIndex = 0;
                inventoryStartOfMenu = 0;
                inventoryEndOfMenu = PlayerItemList.Count;
                if(inventoryEndOfMenu > 6)
                {
                    inventoryEndOfMenu = 6;
                }

            }
            else
            {
                bIsInventoryOpen = false;
                bIsEquipmentOpen = false;
                bIsCharacterSheetOpen = false;
                bIsAbilitiesOpen = false;
            }
        }
        public static void OpenCharacterSheet()
        {
            if (bIsCharacterSheetOpen == false)
            {
                bIsInventoryOpen = false;
                bIsEquipmentOpen = false;
                bIsCharacterSheetOpen = true;
                bIsAbilitiesOpen = false;
            }
            else
            {
                bIsInventoryOpen = false;
                bIsEquipmentOpen = false;
                bIsCharacterSheetOpen = false;
                bIsAbilitiesOpen = false;
            }
        }
        public static void OpenEquipment()
        {
            if (bIsEquipmentOpen == false)
            {
                bIsInventoryOpen = false;
                bIsEquipmentOpen = true;
                bIsCharacterSheetOpen = false;
                bIsAbilitiesOpen = false;
                PlayerEquipmentList = UpdateEquipment(SocketClient.SendData($"{Client.playerSeed},<EQUIPMENT>"));
                equipmentUiSelectionIndex = 0;
            }
            else
            {
                bIsInventoryOpen = false;
                bIsEquipmentOpen = false;
                bIsCharacterSheetOpen = false;
                bIsAbilitiesOpen = false;
            }
        }

        public static void OpenAbilities()
        {
            if (bIsAbilitiesOpen == false)
            {
                bIsInventoryOpen = false;
                bIsEquipmentOpen = false;
                bIsCharacterSheetOpen = false;
                bIsAbilitiesOpen = true;
            }
            else
            {
                bIsInventoryOpen = false;
                bIsEquipmentOpen = false;
                bIsCharacterSheetOpen = false;
                bIsAbilitiesOpen = false;
            }
        }

        private static List<Item> UpdateInventory(string data)
        {
            List<Item> itemList = new List<Item>();
            string[] chunks = data.Split(',');
            for(int i = 0; i < chunks.Length - 1; i++)
            {
                //0.id 1.seed
                string[] subChunks = chunks[i].Split('.');
                Item item = new Item();
                int.TryParse(subChunks[0], out int id);
                item.itemId = id;
                int.TryParse(subChunks[1], out int seed);
                item.itemSeed = seed;
                item = ItemGenerator.GenerateItem(id, seed);
                itemList.Add(item);
            }

            return itemList;
        }
        private static List<Item> UpdateEquipment(string data)
        {
            List<Item> equipmentList = new List<Item>();
            string[] chunks = data.Split(',');
            for (int i = 0; i < chunks.Length - 1; i++)
            {
                //0.id 1. seed
                string[] subChunks = chunks[i].Split('.');
                Item item = new Item();
                int.TryParse(subChunks[0], out int id);
                item.itemId = id;
                int.TryParse(subChunks[1], out int seed);
                item.itemSeed = seed;
                item = ItemGenerator.GenerateItem(id, seed);
                equipmentList.Add(item);
            }

            return equipmentList;
        }
        public void DrawInventory(RLConsole console)
        {
            Client._inventoryConsole.Clear();
            RLColor messageColor = RLColor.White;
            inventoryUiSelectionIndexMaximum = PlayerItemList.Count - 1;

            if (PlayerItemList.Count > 0)
            {
                if (inventoryUiSelectionIndex > inventoryEndOfMenu - 1)
                {
                    if(inventoryUiSelectionIndex >= 6)
                    {
                        inventoryStartOfMenu = inventoryUiSelectionIndex - 5;
                        inventoryEndOfMenu = inventoryUiSelectionIndex + 1;
                    }
                    else
                    {
                        inventoryUiSelectionIndex--;
                    }
                }

                if (inventoryUiSelectionIndex < inventoryStartOfMenu)
                {
                    inventoryEndOfMenu = inventoryUiSelectionIndex + 6;
                    inventoryStartOfMenu = inventoryUiSelectionIndex;

                }

                if (PlayerItemList.Count < inventoryEndOfMenu)
                {
                    inventoryEndOfMenu = PlayerItemList.Count;
                }

                for (int i = inventoryStartOfMenu; i < inventoryEndOfMenu; i++)
                {
                    Item item = PlayerItemList[i];
                    if (inventoryUiSelectionIndex == i)
                    {
                        console.Print(1, i + 2 - inventoryStartOfMenu, $"{item.Symbol} {item.quality} {item.material} {item.itemName}", RLColor.White);
                    }
                    else
                    {
                        console.Print(1, i + 2 - inventoryStartOfMenu, $"{item.Symbol} {item.quality} {item.material} {item.itemName}", RLColor.LightGray);
                    }
                }
            }
            else
            {
                console.Print(1, 3, $"It's empty", RLColor.LightGray);
            }
        }
        public void DropItem()
        {
            if (bIsInventoryOpen)
            {
                if(PlayerItemList.Count > 0)
                {
                    SocketClient.SendData($"{ClientPlayer.ActorID}.{PlayerItemList[inventoryUiSelectionIndex].itemId},<DROP>");
                    PlayerItemList = UpdateInventory(SocketClient.SendData($"{Client.inventoryId},<INVENTORY>"));
                    if (inventoryUiSelectionIndex == inventoryUiSelectionIndexMaximum && inventoryUiSelectionIndex != 0)
                    {
                        inventoryUiSelectionIndex -= 1;
                    }
                }

            }
        }
        public void DrawLootConsole(RLConsole console)
        {
            Client._lootConsole.Clear();
            RLColor messageColor = RLColor.White;
            lootUiSelectionIndexMaximum = LootList.Count - 1;

            if (LootList.Count > 0)
            {
                if (lootUiSelectionIndex > lootEndOfMenu - 1)
                {
                    if (lootUiSelectionIndex >= 6)
                    {
                        lootStartOfMenu = lootUiSelectionIndex - 5;
                        lootEndOfMenu = lootUiSelectionIndex + 1;
                    }
                    else
                    {
                        lootUiSelectionIndex--;
                    }
                }

                if (lootUiSelectionIndex < lootStartOfMenu)
                {
                    lootEndOfMenu = lootUiSelectionIndex + 6;
                    lootStartOfMenu = lootUiSelectionIndex;
                }

                if (LootList.Count < lootEndOfMenu)
                {
                    lootEndOfMenu = LootList.Count;
                }

                if(lootStartOfMenu < 0)
                {
                    lootStartOfMenu = 0;
                }
                if (lootUiSelectionIndex < 0)
                {
                    lootStartOfMenu = 0;
                }

                for (int i = lootStartOfMenu; i < lootEndOfMenu; i++)
                {
                    Item item = LootList[i];
                    if (lootUiSelectionIndex == i)
                    {
                        console.Print(1, i + 2 - lootStartOfMenu, $"{item.Symbol} {item.quality} {item.material} {item.itemName}", RLColor.White);
                    }
                    else
                    {
                        console.Print(1, i + 2 - lootStartOfMenu, $"{item.Symbol} {item.quality} {item.material} {item.itemName}", RLColor.LightGray);
                    }
                }
            }
        }
        public void GetItem()
        {
            if (!bIsInventoryOpen && !bIsCharacterSheetOpen && !bIsEquipmentOpen)
            {
                if(LootList.Count > 0)
                {
                    SocketClient.SendData($"{ClientPlayer.ActorID}.{LootList[lootUiSelectionIndex].itemId},<GET>");
                    PlayerItemList = UpdateInventory(SocketClient.SendData($"{Client.inventoryId},<INVENTORY>"));
                    if (lootUiSelectionIndex == lootUiSelectionIndexMaximum && lootUiSelectionIndex != 0)
                    {
                        lootUiSelectionIndex -= 1;
                    }
                }
            }
        }

        public void EquiptItem()
        {
            if (bIsInventoryOpen)
            {
                if(PlayerItemList.Count > 0)
                {
                    SocketClient.SendData($"{ClientPlayer.ActorID}.{PlayerItemList[inventoryUiSelectionIndex].itemId},<EQUIPT>");
                    PlayerItemList = UpdateInventory(SocketClient.SendData($"{Client.inventoryId},<INVENTORY>"));
                    if (inventoryUiSelectionIndex == inventoryUiSelectionIndexMaximum && inventoryUiSelectionIndex != 0)
                    {
                        inventoryUiSelectionIndex -= 1;
                    }
                }
            }
        }

        public void DequiptItem()
        {
            if (bIsEquipmentOpen)
            {
                if(PlayerEquipmentList.Count > 0)
                {
                    SocketClient.SendData($"{ClientPlayer.ActorID}.{PlayerEquipmentList[equipmentUiSelectionIndex].itemId},<DEQUIPT>");
                    PlayerEquipmentList = UpdateEquipment(SocketClient.SendData($"{Client.playerSeed},<EQUIPMENT>"));
                }
                if (equipmentUiSelectionIndex == PlayerEquipmentList.Count - 1 && equipmentUiSelectionIndex != 0)
                {
                    equipmentUiSelectionIndex -= 1;
                }
            }
        }

        private static void ResetLootMenuNav()
        {
            if (LootList.Count > 0)
            {
                lootUiSelectionIndexMaximum = LootList.Count - 1;
                if (bIsLootOpen == false && bIsInventoryOpen == false)
                {
                    bIsLootOpen = true;
                    lootUiSelectionIndex = 0;
                    lootStartOfMenu = 0;
                    lootEndOfMenu = LootList.Count;
                    if (lootEndOfMenu > 8)
                    {
                        lootEndOfMenu = 8;
                    }
                }
            }
            if (LootList.Count == 0)
            {
                bIsLootOpen = false;
            }
        }

        private static void DrawEquipmentConsole(RLConsole console)
        {
            Client._equipmentConsole.Clear();
            //sort the equipment into categories.
            List<Item> sortedList = new List<Item>();
            int w = 0; //number of weapons
            int a = 0; //number of ammos
            int r = 0; //number of armors
            int n = 0; //index for the sorted equipment list
            for (int i = 0; i < PlayerEquipmentList.Count; i++)
            {
                if(PlayerEquipmentList[i].itemCategory == "melee" || PlayerEquipmentList[i].itemCategory == "ranged")
                {
                    sortedList.Add(PlayerEquipmentList[i]);
                }
            }
            for (int i = 0; i < PlayerEquipmentList.Count; i++)
            {
                if (PlayerEquipmentList[i].itemCategory == "ammo")
                {
                    sortedList.Add(PlayerEquipmentList[i]);
                }
            }
            for (int i = 0; i < PlayerEquipmentList.Count; i++)
            {
                if (PlayerEquipmentList[i].itemCategory == "armor")
                {
                    sortedList.Add(PlayerEquipmentList[i]);
                }
            }

            //We need the equipment list to match the sorted list so that the indexing works properly.
            PlayerEquipmentList = sortedList;

            console.Print(3, 3, "WEAPONS", RLColor.LightGray);
            console.Print(31, 3, "AMMO", RLColor.LightGray);
            console.Print(56, 3, "ARMOR", RLColor.LightGray);

            foreach (Item item in sortedList)
            {
                if (item.itemCategory == "melee" || item.itemCategory == "ranged")
                {
                    w++;
                    if(n == equipmentUiSelectionIndex)
                    {
                        console.Print(1, 3 + w, $"{item.Symbol} {item.quality} {item.material} {item.itemName}", RLColor.White);
                    }
                    else
                    {
                        console.Print(1, 3 + w, $"{item.Symbol} {item.quality} {item.material} {item.itemName}", RLColor.LightGray);
                    }
                }
                if (item.itemCategory == "ammo")
                {
                    a++;
                    if (n == equipmentUiSelectionIndex)
                    {
                        console.Print(29, 3 + a, $"{item.Symbol} {item.quality} {item.material} {item.itemName}", RLColor.White);
                    }
                    else
                    {
                        console.Print(29, 3 + a, $"{item.Symbol} {item.quality} {item.material} {item.itemName}", RLColor.LightGray);
                    }
                }
                if (item.itemCategory == "armor")
                {
                    r++;
                    if (n == equipmentUiSelectionIndex)
                    {
                        console.Print(54, 3 + r, $"{item.Symbol} {item.quality} {item.material} {item.itemName}", RLColor.White);
                    }
                    else
                    {
                        console.Print(54, 3 + r, $"{item.Symbol} {item.quality} {item.material} {item.itemName}", RLColor.LightGray);
                    }
                }
                n++; //keep track of the item slots so we can select them using the keypad.
            }
            if (w == 0)
            {
                console.Print(3, 4, $"None", RLColor.LightGray);
            }
            if (a == 0)
            {
                console.Print(31, 4, $"None", RLColor.LightGray);
            }
            if (r == 0)
            {
                console.Print(56, 4, $"None", RLColor.LightGray);
            }
        }
    }
}
