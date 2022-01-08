using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelight.Interfaces
{
    public interface IItem
    {
        int itemId { get; set; }
        int itemSeed { get; set; }
        string itemName { get; set; }
        string itemCategory { get; set; }
        string material { get; set; }
        string quality { get; set; }
        string description { get; set; }
        int weight { get; set; }
        int value { get; set; }
    }
}
