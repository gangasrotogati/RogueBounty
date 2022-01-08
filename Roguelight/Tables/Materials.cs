using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelight;

namespace Roguelight.Tables
{
    public class Materials
    {
        public string[] materials = new string[6];

        public Materials()
        {
            materials[0] = "Wooden";
            materials[1] = "Iron";
            materials[2] = "Steel";
            materials[3] = "Cloth";
            materials[4] = "Leather";
            materials[5] = "Stone";
        }
    }
}
