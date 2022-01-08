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
    public class StandardMoveAndAttack : IBehavior
    {
        public bool Act(Monster monster, Player player, int nextStepX, int nextStepY, bool bIsInFov)
        {
            DungeonMap dungeonMap = Server.MapList[monster.mapId];
            bool bIsPlayerTargeted = false;

            // If the monster has not been alerted, compute a field-of-view
            // Use the monster's Awareness value for the distance in the FoV check
            // If the player is in the monster's FoV then alert it
            // Add a message to the MessageLog regarding this alerted status

            monster.CurrentTarget = monster.GetNewTarget();

            if (bIsInFov)
            {
                if (monster.CurrentTarget == null)
                {
                    if (!monster.SecondsAlerted.HasValue)
                    {
                        Server.serverMessage = ($"{monster.Name} is eager to fight {player.Name}_0");
                    }
                }
                else if (monster.CurrentTarget == player.ActorID)
                {
                    monster.SecondsAlerted = 1;
                }
                //Check that the monster is targeting this player
                foreach (int[] target in monster.TargetsList)
                {
                    if (target[0] == player.ActorID)
                    {
                        bIsPlayerTargeted = true;
                    }
                }
                if (!bIsPlayerTargeted)
                {
                    monster.TargetsList.Add(new int[] { player.ActorID, 1 });
                }
            }

            

            if (monster.SecondsAlerted.HasValue && monster.CurrentTarget == player.ActorID)
            {
                // In the case that there was a path, tell the CommandSystem to move the monster
                if (nextStepX != 0 || nextStepY != 0)
                {
                    try
                    {
                        Server.MoveMonster(monster, nextStepX, nextStepY, bIsInFov);
                    }
                    catch (NoMoreStepsException)
                    {
                        Server.serverMessage = ($"{monster.Name} growls in frustration_0");
                    }
                }

                // Lose alerted status every 15 turns. 
                // As long as the player is still in FoV the monster will stay alert
                // Otherwise the monster will quit chasing the player.
                if (monster.SecondsAlerted > 15)
                {
                    monster.SecondsAlerted = null;
                    monster.CurrentTarget = null;
                }
                if (monster.TargetsList.Count == 0)
                {
                    monster.bIsAtRest = true;
                }
            }

            return true;
        }
    }
}
