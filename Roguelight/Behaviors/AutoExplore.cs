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
    class AutoExplore : IBehavior
    {
        public static ICell previousCell = null;
        public bool Act(Actor actor, CommandSystem commandSystem)
        {
            DungeonMap dungeonMap = Engine.DungeonMap;
            PathFinder pathFinder = new PathFinder(dungeonMap);
            Path path = null;

            int n = 1;
            IEnumerable<ICell> cells;
            List<ICell> cellCandidates = new List<ICell>();
            Random random = new Random();
            int cellIndex;

            int i = 0;
            bool bCellAccepted = false;
            while (n < 180 && bCellAccepted == false)
            {
                i++;
                n++;
                if(previousCell != null)
                {
                    if (dungeonMap.IsExplored(previousCell.X, previousCell.Y))
                    {
                        cells = dungeonMap.GetCellsInCircle(actor.X, actor.Y, n);
                        foreach (ICell cell in cells)
                        {
                            if (cell.IsExplored == false && cell.IsWalkable == true)
                            {
                                cellCandidates.Add(cell);
                            }
                        }
                        //Randomize the cell we select so that the actor doesn't get stuck between two reoccuring cell paths.
                        if (cellCandidates.Count > 2)
                        {
                            cellIndex = random.Next(0, cellCandidates.Count - 1);
                            try
                            {
                                path = pathFinder.ShortestPath(dungeonMap.GetCell(actor.X, actor.Y), cellCandidates[cellIndex]);
                                commandSystem.RegisterMovement(actor, path.StepForward());
                                previousCell = cellCandidates[cellIndex];
                                bCellAccepted = true;
                            }
                            catch
                            {
                                bCellAccepted = false;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            path = pathFinder.ShortestPath(dungeonMap.GetCell(actor.X, actor.Y), previousCell);
                            commandSystem.RegisterMovement(actor, path.StepForward());
                            return true;
                        }
                        catch
                        {
                            Engine.MessageLog.Add($"{actor.Name} growls in frustration_0");
                            return false;
                        }
                    }
                }
                else
                {
                    cells = dungeonMap.GetCellsInCircle(actor.X, actor.Y, n);
                    foreach (ICell cell in cells)
                    {
                        if (cell.IsExplored == false && cell.IsWalkable == true)
                        {
                            cellCandidates.Add(cell);
                        }
                    }
                    //Randomize the cell we select so that the actor doesn't get stuck between two reoccuring cell paths.
                    if (cellCandidates.Count > 2)
                    {

                        cellIndex = random.Next(0, cellCandidates.Count - 1);
                        try
                        {
                            path = pathFinder.ShortestPath(dungeonMap.GetCell(actor.X, actor.Y), cellCandidates[cellIndex]);
                            commandSystem.RegisterMovement(actor, path.StepForward());
                            previousCell = cellCandidates[cellIndex];
                            bCellAccepted = true;
                        }
                        catch
                        {
                            bCellAccepted = false;
                        }
                    }
                }
            }
            if(bCellAccepted == false)
            {
                Engine.MessageLog.Add($"{actor.Name} has no where else to explore_0");
                return false;
            }
            return true;
        }
    }
}
