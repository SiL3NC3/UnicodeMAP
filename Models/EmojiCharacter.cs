using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Unicode;

namespace UnicodeMAP.Models
{
    public class EmojiCharacter : Character
    {
        public string QualifiedStatus { get; set; }
        public string IconVersion { get; set; }

        public EmojiGroup Group { get; set; }
        public EmojiSubgroup Subgroup { get; set; }

        public override string ToString()
        {
            return $"EmojiCharacter: {Name}";
        }

        public EmojiCharacter(
            string code,
            string qualifiedStatus,
            string icon,
            string iconVersion,
            string name,
            EmojiGroup group,
            EmojiSubgroup subgroup)
        {
            ColorBlend = true;

            Code = code;
            QualifiedStatus = qualifiedStatus;
            Icon = icon;
            IconVersion = iconVersion;
            Name = name;
            Group = group;
            Subgroup = subgroup;
        }
    }
}
