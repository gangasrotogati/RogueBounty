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
    class FindStairsUp : IBehavior
    {
        public bool Act(Actor actor, CommandSystem commandSystem)
        {
            DungeonMap dungeonMap = Engine.DungeonMap;
            PathFinder pathFinder = new PathFinder(dungeonMap);
            Path path = null;
            if (dungeonMap.IsExplored(dungeonMap.StairsUp.X, dungeonMap.StairsUp.Y))
            {
                try
                {
                    path = pathFinder.ShortestPath(dungeonMap.GetCell(actor.X, actor.Y), dungeonMap.GetCell(dungeonMap.StairsUp.X, dungeonMap.StairsUp.Y));
                    while (actor.X != dungeonMap.StairsUp.X || actor.Y != dungeonMap.StairsUp.Y)
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
