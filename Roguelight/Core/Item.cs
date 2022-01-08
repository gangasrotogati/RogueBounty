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
    public class Item: IItem, IDrawable
    {
        public int itemId { get; set; }
        public int itemSeed { get; set; }
        public string itemName { get; set; }
        public string itemCategory { get; set; }
        public string itemSubcategory { get; set; }
        public string material { get; set; }
        public string quality { get; set; }
        public string description { get; set; }
        public int weight { get; set; }
        public int value { get; set; }

        //IDrawable
        public RLColor Color { get; set; }
        public int Symbol { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int XLevel { get; set; }
        public int YLevel { get; set; }
        public int ZLevel { get; set; }
        public int? mapId { get; set; }
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

    }

}
