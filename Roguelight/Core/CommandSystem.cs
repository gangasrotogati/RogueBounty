using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using RogueSharp;

namespace Roguelight.Core
{
    public class CommandSystem
    {
        public void MovePlayer( Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: SocketClient.SendData($"{Client.playerSeed}.UP.<ACTION>"); break;
                case Direction.Down: SocketClient.SendData($"{Client.playerSeed}.DOWN.<ACTION>"); break;
                case Direction.Left: SocketClient.SendData($"{Client.playerSeed}.LEFT.<ACTION>"); break;
                case Direction.Right: SocketClient.SendData($"{Client.playerSeed}.RIGHT.<ACTION>"); break;
                case Direction.UpLeft: SocketClient.SendData($"{Client.playerSeed}.UPLEFT.<ACTION>"); break;
                case Direction.UpRight: SocketClient.SendData($"{Client.playerSeed}.UPRIGHT.<ACTION>"); break;
                case Direction.DownLeft: SocketClient.SendData($"{Client.playerSeed}.DOWNLEFT.<ACTION>"); break;
                case Direction.DownRight: SocketClient.SendData($"{Client.playerSeed}.DOWNRIGHT.<ACTION>"); break;
                case Direction.DownStairs: SocketClient.SendData($"{Client.playerSeed}.STAIRSDOWN.<ACTION>"); break;
                case Direction.UpStairs: SocketClient.SendData($"{Client.playerSeed}.STAIRSUP.<ACTION>"); break;
            }
        }
        public void RegisterMovement(Actor actor, ICell cell)
        {
            int dx = cell.X - actor.X;
            int dy = cell.Y - actor.Y;

            if (dx == -1 && dy == 0)
            {
                SocketClient.SendData($"{Client.playerSeed}.LEFT.<ACTION>");
            }
            else if (dx == 1 && dy == 0)
            {
                SocketClient.SendData($"{Client.playerSeed}.RIGHT.<ACTION>");
            }
            else if(dx == 0 && dy == 1)
            {
                SocketClient.SendData($"{Client.playerSeed}.DOWN.<ACTION>");
            }
            else if(dx == 0 && dy == -1)
            {
                SocketClient.SendData($"{Client.playerSeed}.UP.<ACTION>");
            }
            else if(dx == 1 && dy == -1)
            {
                SocketClient.SendData($"{Client.playerSeed}.UPRIGHT.<ACTION>");
            }
            else if(dx == 1 && dy == 1)
            {
                SocketClient.SendData($"{Client.playerSeed}.DOWNRIGHT.<ACTION>");
            }
            else if(dx == -1 && dy == -1)
            {
                SocketClient.SendData($"{Client.playerSeed}.UPLEFT.<ACTION>");
            }
            else if(dx == -1 && dy == 1)
            {
                SocketClient.SendData($"{Client.playerSeed}.DOWNLEFT.<ACTION>");
            }
        }
    }
}
