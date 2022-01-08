using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelight.Items;
using RLNET;

namespace Roguelight.Core
{
    public class Inventory
    {
        public int inventoryID { get; set; }
        public List<int> itemsList { get; set; }
        private static Random random = new Random();
        public Inventory()
        {
            inventoryID = Server.GenerateSeed();
            itemsList = new List<int>();
        }
    }
}
