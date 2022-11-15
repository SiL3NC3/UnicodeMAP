using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnicodeMAP.Models
{
    public class EmojiSubgroup
    {
        public EmojiGroup Group { get; set; }
        public string Name { get; set; }

        public EmojiSubgroup(string name, EmojiGroup group)
        {
            Group = group;
            Name = name;
        }
        public override string ToString()
        {
            return $"EmojiSubgroup: {Name} [{Group}]";
        }
    }
}
