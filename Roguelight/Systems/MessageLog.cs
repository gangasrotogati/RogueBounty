using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;

namespace Roguelight.Systems
{
    // Represents a queue of messages that can be added to
    // Has a method for and drawing to an RLConsole
    public class MessageLog
    {
        // Define the maximum number of lines to store
        private static readonly int _maxLines = 7;

        // Use a Queue to keep track of the lines of text
        // The first line added to the log will also be the first removed
        private readonly Queue<string[]> _lines;

        public MessageLog()
        {
            _lines = new Queue<string[]>();
        }

        // Add a line to the MessageLog queue
        public void Add(string message)
        {
            string[] chunks = message.Split('_');
            if(chunks[0] != "")
            {
                _lines.Enqueue(chunks);
            }

            // When exceeding the maximum number of lines remove the oldest one.
            if (_lines.Count > _maxLines)
            {
                _lines.Dequeue();
            }
        }

        // Draw each line of the MessageLog queue to the console
        public void Draw(RLConsole console)
        {
            RLColor messageColor = RLColor.White;
            int i = 0;
            foreach(string[] chunks in _lines)
            {
                i++;
                if(chunks.Length > 1)
                {
                    switch (chunks[1])
                    {
                        case "0": { messageColor = RLColor.White; break; }
                        case "1": { messageColor = RLColor.LightRed; break; }
                        case "2": { messageColor = RLColor.Red; break; }
                        case "3": { messageColor = RLColor.LightRed; break; }
                        case "4": { messageColor = RLColor.Red; break; }
                    }
                    console.Print(1, i + 0, chunks[0], messageColor);
                }
            }
        }
    }
}
