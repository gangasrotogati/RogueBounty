using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelight.Tables
{
    public class ItemCategories
    {
        public string[] itemCategories = new string[6];
        public ItemCategories()
        {
            itemCategories[0] = "meleeWeapon";
            itemCategories[1] = "rangedWeapon";
            itemCategories[2] = "magicWeapon";
            itemCategories[3] = "clothing";
            itemCategories[4] = "consumable";
            itemCategories[5] = "container";
        }
    }
}
