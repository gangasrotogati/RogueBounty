using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using RogueSharp;

namespace Roguelight.Core
{
    public class DungeonMap : Map
    {
        //This list of rectangles will be used to generate some simple rooms
        public Stairs StairsUp { get; set; }
        public Stairs StairsDown { get; set; }
        public List<Rectangle> Rooms;
        public List<Monster> _monsters;
        public List<Terrain> _terrain;
        public int SpawnX {get; set;}
        public int SpawnY { get; set; }
        public int Seed { get; set; }
        public int MapId { get; set; }
        public int XLevel { get; set; }
        public int YLevel { get; set; }
        public int ZLevel { get; set; }
        public int XOrigin { get; set; }
        public int YOrigin { get; set; }
        public int ZOrigin { get; set; }
        public int SectionId { get; set; }

        private Random random;
        private int randomizer;
        private int cycles = 0;
        public DungeonMap()
        {
            // Initialize the list of rooms when we create a new DungeonMap
            Rooms = new List<Rectangle>();
            _monsters = new List<Monster>();
            _terrain = new List<Terrain>();
            random = new Random();
            XLevel = -999;
            YLevel = -999;
            ZLevel = -999;
            MapId = -999;
            SectionId = -999;
        }

        // The Draw method will be called each time the map is updated
        // It will render all of the symbols/colors for each cell to the map sub console
        public void Draw(RLConsole mapConsole)
        {
            foreach (Cell cell in GetAllCells())
            {
                SetConsoleSymbolForCell(mapConsole, cell);
            }
            cycles++;
            if (StairsUp != null) StairsUp.Draw(mapConsole, this);
            if (StairsDown != null) StairsDown.Draw(mapConsole, this);
            foreach(Terrain terrain in _terrain)
            {
                terrain.Draw(mapConsole, this);
            }
        }

        private void SetConsoleSymbolForCell(RLConsole console, Cell cell)
        {
            // When we haven't explored a cell yet, we don't want to draw anything
            if (!cell.IsExplored)
            {
                if(ZLevel == 0)
                {
                        console.SetBackColor(cell.X, cell.Y, Swatch.Beige);
                }
                return;
            }

            // When a cell is currently in the field-of-view it should be drawn with ligher colors
            if (IsInFov(cell.X, cell.Y))
            {
                // Choose the symbol to draw based on if the cell is walkable or not
                // '.' for floor and '#' for walls
                if (cell.IsWalkable)
                {
                    console.Set(cell.X, cell.Y, Colors.FloorFov, Colors.FloorBackgroundFov, 1);
                }
                else
                {
                    console.Set(cell.X, cell.Y, Colors.WallFov, Colors.WallBackgroundFov, 2);
                }
            }
            // When a cell is outside of the field of view draw it with darker colors
            else
            {
                if (cell.IsWalkable)
                {
                    console.Set(cell.X, cell.Y, Colors.Floor, Colors.FloorBackground, 1);
                }
                else
                {
                    console.Set(cell.X, cell.Y, Colors.Wall, Colors.WallBackground, 2);
                }
            }
        }
        public void UpdatePlayerFieldOfView(int x, int y, int awareness)
        {
            // Compute the field-of-view based on the player's location and awareness
            ComputeFov(x, y, awareness, true);
            // Mark all cells in field-of-view as having been explored
            foreach (Cell cell in GetAllCells())
            {
                if (IsInFov(cell.X, cell.Y))
                {
                    SetCellProperties(cell.X, cell.Y, cell.IsTransparent, cell.IsWalkable, true);
                }
            }
        }

        public bool CanMoveDownToNextLevel(Player Player)
        {
            return StairsDown.X == Player.X && StairsDown.Y == Player.Y;
        }
        public bool CanMoveUpToNextLevel(Player Player)
        {
            return StairsUp.X == Player.X && StairsUp.Y == Player.Y && Player.ZLevel != 0;
        }
        public void AddMonster(Monster monster)
        {
            if(MapGenerator.IsServer == true)
            {
                monster.ActorID = Server.random.Next(100000000, 999999999);
                _monsters.Add(monster);
                // After adding the monster to the map make sure to make the cell not walkable
                SetCellProperties(monster.X, monster.Y, false, false);
            }
        }
        public void RemoveMonster(Monster monster)
        {
            _monsters.Remove(monster);
            // After removing the monster from the map, make sure the cell is walkable again
            SetCellProperties(monster.X, monster.Y, false, true);
        }
        public Monster GetMonsterAt(int x, int y)
        {
            return _monsters.FirstOrDefault(m => m.X == x && m.Y == y);
        }
        public Point GetRandomWalkableLocationInRoom(Rectangle room)
        {
            if (DoesRoomHaveWalkableSpace(room))
            {
                for (int i = 0; i < 100; i++)
                {
                    Random random = new Random();
                    int x = random.Next(1, room.Width - 2) + room.X;
                    int y = random.Next(1, room.Height - 2) + room.Y;
                    if (IsWalkable(x, y))
                    {
                        return new Point(x, y);
                    }
                }
            }
            return default(Point);
        }
        // Iterate through each Cell in the room and return true if any are walkable
        public bool DoesRoomHaveWalkableSpace(Rectangle room)
        {
            for (int x = 1; x <= room.Width - 2; x++)
            {
                for (int y = 1; y <= room.Height - 2; y++)
                {
                    if (IsWalkable(x + room.X, y + room.Y))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
