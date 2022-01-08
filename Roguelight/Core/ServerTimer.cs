using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp.DiceNotation;
namespace Roguelight.Core
{
    public class ServerTimer
    {
        public DateTime previousTime;
        public DateTime now;
        public DateTime lastMovement;
        public int SecondsSinceStart = 0;
        public long TicksSinceStart = 0;
        public long CentiSecondsSinceStart = 0;
        public long changeInTicks = 0;

        public ServerTimer()
        {
            previousTime = DateTime.Now;
            now = DateTime.Now;
        }

        public void CheckTheClock()
        {
            previousTime = now;
            now = DateTime.Now;
            CountBySeconds();
        }

        public void CountBySeconds()
        {
            if(now.Second > previousTime.Second)
            {
                SecondsSinceStart++;
                if(SecondsSinceStart % 6 == 0)
                {
                    RecoverStamina();
                    Server.SaveGame();
                }
            }
        }

        public long CountByTicks()
        {
            long changeInTicks = 0;
            if (now.Ticks > previousTime.Ticks)
            {
                changeInTicks = now.Ticks - previousTime.Ticks;    
            }
            TicksSinceStart += changeInTicks;

            return changeInTicks;
        }

        public void CountByCentiSeconds()
        {
            long onehundredk = 100000;
            CentiSecondsSinceStart = (TicksSinceStart / onehundredk);
        }

        public static void RecoverStamina()
        {
            foreach(Player player in Server.PlayerList)
            {
                int recovery = Dice.Roll($"{player.Speed}d6");
                if(player.Stamina < player.MaxStamina)
                {
                    if(player.MaxStamina - player.Stamina > recovery)
                    {
                        player.Stamina = player.Stamina + recovery;
                    }
                    else
                    {
                        player.Stamina = player.MaxStamina;
                    }
                }
            }
        }
    }
}
