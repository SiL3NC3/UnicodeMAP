using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnicodeMAP.Models
{
    public class EmojiGroup
    {
        public string Name { get; set; }

        public EmojiGroup(string name)
        {
            Name = name;
        }
        public override string ToString()
        {
            return $"EmojiGroup: {Name}";
        }
    }
}
