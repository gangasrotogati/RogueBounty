using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelight.Tables
{
    public class Qualities
    {
        public string[] qualities = new string[6];

        public Qualities()
        {
            qualities[0] = "";
            qualities[1] = "Broken";
            qualities[2] = "Weighted";
            qualities[3] = "Fine";
            qualities[4] = "Peerless";
            qualities[5] = "Crude";
        }
    }
}
