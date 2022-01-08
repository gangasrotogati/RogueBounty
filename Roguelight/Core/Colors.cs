using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;

namespace Roguelight.Core
{
    public class Colors
    {
        public static RLColor FloorBackground = RLColor.Black;
        public static RLColor Floor = Swatch.MuaveDarker;
        public static RLColor FloorBackgroundFov = Swatch.DbDark;
        public static RLColor FloorFov = Swatch.Muave;

        public static RLColor WallBackground = Swatch.SecondaryDarkest;
        public static RLColor Wall = Swatch.Secondary;
        public static RLColor WallBackgroundFov = Swatch.SecondaryDarker;
        public static RLColor WallFov = Swatch.SecondaryLighter;

        public static RLColor TextHeading = RLColor.White;
        public static RLColor Text = Swatch.DbLight;
        public static RLColor Gold = Swatch.DbSun;
        public static RLColor Player = Swatch.White;
        public static RLColor KoboldColor = Swatch.DbBrightWood;

        public static RLColor DoorBackground = Swatch.ComplimentDarkest;
        public static RLColor Door = Swatch.ComplimentLighter;
        public static RLColor DoorBackgroundFov = Swatch.ComplimentDarker;
        public static RLColor DoorFov = Swatch.ComplimentLightest;

        public static RLColor TreeFov = Swatch.White;
        public static RLColor Tree = Swatch.Gray;
        public static RLColor GrassFov = Swatch.Beige;
        public static RLColor Grass = Swatch.BeigeDarker;
    }
}
