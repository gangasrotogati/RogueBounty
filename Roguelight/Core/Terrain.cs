using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using RogueSharp;

namespace Roguelight.Core
{
    public class Terrain
    {
        public string Category { get; set; }
        public RLColor Color
        {
            get; set;
        }
        public int Symbol
        {
            get; set;
        }
        public int X
        {
            get; set;
        }
        public int Y
        {
            get; set;
        }

        public void Draw(RLConsole console, IMap map)
        {
            if (!map.GetCell(X, Y).IsExplored)
            {
                return;
            }

            if (map.IsInFov(X, Y))
            {
                switch (Category)
                {
                    case "tree": Color = Colors.TreeFov; break;
                    case "grass": Color = Colors.GrassFov; break; 
                }
            }
            else
            {
                switch (Category)
                {
                    case "tree": Color = Colors.Tree; break;
                    case "grass": Color = Colors.Grass; break;
                }
            }

            console.Set(X, Y, Color, null, Symbol);
        }
    }
}
