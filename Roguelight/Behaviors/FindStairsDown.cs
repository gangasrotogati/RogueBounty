using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelight.Core;
using Roguelight.Interfaces;
using RogueSharp;

namespace Roguelight.Behaviors
{
    class FindStairsDown : IBehavior
    {
        public bool Act(Actor actor, CommandSystem commandSystem)
        {
            DungeonMap dungeonMap = Engine.DungeonMap;
            PathFinder pathFinder = new PathFinder(dungeonMap);
            Path path = null;
            if(dungeonMap.IsExplored(dungeonMap.StairsDown.X, dungeonMap.StairsDown.Y))
            {
                try
                {
                    path = pathFinder.ShortestPath(dungeonMap.GetCell(actor.X, actor.Y), dungeonMap.GetCell(dungeonMap.StairsDown.X, dungeonMap.StairsDown.Y));
                    while (actor.X != dungeonMap.StairsDown.X || actor.Y != dungeonMap.StairsDown.Y)
                    {
                        commandSystem.RegisterMovement(actor, path.StepForward());
                        return true;
                    }
                }
                catch
                {
                    Engine.MessageLog.Add($"{actor.Name} growls in frustration");
                    return false;
                }
            }
            return false;
        }
    }
}
