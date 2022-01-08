using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelight.Core;

namespace Roguelight.Items
{
    public class Weapon: Item
    {
        public int skillType { get; set; }
        public int wieldType { get; set; }
    }
}
