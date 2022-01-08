using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;

namespace Roguelight.Core
{
    public class ServerEngine
    {
        private int i = 0;
        public RLRootConsole rootConsole;
        public static List<DungeonMap> DungeonMaps = new List<DungeonMap>();
        public static DungeonMap DungeonMap { get; set; }

        public ServerEngine(RLRootConsole console)
        {
            rootConsole = console;
            rootConsole.Render += Render;
            rootConsole.Update += Update;
        }

        public void Render(object sender, UpdateEventArgs e)
        {
            //Clean the slate
            rootConsole.Clear();


            rootConsole.Draw();
        }

        public void Update(object sender, UpdateEventArgs e)
        {
            i++;
            //Check for disconnected clients
            //This will tell the client not to render the character who is offline, although they will still exist in the server list as of now.
            if (i % 100 == 0)
            {
                foreach (Player player in Server.PlayerList)
                {
                    if (player.isOnline == 1)
                    {
                        player.isOnline = 0;
                    }
                }
            }
            if (i % 100 == 50)
            {
                foreach (Player player in Server.PlayerList)
                {
                    if (player.isOnline == 0)
                    {
                        Server.MapList[player.ZLevel].SetCellProperties(player.X, player.Y, false, true);
                        player.isOnline = -1;
                    }
                }
            }
            SocketListener.Listen();
        }
    }
}
