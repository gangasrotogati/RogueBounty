using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Roguelight.Core
{
    public class SocketListener
    {
        public static void StartServer()
        {
            // Get Host IP Address that is used to establish a connection  
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
            // If a host has multiple addresses, you will get a list of addresses  
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);


            try
            {

                // Create a Socket that will use Tcp protocol      
                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // A Socket must be associated with an endpoint using the Bind method  
                listener.Bind(localEndPoint);
                // Specify how many requests a Socket can listen before it gives Server busy response.  
                // We will listen 10 requests at a time  
                listener.Listen(10);
                int i = 0;
                while (true)
                {
                    i++;
                    Console.WriteLine("Waiting for a connection...");
                    Socket handler = listener.Accept();

                    // Incoming data from the client.    
                    string data = null;
                    byte[] bytes = null;

                    //Response to the client
                    string response = "";

                    while (true)
                    {
                        Server.Timer.CheckTheClock();
                        bytes = new byte[4096]; //previously 1024
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }

                    Console.WriteLine("Text received : {0}", data);
                    if (data.IndexOf("<REGISTER>") > -1)
                    {
                        response = Server.RegisterPlayer();
                    }
                    if (data.IndexOf("<ACTION>") > -1)
                    {
                        Server.RegisterAction(data);
                    }
                    if (data.IndexOf("<UPDATE>") > -1)
                    {
                        response = Server.UpdateClient(data);
                    }
                    if (data.IndexOf("<MAP>") > -1)
                    {
                        response = Server.UpdateDungeonMap(data);
                    }
                    if (data.IndexOf("<INVENTORY>") > -1)
                    {
                        response = Server.GetInventoryItems(data);
                    }
                    if (data.IndexOf("<DROP>") > -1)
                    {
                        Server.DropItem(data);
                    }
                    if (data.IndexOf("<GET>") > -1)
                    {
                        Server.GiveItem(data);
                    }
                    if (data.IndexOf("<EQUIPT>") > -1)
                    {
                        Server.EquiptItem(data);
                    }
                    if (data.IndexOf("<EQUIPMENT>") > -1)
                    {
                        response = Server.GetEquipmentItems(data);
                    }
                    if (data.IndexOf("<DEQUIPT>") > -1)
                    {
                        Server.DequiptItem(data);
                    }
                    if (data.IndexOf("<WAKE>") > -1)
                    {
                        Server.WakeMonster(data);
                    }

                    //Check for disconnected clients
                    //This will tell the client not to render the character who is offline, although they will still exist in the server list as of now.
                    if (i%100 == 0)
                    {
                        foreach(Player player in Server.PlayerList)
                        {
                            if(player.isOnline == 1)
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
                                Server.MapList[player.mapId].SetCellProperties(player.X, player.Y, false, true);
                                player.isOnline = -1;
                            }
                        }
                    }

                    byte[] msg = Encoding.ASCII.GetBytes(response);
                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();

                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\n Press any key to continue...");
            Console.ReadKey();
        }
    }
}
