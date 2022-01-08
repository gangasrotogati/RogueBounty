using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelight.Core;
using RogueSharp;

namespace Roguelight.Behaviors
{
    public class AutoMeleeAttack
    {
        public void Act(int? target, Player player)
        {

            DungeonMap dungeonMap = Engine.DungeonMap;
            FieldOfView playerFov = new FieldOfView(dungeonMap);
            ICell stepForward;
            Monster targetMonster = new Monster();
            bool IsInFov = false;

            foreach(Monster monster in Client.MonsterList)
            {
                if(monster.ActorID == target)
                {
                    targetMonster = monster;
                }
            }

            playerFov.ComputeFov(player.X, player.Y, player.Awareness, true);

            if(playerFov.IsInFov(targetMonster.X, targetMonster.Y))
            {
                IsInFov = true;
            }

            int nextStepX = 0;
            int nextStepY = 0;

            try
            {
                stepForward = Engine.GetPath(player, targetMonster);
                nextStepX = stepForward.X;
                nextStepY = stepForward.Y;
            }
            catch (NoMoreStepsException)//NoMoreStepsException
            {
            }

            if (nextStepX != 0 || nextStepY != 0)
            {
                int dx = nextStepX - player.X;
                int dy = nextStepY - player.Y;

                if (dx == 0 && dy == -1)
                {
                    try
                    {
                        Client.CommandSystem.MovePlayer(Direction.Up);
                    }
                    catch (NoMoreStepsException)
                    {
                    }
                }
                if (dx == 0 && dy == 1)
                {
                    try
                    {
                        Client.CommandSystem.MovePlayer(Direction.Down);
                    }
                    catch (NoMoreStepsException)
                    {
                    }
                }
                if (dx == -1 && dy == -1)
                {
                    try
                    {
                        Client.CommandSystem.MovePlayer(Direction.UpLeft);
                    }
                    catch (NoMoreStepsException)
                    {
                    }
                }
                if (dx == -1 && dy == 1)
                {
                    try
                    {
                        Client.CommandSystem.MovePlayer(Direction.DownLeft);
                    }
                    catch (NoMoreStepsException)
                    {
                    }
                }
                if (dx == 1 && dy == -1)
                {
                    try
                    {
                        Client.CommandSystem.MovePlayer(Direction.UpRight);
                    }
                    catch (NoMoreStepsException)
                    {
                    }
                }
                if (dx == 1 && dy == 1)
                {
                    try
                    {
                        Client.CommandSystem.MovePlayer(Direction.DownRight);
                    }
                    catch (NoMoreStepsException)
                    {
                    }
                }
                if (dx == 1 && dy == 0)
                {
                    try
                    {
                        Client.CommandSystem.MovePlayer(Direction.Right);
                    }
                    catch (NoMoreStepsException)
                    {
                    }
                }
                if (dx == -1 && dy == 0)
                {
                    try
                    {
                        Client.CommandSystem.MovePlayer(Direction.Left);
                    }
                    catch (NoMoreStepsException)
                    {
                    }
                }
            }
        }
        
    }
}
