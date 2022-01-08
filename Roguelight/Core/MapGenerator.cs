using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;
using RogueSharp.DiceNotation;
using Roguelight.Monsters;

namespace Roguelight.Core
{
    public class MapGenerator
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _maxRooms;
        private readonly int _roomMaxSize;
        private readonly int _roomMinSize;
        public readonly int MapSize = 3;

        private static int[,,] terrain;

        public static int playerSpawnX;
        public static int playerSpawnY;

        public static bool IsServer;
        public static int[,] DisplayGrid;

        private readonly DungeonMap _map;

        // The constructor is included in RogueSharp https://bitbucket.org/FaronBracy/roguesharp/wiki/RogueSharp/Map/README.md
        // Generate a new map that is a simple open floor with walls around the outside
        // Constructing a new MapGenerator requires the dimensions of the maps it will create
        // as well as the sizes and maximum number of rooms
        public MapGenerator(int width, int height, int maxRooms, int roomMaxSize, int roomMinSize)
        {
            _width = width;
            _height = height;
            _maxRooms = maxRooms;
            _roomMaxSize = roomMaxSize;
            _roomMinSize = roomMinSize;
            _map = new DungeonMap();
        }

        // Generate a new map that places rooms randomly
        public DungeonMap CreateMap(int seed, bool bCreateMonsters, int zLevel, int xLevel, int yLevel)
        {
            _map.Seed = seed;
            _map.ZLevel = zLevel;
            _map.XLevel = xLevel;
            _map.YLevel = yLevel;
            Random random;
            random = new Random(_map.Seed);
            // Set the properties of all cells to false
            _map.Initialize(_width, _height);
            // Try to place as many rooms as the specified maxRooms
            // Note: Only using decrementing loop because of WordPress formatting
            for (int r = _maxRooms; r > 0; r--)
            {
                // Determine the size and position of the room randomly
                int roomWidth = random.Next(_roomMinSize, _roomMaxSize);
                int roomHeight = random.Next(_roomMinSize, _roomMaxSize);
                int roomXPosition = random.Next(0, _width - roomWidth - 1);
                int roomYPosition = random.Next(0, _height - roomHeight - 1);

                // All of our rooms can be represented as Rectangles
                var newRoom = new Rectangle(roomXPosition, roomYPosition,
                  roomWidth, roomHeight);

                // Check to see if the room rectangle intersects with any other rooms
                bool newRoomIntersects = _map.Rooms.Any(room => newRoom.Intersects(room));

                // As long as it doesn't intersect add it to the list of rooms
                if (!newRoomIntersects)
                {
                    _map.Rooms.Add(newRoom);
                }
            }
            // Iterate through each room that we wanted placed 
            // call CreateRoom to make it
            foreach (Rectangle room in _map.Rooms)
            {
                CreateRoom(room, _map);
            }
            for (int r = 1; r < _map.Rooms.Count; r++)
            {
                // For all remaing rooms get the center of the room and the previous room
                int previousRoomCenterX = _map.Rooms[r - 1].Center.X;
                int previousRoomCenterY = _map.Rooms[r - 1].Center.Y;
                int currentRoomCenterX = _map.Rooms[r].Center.X;
                int currentRoomCenterY = _map.Rooms[r].Center.Y;

                // Give a 50/50 chance of which 'L' shaped connecting hallway to tunnel out
                if (random.Next(1, 2) == 1)
                {
                    CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, previousRoomCenterY, _map);
                    CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, currentRoomCenterX, _map);
                }
                else
                {
                    CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, previousRoomCenterX, _map);
                    CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, currentRoomCenterY, _map);
                }

            }
            if (zLevel != 0)
            {
                //Add stairs
                //CreateStairs(random, _map);
                if (bCreateMonsters)
                {
                    PlaceMonsters(_map);
                }
                //Once the rooms are created, place the spawn point in the center of the first room
                _map.SpawnX = _map.Rooms[0].Center.X;
                _map.SpawnY = _map.Rooms[0].Center.Y;
            }

            if (zLevel == 0)
            {
                for (int x = 0; x < _width; x++)
                {
                    for (int y = 0; y < _height; y++)
                    {
                        _map.SetCellProperties(x, y, true, true, false);
                    }
                }
                //CreateTrees(random, _map);
                //CreateStairsDown(random, _map);
                CreateGrass(_map);
                _map.SpawnX = _width / 2;
                _map.SpawnY = _height / 2;
            }
            return _map;
        }

        private void CreateRoom(Rectangle room, DungeonMap map)
        {
            for (int x = room.Left + 2; x < room.Right; x++)
            {
                for (int y = room.Top + 2; y < room.Bottom; y++)
                {
                    map.SetCellProperties(x, y, true, true, false);
                }
            }
        }

        public static void GenerateMap() //Old
        {
            //Generate random values based on the map seed.
            //The server uses its own map seed. The client gets a map seed from the server.
            DisplayGrid = new int[Client.screenWidth, Client.screenHeight];
            if (IsServer)
            {
                Random random = new Random(Server.MapSeed);
            }
            else
            {
                Int32.TryParse(Client.mapSeed, out int mapSeed);
                Random random = new Random(mapSeed);
            }

            for(int i = 0; i < Client.screenWidth; i++)
            {
                for (int j = 0; j < Client.screenHeight; j++)
                {
                    DisplayGrid[i, j] = 1;
                }
            }

        }
        // Carve a tunnel out of the map parallel to the x-axis
        private void CreateHorizontalTunnel(int xStart, int xEnd, int yPosition, DungeonMap map)
        {
            for (int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); x++)
            {
                map.SetCellProperties(x, yPosition, true, true);
            }
        }

        // Carve a tunnel out of the map parallel to the y-axis
        private void CreateVerticalTunnel(int yStart, int yEnd, int xPosition, DungeonMap map)
        {
            for (int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); y++)
            {
                map.SetCellProperties(xPosition, y, true, true);
            }
        }
        private void CreateStairs(Random random, DungeonMap map)
        {
            int x1 = random.Next(0, _width);
            int y1 = random.Next(0, _height);
            int x2 = random.Next(0, _width);
            int y2 = random.Next(0, _height);

            int i = 0;
            int j = 0;
            while (map.IsWalkable(x1, y1) == false && i < 100)
            {
                x1 = random.Next(0, _width);
                y1 = random.Next(0, _height);
                i++;
            }
            while (map.IsWalkable(x2, y2) == false && j < 100)
            {
                x2 = random.Next(0, _width);
                y2 = random.Next(0, _height);
                j++;
            }

            map.StairsUp = new Stairs
            {
                X = x1,
                Y = y1,
                IsUp = true
            };
            map.StairsDown = new Stairs
            {
                X = x2,
                Y = y2,
                IsUp = false
            };
        }
        private void CreateStairsDown(int seed, DungeonMap map)
        {
            Random random = new Random(seed);
            int x = random.Next(0, _width);
            int y = random.Next(0, _height);
            while(!map.IsWalkable(x, y))
            {
                x = random.Next(0, _width);
                y = random.Next(0, _height);
            }
            map.StairsDown = new Stairs
            {
                X = x,
                Y = y,
                IsUp = false
            };
        }
        private void CreateStairsUp(int seed, DungeonMap map)
        {
            Random random = new Random(seed + 1);
            int x = random.Next(0, _width);
            int y = random.Next(0, _height);
            while (!map.IsWalkable(x, y))
            {
                x = random.Next(0, _width);
                y = random.Next(0, _height);
            }
            map.StairsUp = new Stairs
            {
                X = x,
                Y = y,
                IsUp = true
            };
        }

        private void CreateTrees(int seed, DungeonMap map)
        {
            Random random = new Random(seed);
            int clusters = random.Next(5, 11);
            for(int i = 0; i < clusters; i++)
            {
                int[] randomNumberInts = new int[9];
                string treeSeed = random.Next(100000000, 999999999).ToString();
                char[] randomNumbersStrings = treeSeed.ToCharArray();
                int v = 0;
                foreach (char number in randomNumbersStrings)
                {
                    Int32.TryParse(number.ToString(), out randomNumberInts[v]);
                    v++;
                }
                int trees = randomNumberInts[1];
                int xCoord = randomNumberInts[1] * randomNumberInts[3] * i;
                int yCoord = randomNumberInts[0] * randomNumberInts[2] * i;
                for(int j = 0; j < trees; j++)
                {
                    Terrain tree = new Terrain();
                    tree.Category = "tree";
                    tree.X = xCoord + random.Next(1, 21);
                    tree.Y = yCoord + random.Next(1, 21);
                    int n = 0;
                    while((tree.X < 0 || tree.X > _width - 1 || tree.Y < 0 || tree.Y > _height - 1 || !map.IsWalkable(tree.X, tree.Y)) && n < 50)
                    {
                        tree.X = xCoord + randomNumberInts[4];
                        tree.Y = yCoord + randomNumberInts[5];
                        n++;
                    }
                    if(n < 50)
                    {
                        map.SetCellProperties(tree.X, tree.Y, false, false);
                        tree.Symbol = 3;
                        map._terrain.Add(tree);
                    }
                }

            }
        }
        private void CreateGrass(DungeonMap map)
        {
            foreach(ICell cell in map.GetAllCells())
            {
                if(cell.IsWalkable == true && (cell.X != map.StairsDown.X || cell.Y != map.StairsDown.Y))
                {
                    Terrain grass = new Terrain();
                    grass.X = cell.X;
                    grass.Y = cell.Y;
                    grass.Category = "grass";
                    grass.Symbol = 4;
                    map._terrain.Add(grass);
                }
            }
        }
        private void PlaceMonsters(DungeonMap map)
        {
            Random random = new Random();
            List<int[]> monsterSpawns = new List<int[]>();
            int[] monsterSpawn = new int[2];
            bool isSpawnPointInvalid = false;
            bool isCreatureValid;
            foreach (var room in map.Rooms)
            {
                isCreatureValid = true;
                // Each room has a 60% chance of having monsters
                if (true)
                {
                    // Generate between 1 and 4 monsters
                    var numberOfMonsters = Dice.Roll("1D4");
                    for (int i = 0; i < numberOfMonsters; i++)
                    {
                        // Find a random walkable location in the room to place the monster
                        Point randomRoomLocation = map.GetRandomWalkableLocationInRoom(room);
                        // It's possible that the room doesn't have space to place a monster
                        // In that case skip creating the monster
                        if (randomRoomLocation != null)
                        {
                            // Temporarily hard code this monster to be created at level 1
                            var monster = SkeletonMan.Create(1);
                            //Find a random spawn point for the monster.
                            int j = 0;
                            do
                            {
                                j++;
                                monster.X = random.Next(room.Left + 2, room.Right - 1);
                                monster.Y = random.Next(room.Top + 2, room.Bottom - 1);
                                if (monsterSpawns.Count > 0)
                                {
                                    foreach (int[] spawn in monsterSpawns)
                                    {
                                        if (spawn[0] == monster.X && spawn[1] == monster.Y)
                                        {
                                            isSpawnPointInvalid = true;
                                        }
                                    }
                                }
                                if(map.StairsUp != null && monster.X == map.StairsUp.X && monster.Y == map.StairsUp.Y)
                                {
                                    isSpawnPointInvalid = true;
                                }
                                if(j == 50)
                                {
                                    isCreatureValid = false;
                                    break;
                                }
                            } while (isSpawnPointInvalid);
                            if(isCreatureValid)
                            {
                                monsterSpawn[0] = monster.X;
                                monsterSpawn[1] = monster.Y;
                                monsterSpawns.Add(monsterSpawn);
                                monster.ZLevel = map.ZLevel;
                                monster.mapId = map.Seed;
                                map.AddMonster(monster);
                            }
                        }
                    }
                }
            }
        }

        public List<DungeonMap> GenerateMapSection(int seed, bool bCreateMonsters, int xOrigin, int yOrigin, int zOrigin, int mapId)
        {
            Random random;
            random = new Random(seed);
            List<DungeonMap> mapList = new List<DungeonMap>();
            //Subtract 1 for convenience of calculations
            const int iterations = 4;
            const int intensity = 3; // intensity of pruning stray rocks
            const int scoreThresh = 4;
            const int MonsterRarity = 250;
            DungeonMap _map = new DungeonMap();
            // Set the properties of all cells to false
            _map.Initialize(_width * MapSize * MapSize, _height * MapSize);
            // Try to place as many rooms as the specified maxRooms
            for (int r = _maxRooms * MapSize; r > 0; r--)
            {
                // Determine the size and position of the room randomly
                int roomWidth = random.Next(_roomMinSize, _roomMaxSize);
                int roomHeight = random.Next(_roomMinSize, _roomMaxSize);
                int roomXPosition = random.Next(0, _width * MapSize - roomWidth - 1);
                int roomYPosition = random.Next(0, _height * MapSize - roomHeight - 1);

                // All of our rooms can be represented as Rectangles
                var newRoom = new Rectangle(roomXPosition, roomYPosition,
                  roomWidth, roomHeight);

                // Check to see if the room rectangle intersects with any other rooms
                bool newRoomIntersects = _map.Rooms.Any(room => newRoom.Intersects(room));

                // As long as it doesn't intersect add it to the list of rooms
                if (!newRoomIntersects)
                {
                    _map.Rooms.Add(newRoom);
                }
            }
            // Iterate through each room that we wanted placed 
            // call CreateRoom to make it
            foreach (Rectangle room in _map.Rooms)
            {
                CreateRoom(room, _map);
            }
            for (int r = 1; r < _map.Rooms.Count; r++)
            {
                // For all remaing rooms get the center of the room and the previous room
                int previousRoomCenterX = _map.Rooms[r - 1].Center.X;
                int previousRoomCenterY = _map.Rooms[r - 1].Center.Y;
                int currentRoomCenterX = _map.Rooms[r].Center.X;
                int currentRoomCenterY = _map.Rooms[r].Center.Y;

                if (random.Next(0, 100) > 80)
                {
                    // Give a 50/50 chance of which 'L' shaped connecting hallway to tunnel out
                    if (random.Next(0, 2) == 1)
                    {
                        CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, previousRoomCenterY, _map);
                        CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, currentRoomCenterX, _map);
                    }
                    else
                    {
                        CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, previousRoomCenterX, _map);
                        CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, currentRoomCenterY, _map);
                    }
                }
            }

            _map.ZLevel = zOrigin;

            if (zOrigin == 0)
            {
                for (int x = 0; x < _width * MapSize; x++)
                {
                    for (int y = 0; y < _height * MapSize; y++)
                    {
                        int n1 = random.Next(0, 11);

                        if (n1 > intensity)
                        {
                            _map.SetCellProperties(x, y, true, false);
                        }
                        else
                        {
                            _map.SetCellProperties(x, y, true, true);
                        }
                    }
                }
                for (int n = 0; n < iterations; n++)
                {
                    for (int x = 0; x < _width * MapSize - 2; x++)
                    {
                        for (int y = 0; y < _height * MapSize - 2; y++)
                        {
                            int score = 0;
                            if (_map.IsWalkable(x + 1, y + 0) == false)
                            {
                                score++;
                            }
                            if (_map.IsWalkable(x + 0, y + 1) == false)
                            {
                                score++;
                            }
                            if (_map.IsWalkable(x + 1, y + 1) == false)
                            {
                                score++;
                            }
                            if (_map.IsWalkable(x + 2, y + 0) == false)
                            {
                                score++;
                            }
                            if (_map.IsWalkable(x + 2, y + 1) == false)
                            {
                                score++;
                            }
                            if (_map.IsWalkable(x + 0, y + 2) == false)
                            {
                                score++;
                            }
                            if (_map.IsWalkable(x + 1, y + 2) == false)
                            {
                                score++;
                            }

                            if (score > scoreThresh)
                            {
                                _map.SetCellProperties(x, y, false, false);
                            }
                            else
                            {
                                _map.SetCellProperties(x, y, true, true);
                            }
                        }
                    }
                    for (int x = _width * MapSize - 2; x < _width * MapSize; x++)
                    {
                        for (int y = 0; y < _height * MapSize - 2; y++)
                        {
                            int score = 0;
                            if (_map.IsWalkable(x - 1, y + 0) == false)
                            {
                                score++;
                            }
                            if (_map.IsWalkable(x + 0, y + 1) == false)
                            {
                                score++;
                            }
                            if (_map.IsWalkable(x - 1, y + 1) == false)
                            {
                                score++;
                            }
                            if (_map.IsWalkable(x - 2, y + 0) == false)
                            {
                                score++;
                            }
                            if (_map.IsWalkable(x - 2, y + 1) == false)
                            {
                                score++;
                            }
                            if (_map.IsWalkable(x + 0, y + 2) == false)
                            {
                                score++;
                            }
                            if (_map.IsWalkable(x - 1, y + 2) == false)
                            {
                                score++;
                            }

                            if (score > scoreThresh)
                            {
                                _map.SetCellProperties(x, y, false, false);
                            }
                            else
                            {
                                _map.SetCellProperties(x, y, true, true);
                            }
                        }
                    }
                    for (int x = 0; x < _width * MapSize - 2; x++)
                    {
                        for (int y = _height * MapSize - 2; y < _height * MapSize; y++)
                        {
                            int score = 0;
                            if (_map.IsWalkable(x + 1, y - 0) == false)
                            {
                                score++;
                            }
                            if (_map.IsWalkable(x + 0, y - 1) == false)
                            {
                                score++;
                            }
                            if (_map.IsWalkable(x + 1, y - 1) == false)
                            {
                                score++;
                            }
                            if (_map.IsWalkable(x + 2, y - 0) == false)
                            {
                                score++;
                            }
                            if (_map.IsWalkable(x + 2, y - 1) == false)
                            {
                                score++;
                            }
                            if (_map.IsWalkable(x + 0, y - 2) == false)
                            {
                                score++;
                            }
                            if (_map.IsWalkable(x + 1, y - 2) == false)
                            {
                                score++;
                            }

                            if (score > scoreThresh)
                            {
                                _map.SetCellProperties(x, y, false, false);
                            }
                            else
                            {
                                _map.SetCellProperties(x, y, true, true);
                            }
                        }
                    }
                }
            }

            int m = 0;
            int randomMod = 0;
            int section = mapId;
            for (int i = xOrigin; i < MapSize + xOrigin; i++)
            {
                for (int j = yOrigin; j < MapSize + yOrigin; j++)
                {
                    DungeonMap map = new DungeonMap();
                    map.Initialize(_width, _height);
                    for (int k = 0; k < _width; k++)
                    {
                        for (int l = 0; l < _height; l++)
                        {
                            m++;
                            map.SetCellProperties(k, l, _map.IsTransparent((i - xOrigin) * _width + k, (j - yOrigin) * _height + l), _map.IsWalkable((i - xOrigin) * _width + k, (j - yOrigin) * _height + l));
                            if (m % MonsterRarity == randomMod)
                            {
                                if (bCreateMonsters)
                                {
                                    if (_map.IsWalkable((i - xOrigin) * _width + k, (j - yOrigin) * _height + l))
                                    {
                                        Monster newMonster = SkeletonMan.Create(1);
                                        newMonster.X = k;
                                        newMonster.Y = l;
                                        newMonster.XLevel = i;
                                        newMonster.YLevel = j;
                                        newMonster.ZLevel = zOrigin;
                                        newMonster.mapId = mapId;
                                        newMonster.ActorID = Server.GenerateRandomID();
                                        map.AddMonster(newMonster);
                                    }
                                }
                                randomMod = random.Next(0, MonsterRarity);
                            }
                        }
                    }
                    map.SpawnX = _width / 2;
                    map.SpawnY = _height / 2;
                    while (!map.IsWalkable(map.SpawnX, map.SpawnY))
                    {
                        map.SpawnX = random.Next(0, _width);
                        map.SpawnY = random.Next(0, _height);
                    }
                    map.XLevel = i;
                    map.YLevel = j;
                    map.ZLevel = zOrigin;
                    map.XOrigin = xOrigin;
                    map.YOrigin = yOrigin;
                    map.ZOrigin = zOrigin;
                    CreateStairsDown(seed, map);
                    if (zOrigin != 0) CreateStairsUp(seed, map);
                    else
                    {
                        //CreateTrees(random, map);
                        int clusters = random.Next(5, 11);
                        for (int q = 0; q < clusters; q++)
                        {
                            int[] randomNumberInts = new int[9];
                            string treeSeed = random.Next(100000000, 999999999).ToString();
                            char[] randomNumbersStrings = treeSeed.ToCharArray();
                            int v = 0;
                            foreach (char number in randomNumbersStrings)
                            {
                                Int32.TryParse(number.ToString(), out randomNumberInts[v]);
                                v++;
                            }
                            int trees = randomNumberInts[1];
                            int xCoord = randomNumberInts[1] * randomNumberInts[3] * q;
                            int yCoord = randomNumberInts[0] * randomNumberInts[2] * q;
                            for (int r = 0; r < trees; r++)
                            {
                                Terrain tree = new Terrain();
                                tree.Category = "tree";
                                tree.X = xCoord + random.Next(1, 21);
                                tree.Y = yCoord + random.Next(1, 21);
                                int n = 0;
                                while ((tree.X < 0 || tree.X > _width - 1 || tree.Y < 0 || tree.Y > _height - 1 || !map.IsWalkable(tree.X, tree.Y)) && n < 50)
                                {
                                    tree.X = xCoord + randomNumberInts[4];
                                    tree.Y = yCoord + randomNumberInts[5];
                                    n++;
                                }
                                if (n < 50)
                                {
                                    map.SetCellProperties(tree.X, tree.Y, false, false);
                                    tree.Symbol = 3;
                                    map._terrain.Add(tree);
                                }
                            }

                        }
                        CreateGrass(map);
                    }
                    map.Seed = seed;
                    map.MapId = mapId;
                    map.SectionId = section;
                    mapList.Add(map);
                    mapId++;
                }
            }

            return mapList;
        }
    }
}
