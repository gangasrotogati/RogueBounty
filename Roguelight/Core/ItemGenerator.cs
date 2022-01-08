using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelight.Core
{
    public class ItemGenerator
    {
        const int NumberOfItemCategories = 5;
        const int NumberOfConsumables = 1;
        const int NumberOfConsumableMaterials = 5;
        const int NumberOfConsumableQualities = 5;
        const int NumberOfAmmos = 2;
        const int NumberOfAmmoMaterials = 5;
        const int NumberOfAmmoQualities = 5;
        const int NumberOfArmors = 5;
        const int NumberOfArmorMaterials = 5;
        const int NumberOfArmorQualities = 5;
        const int NumberOfRanges = 5;
        const int NumberOfRangedMaterials = 4;
        const int NumberOfRangedQualities = 4;
        const int NumberOfMelees = 5;
        const int NumberOfMeleeMaterials = 4;
        const int NumberOfMeleeQualities = 4;
        public static Item GenerateItem(int itemId, int itemSeed)
        {
            Item item = new Item();
            item.itemId = itemId;
            item.itemSeed = itemSeed;
            Random random = new Random(itemSeed);
            int itemValue = random.Next(0, NumberOfItemCategories);
            switch (itemValue)
            {
                case 0: item.itemCategory = "consumable"; break;
                case 1: item.itemCategory = "ammo"; break;
                case 2: item.itemCategory = "armor"; break;
                case 3: item.itemCategory = "ranged"; break;
                case 4: item.itemCategory = "melee"; break;
            }
            switch (item.itemCategory)
            {
                case "consumable":
                    {
                        itemValue = random.Next(0, NumberOfConsumables);
                        switch (itemValue)
                        {
                            case 0: item.itemName = "health philter"; item.itemSubcategory = "potion"; break;
                        }
                        itemValue = random.Next(0, NumberOfConsumableMaterials);
                        switch (itemValue)
                        {
                            case 0: item.material = "glass"; break;
                            case 1: item.material = "tin"; break;
                            case 2: item.material = "ceramic"; break;
                            case 3: item.material = "plastic"; break;
                            case 4: item.material = "paper"; break;
                        }
                        itemValue = random.Next(0, NumberOfConsumableQualities);
                        switch (itemValue)
                        {
                            case 0: item.quality = "Cracked"; break;
                            case 1: item.quality = "Tarnished"; break;
                            case 2: item.quality = "Simple"; break;
                            case 3: item.quality = "Grand"; break;
                            case 4: item.quality = "Decorated"; break;
                        }
                        break;
                    }
                case "ammo":
                    {
                        itemValue = random.Next(0, NumberOfAmmos);
                        switch (itemValue)
                        {
                            case 0: item.itemName = "arrows"; item.itemSubcategory = "arrows"; break;
                            case 1: item.itemName = "bullets"; item.itemSubcategory = "bullets"; break;
                        }
                        itemValue = random.Next(0, NumberOfAmmoMaterials);
                        switch (itemValue)
                        {
                            case 0: item.material = "wooden"; break;
                            case 1: item.material = "lead"; break;
                            case 2: item.material = "steel"; break;
                            case 3: item.material = "bone"; break;
                            case 4: item.material = "carbon"; break;
                        }
                        itemValue = random.Next(0, NumberOfAmmoQualities);
                        switch (itemValue)
                        {
                            case 0: item.quality = "Ruined"; break;
                            case 1: item.quality = "Average"; break;
                            case 2: item.quality = "Reliable"; break;
                            case 3: item.quality = "Deadly"; break;
                            case 4: item.quality = "Perfect"; break;
                        }
                        break;
                    }
                case "armor":
                    {
                        itemValue = random.Next(0, NumberOfArmors);
                        switch (itemValue)
                        {
                            case 0: item.itemName = "hat"; item.itemSubcategory = "headArmor"; break;
                            case 1: item.itemName = "vest"; item.itemSubcategory = "chestArmor"; break;
                            case 2: item.itemName = "robe"; item.itemSubcategory = "chestArmor"; break; 
                            case 3: item.itemName = "gloves"; item.itemSubcategory = "handArmor"; break;
                            case 4: item.itemName = "boots"; item.itemSubcategory = "footArmor"; break;
                        }
                        itemValue = random.Next(0, NumberOfArmorMaterials);
                        switch (itemValue)
                        {
                            case 0: item.material = "cloth"; break;
                            case 1: item.material = "leather"; break;
                            case 2: item.material = "iron"; break;
                            case 3: item.material = "steel"; break;
                            case 4: item.material = "kevlar"; break;
                        }
                        itemValue = random.Next(0, NumberOfArmorQualities);
                        switch (itemValue)
                        {
                            case 0: item.quality = "Torn"; break;
                            case 1: item.quality = "Unkempt"; break;
                            case 2: item.quality = "Sturdy"; break;
                            case 3: item.quality = "Hardened"; break;
                            case 4: item.quality = "Stalwart"; break;
                        }
                        break;
                    }
                case "ranged":
                    {
                        itemValue = random.Next(0, NumberOfRanges);
                        switch (itemValue)
                        {
                            case 0: item.itemName = "bow"; item.itemSubcategory = "bow"; break;
                            case 1: item.itemName = "rifle"; item.itemSubcategory = "gun"; break;
                            case 2: item.itemName = "crossbow"; item.itemSubcategory = "crossbow"; break;
                            case 3: item.itemName = "musket"; item.itemSubcategory = "gun"; break;
                            case 4: item.itemName = "pistol"; item.itemSubcategory = "gun"; break;
                        }
                        itemValue = random.Next(0, NumberOfRangedMaterials);
                        switch (itemValue)
                        {
                            case 0: item.material = "wooden"; break;
                            case 1: item.material = "steel"; break;
                            case 2: item.material = "fiberglass"; break;
                            case 3: item.material = "plastic"; break;
                        }
                        itemValue = random.Next(0, NumberOfRangedQualities);
                        switch (itemValue)
                        {
                            case 0: item.quality = "Aweful"; break;
                            case 1: item.quality = "Steady"; break;
                            case 2: item.quality = "Trusty"; break;
                            case 3: item.quality = "Lucky"; break;
                        }
                        break;
                    }
                case "melee":
                    {
                        itemValue = random.Next(0, NumberOfMelees);
                        switch (itemValue)
                        {
                            case 0: item.itemName = "sword"; item.itemSubcategory = "sword"; break;
                            case 1: item.itemName = "axe"; item.itemSubcategory = "axe"; break;
                            case 2: item.itemName = "mace"; item.itemSubcategory = "mace"; break;
                            case 3: item.itemName = "spear"; item.itemSubcategory = "spear"; break;
                            case 4: item.itemName = "staff"; item.itemSubcategory = "unarmed"; break;
                        }
                        itemValue = random.Next(0, NumberOfMeleeMaterials);
                        switch (itemValue)
                        {
                            case 0: item.material = "wooden"; break;
                            case 1: item.material = "copper"; break;
                            case 2: item.material = "iron"; break;
                            case 3: item.material = "steel"; break;
                        }
                        itemValue = random.Next(0, NumberOfMeleeQualities);
                        switch (itemValue)
                        {
                            case 0: item.quality = "Broken"; break;
                            case 1: item.quality = "Fine"; break;
                            case 2: item.quality = "Greater"; break;
                            case 3: item.quality = "Deadly"; break;
                        }
                        break;
                    }
            }
            item.weight = 1;
            item.value = 1;
            return item;
        }
    }
}
