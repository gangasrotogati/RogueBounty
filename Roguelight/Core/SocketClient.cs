using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using RLNET;

namespace Roguelight.Core
{
    public class SocketClient
    {
        /*public static int Main(String[] args)
        {
            string input;
            while (true)
            {
                Console.Write("Message: ");
                input = Console.ReadLine();
                SendData(input);
            }

        }*/


        public static string SendData(string input)
        {
            byte[] bytes = new byte[4096];//previously 1024
            string response = "";

            try
            {
                // Connect to a Remote server  
                // Get Host IP Address that is used to establish a connection  
                // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
                // If a host has multiple addresses, you will get a list of addresses  
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                // Create a TCP/IP  socket.    
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.    
                try
                {
                    // Connect to Remote EndPoint  
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    // Encode the data string into a byte array.
                    byte[] msg = Encoding.ASCII.GetBytes(input + "<EOF>");

                    // Send the data through the socket.    
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.    
                    int bytesRec = sender.Receive(bytes);
                    Console.WriteLine("Echoed test = {0}",
                        Encoding.ASCII.GetString(bytes, 0, bytesRec));
                    response = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    // Release the socket.    
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return response;
        }
        public static void GeneratePlayerData(string data)
        {
            List<Player> PlayerData = new List<Player>();
            List<Monster> MonsterData = new List<Monster>();
            List<Item> ItemData = new List<Item>();
            string[] chunks = data.Split(',');
            for (int i = 0; i < chunks.Length; i++) //Each chunk is data for a different character on the same level as the user
            {
                if (i < chunks.Length - 2)
                {
                    string[] subchunks = chunks[i].Split('.'); //0.ID 1.X 2.Y 3.isOnline 4.Health 5.MaxHealth 6.symbol 7.name 8.player 9.Xlevel 10.YLevel 11.ZLevel 12.MapId
                    if (subchunks.Length >= 8 && subchunks[8] == "player")
                    { 
                    Player PlayerInstance = new Player();
                    //length minus the Server Message and the EOF
                        int result;
                        Int32.TryParse(subchunks[0], out result);
                        PlayerInstance.ActorID = result;
                        Int32.TryParse(subchunks[1], out result);
                        PlayerInstance.X = result;
                        Int32.TryParse(subchunks[2], out result);
                        PlayerInstance.Y = result;
                        Int32.TryParse(subchunks[3], out result);
                        PlayerInstance.isOnline = result;
                        Int32.TryParse(subchunks[4], out result);
                        PlayerInstance.Health = result;
                        Int32.TryParse(subchunks[5], out result);
                        PlayerInstance.MaxHealth = result;
                        Int32.TryParse(subchunks[6], out result);
                        PlayerInstance.Symbol = result;
                        PlayerInstance.Name = subchunks[7];
                        Int32.TryParse(subchunks[9], out result);
                        PlayerInstance.XLevel = result;
                        Int32.TryParse(subchunks[10], out result);
                        PlayerInstance.YLevel = result;
                        Int32.TryParse(subchunks[11], out result);
                        PlayerInstance.ZLevel = result;
                        Int32.TryParse(subchunks[12], out result);
                        PlayerInstance.mapId = result;

                        PlayerData.Add(PlayerInstance);
                    }
                    if (subchunks.Length >= 8 && subchunks[8] == "monster") //0.ID 1.X 2.Y 3.Health 4.MaxHealth 5.symbol 6.name 7.awareness 8.monster 9.Xlevel 10.YLevel 11.ZLevel 12.MapId
                    {
                        Monster MonsterInstance = new Monster();
                        //length minus the Server Message and the EOF
                        int result;
                        Int32.TryParse(subchunks[0], out result);
                        MonsterInstance.ActorID = result;
                        Int32.TryParse(subchunks[1], out result);
                        MonsterInstance.X = result;
                        Int32.TryParse(subchunks[2], out result);
                        MonsterInstance.Y = result;
                        Int32.TryParse(subchunks[3], out result);
                        MonsterInstance.Health = result;
                        Int32.TryParse(subchunks[4], out result);
                        MonsterInstance.MaxHealth = result;
                        Int32.TryParse(subchunks[5], out result);
                        MonsterInstance.Symbol = result;
                        MonsterInstance.Name = subchunks[6];
                        Int32.TryParse(subchunks[7], out result);
                        MonsterInstance.Awareness = result;
                        Int32.TryParse(subchunks[9], out result);
                        MonsterInstance.XLevel = result;
                        Int32.TryParse(subchunks[10], out result);
                        MonsterInstance.YLevel = result;
                        Int32.TryParse(subchunks[11], out result);
                        MonsterInstance.ZLevel = result;
                        Int32.TryParse(subchunks[12], out result);
                        MonsterInstance.mapId = result;
                        Int32.TryParse(subchunks[13], out result);
                        MonsterInstance.Speed = result;
                        if (subchunks[14] == "True") MonsterInstance.bIsAtRest = true; else MonsterInstance.bIsAtRest = false;
                        MonsterInstance.Color = RLColor.White;
                        MonsterData.Add(MonsterInstance);
                    }
                    if(subchunks[5] == "item")
                    {
                        Item itemInstance = new Item();
                        int result;
                        Int32.TryParse(subchunks[0], out result);
                        itemInstance.itemId = result;
                        Int32.TryParse(subchunks[1], out result);
                        itemInstance.itemSeed = result;
                        itemInstance = ItemGenerator.GenerateItem(itemInstance.itemId, result);
                        Int32.TryParse(subchunks[2], out result);
                        itemInstance.X = result;
                        Int32.TryParse(subchunks[3], out result);
                        itemInstance.Y = result;
                        Int32.TryParse(subchunks[4], out result);
                        itemInstance.mapId = result;
                        ItemData.Add(itemInstance);
                    }
                }

                //Handle incoming messages
                if (i == chunks.Length - 2 && chunks[i] != "")
                {
                    string[] subChunks = chunks[i].Split('.');
                    foreach(string subChunk in subChunks)
                    {
                        Engine.MessageLog.Add(subChunk);
                    }
                }
            }
            Client.PlayerList = PlayerData;
            Client.MonsterList = MonsterData;
            Client.MapItemList = ItemData;
        }

        public static string RegisterPlayer()
        {
            return SendData("<REGISTER>");
        }

        public static void RequestDungeonMap(int mapId)
        {
            string response = SendData($"{mapId},<MAP>");
            string[] chunks = response.Split('.');
            Int32.TryParse(chunks[0], out int seed);
            MapGenerator mapGenerator = new MapGenerator(Client._mapWidth, Client._mapHeight, 400, 13, 7);
            int ZOrigin = Int32.Parse(chunks[1]);
            int XOrigin = Int32.Parse(chunks[2]);
            int YOrigin = Int32.Parse(chunks[3]);
            int SectionId = Int32.Parse(chunks[4]);
            List<DungeonMap> newMaps = mapGenerator.GenerateMapSection(seed, false, XOrigin, YOrigin, ZOrigin, SectionId);

            foreach(DungeonMap map in newMaps)
            {
                Engine.DungeonMaps.Add(map);
            }
        }
    }
}
